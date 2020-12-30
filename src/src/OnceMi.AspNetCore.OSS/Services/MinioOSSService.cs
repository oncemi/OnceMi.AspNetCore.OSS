using EasyCaching.Core;
using Microsoft.AspNetCore.StaticFiles;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class MinioOSSService : IMinioOSSService
    {
        private readonly IEasyCachingProvider _cache;
        private readonly MinioClient _client = null;
        public OSSOptions Options { get; private set; }

        private readonly string _defaultPolicyVersion = "2012-10-17";

        public MinioClient Context
        {
            get
            {
                return this._client;
            }
        }

        public MinioOSSService(MinioClient client
            , IEasyCachingProvider provider
            , OSSOptions options)
        {
            this._client = client ?? throw new ArgumentNullException(nameof(MinioClient));
            this._cache = provider ?? throw new ArgumentNullException(nameof(IEasyCachingProvider));
            this.Options = options ?? throw new ArgumentNullException(nameof(OSSOptions));
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
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            await _client.RemoveIncompleteUploadAsync(bucketName, objectName);
            return true;
        }

        /// <summary>
        /// 列出存储桶中未完整上传的对象。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns></returns>
        public async Task<List<ItemUploadInfo>> ListIncompleteUploads(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            IObservable<Upload> observable = _client.ListIncompleteUploads(bucketName);

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
                await Task.Delay(1);
            }
            return result;
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
                return DeserializeJsonToObject<PolicyInfo>(policyJson);
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

            string policyJson = SerializeObject(info);
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
                bool isFindSource = false;
                if (item.Resource.Count == 1)
                {
                    if ((IsRootResource(bucketName, item.Resource[0]) && IsRootResource(bucketName, statement.Resource[0]))
                        || item.Resource[0].Equals(statement.Resource[0]))
                    {
                        isFindSource = true;
                    }
                }
                else
                {
                    foreach (var sourceitem in item.Resource)
                    {
                        if (sourceitem.Equals(statement.Resource[0])
                            && item.Effect.Equals(statement.Effect, StringComparison.OrdinalIgnoreCase))
                        {
                            isFindSource = true;
                        }
                    }
                }
                if (!isFindSource)
                {
                    continue;
                }
                //验证规则
                if (!item.Effect.Equals(statement.Effect))
                {
                    return false;
                }
                if (item.Action.Count < statement.Action.Count)
                {
                    return false;
                }
                foreach (var actionItem in statement.Action)
                {
                    if (!item.Action.Exists(p => p.Equals(actionItem, StringComparison.OrdinalIgnoreCase)))
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Bucket

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            return await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
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
                throw new Exception($"Bucket '{bucketName}' already exists");
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

        public async Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode)
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
                        return await this.SetPolicyAsync(bucketName, statementItems);
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
                        return await this.SetPolicyAsync(bucketName, statementItems);
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
                        return await this.SetPolicyAsync(bucketName, statementItems);
                    }
                case AccessMode.Default:
                default:
                    {
                        return await this.RemovePolicyAsync(bucketName);
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
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            List<Item> items = await ListObjectsAsync(bucketName, objectName);
            if (items != null && items.Count > 0)
            {
                Item result = items.Where(p => p.Key == objectName).FirstOrDefault();
                if (result != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
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
                    throw ex;
                },
                () =>
                {
                    isFinish = true;
                });

            while (!isFinish)
            {
                await Task.Delay(1);
            }
            return result;
        }

        public async Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            await _client.GetObjectAsync(bucketName, objectName, (stream) =>
            {
                callback(stream);
            }, null, cancellationToken);
        }

        public async Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            string path = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            await _client.GetObjectAsync(bucketName, objectName, fileName, null, cancellationToken);
        }

        public async Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            string contentType = "application/octet-stream";
            if (data is FileStream fileStream)
            {
                string fileName = fileStream.Name;
                if (!string.IsNullOrEmpty(fileName))
                {
                    new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
                }
            }
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }
            await _client.PutObjectAsync(bucketName, objectName, data, data.Length, contentType, null, null, cancellationToken);
            return true;
        }

        public async Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
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
            await _client.PutObjectAsync(bucketName, objectName, filePath, contentType, null, null, cancellationToken);
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
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
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
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (string.IsNullOrEmpty(destBucketName))
            {
                destBucketName = bucketName;
            }
            if (string.IsNullOrEmpty(destObjectName))
            {
                destObjectName = objectName;
            }

            await _client.CopyObjectAsync(bucketName, objectName, objectName, destObjectName);
            return true;
        }

        public async Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            await _client.RemoveObjectAsync(bucketName, objectName);
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

            IObservable<Minio.Exceptions.DeleteError> observable = await _client.RemoveObjectAsync(bucketName, objectNames);
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
                await Task.Delay(1);
            }
            if (removeFailed.Count > 0)
            {
                if (removeFailed.Count == objectNames.Count)
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
        public async Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return await PresignedObjectAsync(bucketName, objectName, expiresInt, PresignedObjectType.Get);
        }

        public async Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return await PresignedObjectAsync(bucketName, objectName, expiresInt, PresignedObjectType.Put);
        }

        /// <summary>
        /// 清除临时连接缓存
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        public async void RemovePresignedUrlCache(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (Options.IsEnableCache)
            {
                string key = Encrypt.MD5($"{bucketName}_{objectName}_{PresignedObjectType.Put.ToString().ToUpper()}");
                await _cache.RemoveAsync(key);
                key = Encrypt.MD5($"{bucketName}_{objectName}_{PresignedObjectType.Get.ToString().ToUpper()}");
                await _cache.RemoveAsync(key);
            }
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

        public async Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
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
                        return await this.SetPolicyAsync(bucketName, statementItems);
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
                        return await this.SetPolicyAsync(bucketName, statementItems);
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
                        return await this.SetPolicyAsync(bucketName, statementItems);
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
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
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
            if (string.IsNullOrEmpty(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
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
                if(!await SetPolicyAsync(bucketName, statements))
                {
                    throw new Exception("Save new policy info failed when remove object acl.");
                }
            }
            return await GetObjectAclAsync(bucketName, objectName);
        }

        #endregion

        #region Private

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="expiresInt"></param>
        /// <param name="type">0为get，1为put</param>
        /// <returns></returns>
        private async Task<string> PresignedObjectAsync(string bucketName, string objectName, int expiresInt, PresignedObjectType type)
        {
            try
            {
                if (string.IsNullOrEmpty(bucketName))
                {
                    throw new ArgumentNullException(nameof(bucketName));
                }
                if (string.IsNullOrEmpty(objectName))
                {
                    throw new ArgumentNullException(nameof(objectName));
                }
                if (expiresInt <= 0)
                {
                    throw new Exception("ExpiresIn time can not less than 0.");
                }
                if (expiresInt > 7 * 24 * 3600)
                {
                    throw new Exception("ExpiresIn time no more than 7 days.");
                }
                const int minExpiresInt = 600;

                if (Options.IsEnableCache && expiresInt > minExpiresInt)
                {
                    string key = Encrypt.MD5($"{bucketName}_{objectName}_{type.ToString().ToUpper()}");
                    var cacheResult = await _cache.GetAsync<PresignedUrlCache>(key);
                    PresignedUrlCache cache = cacheResult.HasValue ? cacheResult.Value : null;
                    //Unix时间
                    long nowTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                    //缓存中存在，且有效时间不低于10分钟
                    if (cache != null
                        && cache.Type == type
                        && cache.CreateTime > 0
                        && (cache.CreateTime + expiresInt - nowTime) > minExpiresInt
                        && cache.Name == objectName
                        && cache.BucketName == bucketName)
                    {
                        return cache.Url;
                    }
                    else
                    {
                        if (type == PresignedObjectType.Get && !await this.ObjectsExistsAsync(bucketName, objectName))
                        {
                            throw new Exception($"Object '{objectName}' not in bucket '{bucketName}'.");
                        }
                        PresignedPutObjectArgs args = new PresignedPutObjectArgs()
                            .WithBucket(bucketName)
                            .WithObject(objectName)
                            .WithExpiry(expiresInt);
                        string presignedUrl = type == PresignedObjectType.Get ? await _client.PresignedGetObjectAsync(bucketName, objectName, expiresInt)
                            : await _client.PresignedPutObjectAsync(args);
                        if (string.IsNullOrEmpty(presignedUrl))
                        {
                            throw new Exception("Result url is null.");
                        }
                        PresignedUrlCache urlCache = new PresignedUrlCache()
                        {
                            Url = presignedUrl,
                            CreateTime = nowTime,
                            Name = objectName,
                            BucketName = bucketName,
                            Type = type
                        };
                        int randomSec = new Random().Next(5, 30);
                        await _cache.SetAsync(key, urlCache, TimeSpan.FromSeconds(expiresInt + randomSec));
                        return urlCache.Url;
                    }
                }
                else
                {
                    if (type == 0 && !await this.ObjectsExistsAsync(bucketName, objectName))
                    {
                        throw new Exception($"Object '{objectName}' not in bucket '{bucketName}'.");
                    }
                    PresignedPutObjectArgs args = new PresignedPutObjectArgs()
                            .WithBucket(bucketName)
                            .WithObject(objectName)
                            .WithExpiry(expiresInt);
                    string presignedUrl = type == PresignedObjectType.Get ? await _client.PresignedGetObjectAsync(bucketName, objectName, expiresInt)
                        : await _client.PresignedPutObjectAsync(args);
                    return presignedUrl;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Presigned {(type == PresignedObjectType.Get ? "get" : "put")} url for object '{objectName}' from {bucketName} failed. {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        private string SerializeObject(object o)
        {
            string json = JsonConvert.SerializeObject(o);
            return json;
        }

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

        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        private T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }
        #endregion
    }
}
