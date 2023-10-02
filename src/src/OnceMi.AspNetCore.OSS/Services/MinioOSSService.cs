using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Result;
using Minio.Exceptions;
using OnceMi.AspNetCore.OSS.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class MinioOSSService : BaseOSSService, IMinioOSSService
    {
        private readonly IMinioClient _client = null;
        private readonly string _defaultPolicyVersion = "2012-10-17";

        public IMinioClient Context
        {
            get
            {
                return this._client;
            }
        }

        public MinioOSSService(ICacheProvider cache, OSSOptions options)
            : base(cache, options)
        {
            IMinioClient client = new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithRegion(options.Region)
                .WithCredentials(options.AccessKey, options.SecretKey);
            if (options.IsEnableHttps)
            {
                client = client.WithSSL();
            }
            this._client = client.Build();
        }

        #region Minio自有方法

        /// <summary>
        /// 删除一个未完整上传的对象。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">存储桶里的对象名称。</param>
        /// <returns></returns>
        public async Task<bool> RemoveIncompleteUploadAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            RemoveIncompleteUploadArgs args = new RemoveIncompleteUploadArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);
            await _client.RemoveIncompleteUploadAsync(args);
            return true;
        }

        /// <summary>
        /// 列出存储桶中未完整上传的对象。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns></returns>
        public Task<List<ItemUploadInfo>> ListIncompleteUploads(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            ListIncompleteUploadsArgs args = new ListIncompleteUploadsArgs()
                .WithBucket(bucketName);
            IObservable<Upload> observable = _client.ListIncompleteUploads(args);

            bool isFinish = false;
            List<ItemUploadInfo> result = new List<ItemUploadInfo>();

            IDisposable subscription = observable.Subscribe(
                item =>
                {
                    result.Add(new ItemUploadInfo()
                    {
                        Key = item.Key,
                        Initiated = item.Initiated,
                        UploadId = item.UploadId,
                    });
                },
                ex =>
                {
                    isFinish = true;
                    throw new Exception(ex.Message, ex);
                },
                () =>
                {
                    isFinish = true;
                });
            while (!isFinish)
            {
                Thread.Sleep(0);
            }
            return Task.FromResult(result);
        }

        /// <summary>
        /// 获取存储桶的权限
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns></returns>
        public async Task<PolicyInfo> GetPolicyAsync(string bucketName)
        {
            try
            {
                if (string.IsNullOrEmpty(bucketName))
                {
                    throw new ArgumentNullException(nameof(bucketName));
                }

                var args = new GetPolicyArgs()
                    .WithBucket(bucketName);
                string policyJson = await _client.GetPolicyAsync(args);
                if (string.IsNullOrEmpty(policyJson))
                {
                    throw new Exception("Result policy json is null.");
                }
                return JsonUtil.DeserializeObject<PolicyInfo>(policyJson);
            }
            catch (MinioException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message) && ex.Message.ToLower().Contains("the bucket policy does not exist"))
                {
                    return new PolicyInfo()
                    {
                        Version = _defaultPolicyVersion,
                        Statement = new List<StatementItem>()
                    };
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 设置存储桶的权限
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="statements">权限条目</param>
        /// <returns></returns>
        public async Task<bool> SetPolicyAsync(string bucketName, List<StatementItem> statements)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (statements == null || statements.Count == 0)
            {
                throw new ArgumentNullException(nameof(PolicyInfo));
            }

            List<StatementItem> oldStatements = null;
            List<StatementItem> addStatements = statements;
            List<StatementItem> tempStatements = new List<StatementItem>();
            //获取原有的
            PolicyInfo info = await GetPolicyAsync(bucketName);
            if (info.Statement == null)
            {
                info.Statement = new List<StatementItem>();
            }
            else
            {
                oldStatements = UnpackResource(info.Statement);
            }
            //解析要添加的条目，将包含多条Resource的条目解析为仅包含一条条目的数据
            statements = UnpackResource(statements);
            //验证要添加的数据
            foreach (var addItem in statements)
            {
                if (!addItem.Effect.Equals("Allow", StringComparison.OrdinalIgnoreCase)
                    && !addItem.Effect.Equals("Deny", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Add statement effect only support 'Allow' or 'Deny'.");
                }
                if (addItem.Action == null || addItem.Action.Count == 0)
                {
                    throw new Exception("Add statement action can not null");
                }
                if (addItem.Resource == null || addItem.Resource.Count == 0)
                {
                    throw new Exception("Add statement resource can not null");
                }
                if (addItem.Principal == null || addItem.Principal.AWS == null || addItem.Principal.AWS.Count == 0)
                {
                    addItem.Principal = new Principal()
                    {
                        AWS = new List<string>()
                        {
                            "*"
                        }
                    };
                }
            }
            if (oldStatements == null || oldStatements.Count == 0)
            {
                //没有Policy数据的情况，新建，修改或删除
                foreach (var addItem in statements)
                {
                    //跳过删除
                    if (addItem.IsDelete)
                    {
                        continue;
                    }
                    tempStatements.Add(addItem);
                }
            }
            else
            {
                foreach (var addItem in addStatements)
                {
                    foreach (var oldItem in oldStatements)
                    {
                        //判断已经存在的条目是否包含现有要添加的条目
                        //如果存在条目，则更新；不存在条目，添加进去
                        if ((IsRootResource(bucketName, oldItem.Resource[0]) && IsRootResource(bucketName, addItem.Resource[0]))
                        || oldItem.Resource[0].Equals(addItem.Resource[0], StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            oldItem.IsDelete = true;  //就记录标识为删除，不重新添加到待添加列表中
                        }
                    }
                    if (!addItem.IsDelete)
                    {
                        tempStatements.Add(addItem);
                    }
                }
                foreach (var oldItem in oldStatements)
                {
                    if (!oldItem.IsDelete)
                    {
                        tempStatements.Add(oldItem);
                    }
                }
            }
            //reset info
            info.Version = _defaultPolicyVersion;
            info.Statement = tempStatements;

            string policyJson = JsonUtil.SerializeObject(info);
            await _client.SetPolicyAsync(new SetPolicyArgs()
                .WithBucket(bucketName)
                .WithPolicy(policyJson));
            return true;
        }

        /// <summary>
        /// 移除全部存储桶的权限
        /// 如果要单独移除某个桶的权限，可以使用SetPolicyAsync，并将StatementItem中的IsDelete设置为true
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns></returns>
        public async Task<bool> RemovePolicyAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            var args = new RemovePolicyArgs().WithBucket(bucketName);
            await _client.RemovePolicyAsync(args);
            return true;
        }

        public async Task<bool> PolicyExistsAsync(string bucketName, StatementItem statement)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (statement == null
                || string.IsNullOrEmpty(statement.Effect)
                || (statement.Action == null || statement.Action.Count == 0)
                || (statement.Resource == null || statement.Resource.Count == 0))
            {
                throw new ArgumentNullException(nameof(StatementItem));
            }
            PolicyInfo info = await GetPolicyAsync(bucketName);
            if (info.Statement == null || info.Statement.Count == 0)
            {
                return false;
            }
            if (statement.Resource.Count > 1)
            {
                throw new Exception("Only support one resource.");
            }
            foreach (var item in info.Statement)
            {
                bool result = true;
                bool findSource = false;
                if (item.Resource.Count == 1)
                {
                    if ((IsRootResource(bucketName, item.Resource[0]) && IsRootResource(bucketName, statement.Resource[0]))
                        || item.Resource[0].Equals(statement.Resource[0]))
                    {
                        findSource = true;
                    }
                }
                else
                {
                    foreach (var sourceitem in item.Resource)
                    {
                        if (sourceitem.Equals(statement.Resource[0])
                            && item.Effect.Equals(statement.Effect, StringComparison.OrdinalIgnoreCase))
                        {
                            findSource = true;
                        }
                    }
                }
                if (!findSource) continue;
                //验证规则
                if (!item.Effect.Equals(statement.Effect))
                {
                    //访问权限
                    continue;
                }
                if (item.Action.Count < statement.Action.Count)
                {
                    //动作，如果存在的条目数量少于要验证的，false
                    continue;
                }
                foreach (var actionItem in statement.Action)
                {
                    //验证action
                    if (!item.Action.Any(p => p.Equals(actionItem, StringComparison.OrdinalIgnoreCase)))
                    {
                        result = false;
                    }
                }
                if (result)
                {
                    return result;
                }
            }
            return false;
        }

        #endregion

        #region Bucket

        public Task<bool> BucketExistsAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            var args = new BucketExistsArgs().WithBucket(bucketName);
            return _client.BucketExistsAsync(args);
        }

        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            bool found = await BucketExistsAsync(bucketName);
            if (found)
            {
                throw new BucketExistException($"Bucket '{bucketName}' already exists.");
            }
            else
            {
                await _client.MakeBucketAsync(
                    new MakeBucketArgs()
                        .WithBucket(bucketName)
                        .WithLocation(Options.Region));
                return true;
            }
        }

        public async Task<List<Bucket>> ListBucketsAsync()
        {
            ListAllMyBucketsResult list = await _client.ListBucketsAsync();
            if (list == null)
            {
                throw new Exception("List buckets failed, result obj is null");
            }
            List<Bucket> result = new List<Bucket>();
            foreach (var item in list.Buckets)
            {
                result.Add(new Bucket()
                {
                    Name = item.Name,
                    Location = Options.Region,
                    CreationDate = item.CreationDate,
                    Owner = new Owner()
                    {
                        Id = Options.AccessKey,
                        Name = Options.AccessKey,
                    }
                });
            }
            return result;
        }

        public async Task<bool> RemoveBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            bool found = await BucketExistsAsync(bucketName);
            if (!found)
            {
                return true;
            }
            else
            {
                await _client.RemoveBucketAsync(new RemoveBucketArgs().WithBucket(bucketName));
                return true;
            }
        }

        public Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            List<StatementItem> statementItems = new List<StatementItem>();
            switch (mode)
            {
                case AccessMode.Private:
                    {
                        statementItems.Add(new StatementItem()
                        {
                            Effect = "Deny",
                            Principal = new Principal()
                            {
                                AWS = new List<string>()
                                {
                                    "*"
                                }
                            },
                            Action = new List<string>()
                            {
                                "s3:DeleteObject",
                                "s3:GetObject",
                                "s3:ListBucket",
                                "s3:PutObject"
                            },
                            Resource = new List<string>()
                            {
                                "arn:aws:s3:::*",
                            },
                            IsDelete = false
                        });

                        return this.SetPolicyAsync(bucketName, statementItems);
                    }
                case AccessMode.PublicRead:
                    {
                        //允许列出和下载
                        statementItems.Add(new StatementItem()
                        {
                            Effect = "Allow",
                            Principal = new Principal()
                            {
                                AWS = new List<string>()
                                {
                                    "*"
                                }
                            },
                            Action = new List<string>()
                            {
                                "s3:GetObject",
                                "s3:ListBucket"
                            },
                            Resource = new List<string>()
                            {
                                "arn:aws:s3:::*",
                            },
                            IsDelete = false
                        });
                        //禁止删除和修改
                        statementItems.Add(new StatementItem()
                        {
                            Effect = "Deny",
                            Principal = new Principal()
                            {
                                AWS = new List<string>()
                                {
                                    "*"
                                }
                            },
                            Action = new List<string>()
                            {
                                "s3:DeleteObject",
                                "s3:PutObject"
                            },
                            Resource = new List<string>()
                            {
                                "arn:aws:s3:::*",
                            },
                            IsDelete = false
                        });
                        return this.SetPolicyAsync(bucketName, statementItems);
                    }
                case AccessMode.PublicReadWrite:
                    {
                        statementItems.Add(new StatementItem()
                        {
                            Effect = "Allow",
                            Principal = new Principal()
                            {
                                AWS = new List<string>()
                                {
                                    "*"
                                }
                            },
                            Action = new List<string>()
                            {
                                "s3:DeleteObject",
                                "s3:GetObject",
                                "s3:ListBucket",
                                "s3:PutObject"
                            },
                            Resource = new List<string>()
                            {
                                "arn:aws:s3:::*",
                            },
                            IsDelete = false
                        });
                        return this.SetPolicyAsync(bucketName, statementItems);
                    }
                case AccessMode.Default:
                default:
                    {
                        return this.RemovePolicyAsync(bucketName);
                    }
            }
        }

        public async Task<AccessMode> GetBucketAclAsync(string bucketName)
        {
            bool FindAction(List<string> actions, string input)
            {
                if (actions != null && actions.Count > 0 && actions.Exists(p => p.Equals(input, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
                return false;
            }

            PolicyInfo info = await GetPolicyAsync(bucketName);
            if (info == null)
            {
                return AccessMode.Default;
            }
            if (info.Statement == null || info.Statement.Count == 0)
            {
                return AccessMode.Private;
            }
            List<StatementItem> statements = UnpackResource(info.Statement);

            bool isPublicRead = false;
            bool isPublicWrite = false;
            foreach (var item in statements)
            {
                if (!IsRootResource(bucketName, item.Resource[0]))
                {
                    continue;
                }
                if (item.Action == null || item.Action.Count == 0)
                {
                    continue;
                }
                if (item.Effect.Equals("Allow", StringComparison.OrdinalIgnoreCase))
                {
                    if (FindAction(item.Action, "*"))
                    {
                        return AccessMode.PublicReadWrite;
                    }
                    if (FindAction(item.Action, "s3:GetObject"))
                    {
                        isPublicRead = true;
                    }
                    if (FindAction(item.Action, "s3:PutObject"))
                    {
                        isPublicWrite = true;
                    }
                }
                if (isPublicRead && isPublicWrite)
                {
                    return AccessMode.PublicReadWrite;
                }
            }
            //结果
            if (isPublicRead && !isPublicWrite)
            {
                return AccessMode.PublicRead;
            }
            else if (isPublicRead && isPublicWrite)
            {
                return AccessMode.PublicReadWrite;
            }
            else if (!isPublicRead && isPublicWrite)
            {
                return AccessMode.Private;
            }
            else
            {
                return AccessMode.Private;
            }
        }

        #endregion

        #region Object

        public async Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            try
            {
                var result = await GetObjectMetadataAsync(bucketName, objectName);
                return result != null;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            IObservable<Minio.DataModel.Item> observable = _client.ListObjectsAsync(
                new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix(prefix)
                    .WithRecursive(true));
            List<Item> result = new List<Item>();
            bool isFinish = false;

            IDisposable subscription = observable.Subscribe(
                item =>
                {
                    result.Add(new Item()
                    {
                        Key = item.Key,
                        LastModified = item.LastModified,
                        ETag = item.ETag,
                        Size = item.Size,
                        BucketName = bucketName,
                        IsDir = item.IsDir,
                        LastModifiedDateTime = item.LastModifiedDateTime
                    });
                },
                ex =>
                {
                    isFinish = true;
                },
                () =>
                {
                    isFinish = true;
                });

            while (!isFinish)
            {
                Thread.Sleep(0);
            }
            return Task.FromResult(result);
        }

        public async Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            GetObjectArgs args = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream((stream) =>
                {
                    callback(stream);
                });
            _ = await _client.GetObjectAsync(args, cancellationToken);
        }

        public async Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            string fullPath = Path.GetFullPath(fileName);
            string parentPath = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(parentPath) && !Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            objectName = FormatObjectName(objectName);
            GetObjectArgs args = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream((stream) =>
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                        stream.Dispose();
                        fs.Close();
                    }
                });
            _ = await _client.GetObjectAsync(args, cancellationToken);
        }

        public async Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            string contentType = "application/octet-stream";
            if (data is FileStream fileStream)
            {
                string fileName = fileStream.Name;
                if (!string.IsNullOrEmpty(fileName))
                {
                    new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
                }
            }
            else
            {
                new FileExtensionContentTypeProvider().TryGetContentType(objectName, out contentType);
            }
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }
            PutObjectArgs args = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(data)
                .WithObjectSize(data.Length)
                .WithContentType(contentType);
            await _client.PutObjectAsync(args, cancellationToken);
            return true;
        }

        public async Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (!File.Exists(filePath))
            {
                throw new Exception("File not exist.");
            }
            string fileName = Path.GetFileName(filePath);
            string contentType = null;
            if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            PutObjectArgs args = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(filePath)
                .WithContentType(contentType);
            await _client.PutObjectAsync(args, cancellationToken);
            return true;
        }

        public async Task<ItemMeta> GetObjectMetadataAsync(string bucketName
            , string objectName
            , string versionID = null
            , string matchEtag = null
            , DateTime? modifiedSince = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            StatObjectArgs args = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithVersionId(versionID)
                .WithMatchETag(matchEtag);
            if (modifiedSince.HasValue)
            {
                args = args.WithModifiedSince(modifiedSince.Value);
            }
            ObjectStat statObject = await _client.StatObjectAsync(args);

            return new ItemMeta()
            {
                ObjectName = statObject.ObjectName,
                Size = statObject.Size,
                LastModified = statObject.LastModified,
                ETag = statObject.ETag,
                ContentType = statObject.ContentType,
                IsEnableHttps = Options.IsEnableHttps,
                MetaData = statObject.MetaData
            };
        }

        public async Task<bool> CopyObjectAsync(string bucketName, string objectName, string destBucketName = null, string destObjectName = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (string.IsNullOrEmpty(destBucketName))
            {
                destBucketName = bucketName;
            }
            destObjectName = FormatObjectName(destObjectName);
            CopySourceObjectArgs cpSrcArgs = new CopySourceObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);
            CopyObjectArgs args = new CopyObjectArgs()
                .WithBucket(destBucketName)
                .WithObject(destObjectName)
                .WithCopyObjectSource(cpSrcArgs);
            await _client.CopyObjectAsync(args);
            return true;
        }

        public async Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            RemoveObjectArgs args = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);
            await _client.RemoveObjectAsync(args);
            return true;
        }

        public async Task<bool> RemoveObjectAsync(string bucketName, List<string> objectNames)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (objectNames == null || objectNames.Count == 0)
            {
                throw new ArgumentNullException(nameof(objectNames));
            }
            List<string> delObjects = new List<string>();
            foreach (var item in objectNames)
            {
                delObjects.Add(FormatObjectName(item));
            }
            RemoveObjectsArgs args = new RemoveObjectsArgs()
                .WithBucket(bucketName)
                .WithObjects(delObjects);
            IObservable<Minio.Exceptions.DeleteError> observable = await _client.RemoveObjectsAsync(args);
            List<string> removeFailed = new List<string>();

            bool isFinish = false;
            IDisposable subscription = observable.Subscribe(
               err =>
               {
                   removeFailed.Add(err.Key);
               },
               ex =>
               {
                   isFinish = true;
                   throw ex;
               },
               () =>
               {
                   isFinish = true;
               });
            while (!isFinish)
            {
                Thread.Sleep(0);
            }
            if (removeFailed.Count > 0)
            {
                if (removeFailed.Count == delObjects.Count)
                {
                    throw new Exception("Remove all object failed.");
                }
                else
                {
                    throw new Exception($"Remove objects '{string.Join(",", removeFailed)}' from {bucketName} failed.");
                }
            }
            return true;
        }

        /// <summary>
        /// 生成一个临时连接
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="expiresInt"></param>
        /// <returns></returns>
        public Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName
                , objectName
                , expiresInt
                , PresignedObjectType.Get
                , async (bucketName, objectName, expiresInt) =>
                {
                    objectName = FormatObjectName(objectName);
                    //生成URL
                    AccessMode accessMode = await this.GetObjectAclAsync(bucketName, objectName);
                    if (accessMode == AccessMode.PublicRead || accessMode == AccessMode.PublicReadWrite)
                    {
                        return $"{(Options.IsEnableHttps ? "https" : "http")}://{Options.Endpoint}/{bucketName}{(objectName.StartsWith("/") ? objectName : $"/{objectName}")}";
                    }
                    else
                    {
                        PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                            .WithBucket(bucketName)
                            .WithObject(objectName)
                            .WithExpiry(expiresInt);
                        return await _client.PresignedGetObjectAsync(args);
                    }
                });
        }

        public Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName
                , objectName
                , expiresInt
                , PresignedObjectType.Put
                , async (bucketName, objectName, expiresInt) =>
                {
                    objectName = FormatObjectName(objectName);
                    //生成URL
                    PresignedPutObjectArgs args = new PresignedPutObjectArgs()
                            .WithBucket(bucketName)
                            .WithObject(objectName)
                            .WithExpiry(expiresInt);
                    return await _client.PresignedPutObjectAsync(args);
                });
        }

        /// <summary>
        /// 将应用程序详细信息添加到User-Agent。
        /// </summary>
        /// <param name="appName">执行API请求的应用程序的名称</param>
        /// <param name="appVersion">执行API请求的应用程序的版本</param>
        /// <returns></returns>
        public Task SetAppInfo(string appName, string appVersion)
        {
            if (string.IsNullOrEmpty(appName))
            {
                throw new ArgumentNullException(nameof(appName));
            }
            if (string.IsNullOrEmpty(appVersion))
            {
                throw new ArgumentNullException(nameof(appVersion));
            }
            _client.SetAppInfo(appName, appVersion);
            return Task.FromResult(true);
        }

        public Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (!objectName.StartsWith(bucketName))
            {
                objectName = $"{bucketName}/{objectName}";
            }
            List<StatementItem> statementItems = new List<StatementItem>();
            switch (mode)
            {
                case AccessMode.Private:
                    {
                        statementItems.Add(new StatementItem()
                        {
                            Effect = "Deny",
                            Principal = new Principal()
                            {
                                AWS = new List<string>()
                                {
                                    "*"
                                }
                            },
                            Action = new List<string>()
                            {
                                "s3:DeleteObject",
                                "s3:GetObject",
                                "s3:PutObject"
                            },
                            Resource = new List<string>()
                            {
                                $"arn:aws:s3:::{objectName}",
                            },
                            IsDelete = false
                        });
                        return this.SetPolicyAsync(bucketName, statementItems);
                    }
                case AccessMode.PublicRead:
                    {
                        //允许列出和下载
                        statementItems.Add(new StatementItem()
                        {
                            Effect = "Allow",
                            Principal = new Principal()
                            {
                                AWS = new List<string>()
                                {
                                    "*"
                                }
                            },
                            Action = new List<string>()
                            {
                                "s3:GetObject"
                            },
                            Resource = new List<string>()
                            {
                                $"arn:aws:s3:::{objectName}",
                            },
                            IsDelete = false
                        });
                        //禁止删除和修改
                        statementItems.Add(new StatementItem()
                        {
                            Effect = "Deny",
                            Principal = new Principal()
                            {
                                AWS = new List<string>()
                                {
                                    "*"
                                }
                            },
                            Action = new List<string>()
                            {
                                "s3:DeleteObject",
                                "s3:PutObject"
                            },
                            Resource = new List<string>()
                            {
                                $"arn:aws:s3:::{objectName}",
                            },
                            IsDelete = false
                        });
                        return this.SetPolicyAsync(bucketName, statementItems);
                    }
                case AccessMode.PublicReadWrite:
                    {
                        statementItems.Add(new StatementItem()
                        {
                            Effect = "Allow",
                            Principal = new Principal()
                            {
                                AWS = new List<string>()
                                {
                                    "*"
                                }
                            },
                            Action = new List<string>()
                            {
                                "s3:DeleteObject",
                                "s3:GetObject",
                                "s3:PutObject"
                            },
                            Resource = new List<string>()
                            {
                                $"arn:aws:s3:::{objectName}",
                            },
                            IsDelete = false
                        });
                        return this.SetPolicyAsync(bucketName, statementItems);
                    }
                case AccessMode.Default:
                default:
                    {
                        throw new ArgumentNullException($"Unsupport access mode '{mode}'");
                    }
            }
        }

        public async Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName)
        {
            bool FindAction(List<string> actions, string input)
            {
                if (actions != null && actions.Count > 0 && actions.Exists(p => p.Equals(input, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
                return false;
            }

            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (!objectName.StartsWith(bucketName))
            {
                objectName = $"{bucketName}/{objectName}";
            }
            //获取存储桶默认权限
            AccessMode bucketMode = await GetBucketAclAsync(bucketName);
            PolicyInfo info = await GetPolicyAsync(bucketName);
            if (info == null || info.Statement == null || info.Statement.Count == 0)
            {
                return bucketMode;
            }
            List<StatementItem> statements = UnpackResource(info.Statement);

            bool isPublicRead = false;
            bool isPublicWrite = false;
            switch (bucketMode)
            {
                case AccessMode.PublicRead:
                    {
                        isPublicRead = true;
                        isPublicWrite = false;
                        break;
                    }
                case AccessMode.PublicReadWrite:
                    {
                        isPublicRead = true;
                        isPublicWrite = true;
                        break;
                    }
                case AccessMode.Default:
                case AccessMode.Private:
                default:
                    {
                        isPublicRead = false;
                        isPublicWrite = false;
                        break;
                    }
            }

            foreach (var item in statements)
            {
                if (!item.Resource[0].Equals($"arn:aws:s3:::{objectName}")
                    && !item.Resource[0].Equals($"{objectName}"))
                {
                    continue;
                }
                if (item.Action == null || item.Action.Count == 0)
                {
                    continue;
                }
                if (item.Effect.Equals("Allow", StringComparison.OrdinalIgnoreCase))
                {
                    if (FindAction(item.Action, "*"))
                    {
                        return AccessMode.PublicReadWrite;
                    }
                    if (FindAction(item.Action, "s3:GetObject"))
                    {
                        isPublicRead = true;
                    }
                    if (FindAction(item.Action, "s3:PutObject"))
                    {
                        isPublicWrite = true;
                    }
                }
                else if (item.Effect.Equals("Deny", StringComparison.OrdinalIgnoreCase))
                {
                    if (FindAction(item.Action, "*"))
                    {
                        return AccessMode.Private;
                    }
                    if (FindAction(item.Action, "s3:GetObject"))
                    {
                        isPublicRead = false;
                    }
                    if (FindAction(item.Action, "s3:PutObject"))
                    {
                        isPublicWrite = false;
                    }
                }
            }
            //结果
            if (isPublicRead && !isPublicWrite)
            {
                return AccessMode.PublicRead;
            }
            else if (isPublicRead && isPublicWrite)
            {
                return AccessMode.PublicReadWrite;
            }
            else if (!isPublicRead && isPublicWrite)
            {
                return AccessMode.Private;
            }
            else
            {
                return AccessMode.Private;
            }
        }

        public async Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            objectName = FormatObjectName(objectName);
            if (!objectName.StartsWith(bucketName))
            {
                objectName = $"{bucketName}/{objectName}";
            }
            PolicyInfo info = await GetPolicyAsync(bucketName);
            if (info == null || info.Statement == null || info.Statement.Count == 0)
            {
                return await GetObjectAclAsync(bucketName, objectName);
            }
            List<StatementItem> statements = UnpackResource(info.Statement);
            bool hasUpdate = false;
            foreach (var item in statements)
            {
                if (item.Resource[0].Equals($"arn:aws:s3:::{objectName}")
                    || item.Resource[0].Equals($"{objectName}"))
                {
                    hasUpdate = true;
                    item.IsDelete = true;
                }
            }
            if (hasUpdate)
            {
                if (!await SetPolicyAsync(bucketName, statements))
                {
                    throw new Exception("Save new policy info failed when remove object acl.");
                }
            }
            return await GetObjectAclAsync(bucketName, objectName);
        }

        #region private

        private List<StatementItem> UnpackResource(List<StatementItem> source)
        {
            List<StatementItem> dest = new List<StatementItem>();
            if (source == null || source.Count == 0)
            {
                return dest;
            }
            foreach (var item in source)
            {
                if (item.Resource == null || item.Resource.Count == 0)
                {
                    continue;
                }
                else if (item.Resource.Count > 0)
                {
                    foreach (var resourceItem in item.Resource)
                    {
                        StatementItem newItem = new StatementItem()
                        {
                            Effect = item.Effect,
                            Principal = item.Principal,
                            Action = item.Action,
                            Resource = new List<string>()
                            {
                                resourceItem
                            },
                            IsDelete = item.IsDelete
                        };
                        dest.Add(newItem);
                    }
                }
                else
                {
                    dest.Add(item);
                }
            }
            return dest;
        }

        private bool IsRootResource(string bucketName, string resource)
        {
            if (resource.StartsWith("*", StringComparison.OrdinalIgnoreCase)
                || resource.StartsWith("arn:aws:s3:::*", StringComparison.OrdinalIgnoreCase)
                || resource.StartsWith($"arn:aws:s3:::{bucketName}*", StringComparison.OrdinalIgnoreCase)
                || resource.StartsWith($"arn:aws:s3:::{bucketName}/*", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        #endregion

        #endregion
    }
}
