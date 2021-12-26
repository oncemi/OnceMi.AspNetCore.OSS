/*----------------------------------------------------------------------------------
// Copyright 2019 Huawei Technologies Co.,Ltd.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License.  You may obtain a copy of the
// License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations under the License.
//----------------------------------------------------------------------------------*/
using OBS.Internal;
using OBS.Model;
using System.Net;

namespace OBS
{
    public partial class ObsClient
    {
        /// <summary>
        /// Upload an object.
        /// </summary>
        /// <param name="request">Parameters in an object upload request</param>
        /// <returns> Response to an object upload request</returns>
        public PutObjectResponse PutObject(PutObjectRequest request)
        {
            return this.DoRequest<PutObjectRequest, PutObjectResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
        }

        /// <summary>
        /// Perform an appendable upload.
        /// </summary>
        /// <param name="request">Parameters in an appendable upload request</param>
        /// <returns>Response to an appendable upload request</returns>
        public AppendObjectResponse AppendObject(AppendObjectRequest request)
        {
            return this.DoRequest<AppendObjectRequest, AppendObjectResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
        }


        /// <summary>
        /// Copy an object.
        /// </summary>
        /// <param name="request">Parameters in a request for copying an object</param>
        /// <returns> Response to an object copy request</returns>
        public CopyObjectResponse CopyObject(CopyObjectRequest request)
        {
            return this.DoRequest<CopyObjectRequest, CopyObjectResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
                if (string.IsNullOrEmpty(request.SourceBucketName))
                {
                    throw new ObsException(Constants.InvalidSourceBucketNameMessage, ErrorType.Sender, Constants.InvalidBucketName, "");
                }
                if (request.SourceObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidSourceObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
        }


        /// <summary>
        /// Upload a part.
        /// </summary>
        /// <param name="request">Parameters in a request for uploading a part</param>
        /// <returns>Response to a part upload request</returns>
        public UploadPartResponse UploadPart(UploadPartRequest request)
        {
            UploadPartResponse response = this.DoRequest<UploadPartRequest, UploadPartResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
                if (string.IsNullOrEmpty(request.UploadId))
                {
                    throw new ObsException(Constants.InvalidUploadIdMessage, ErrorType.Sender, Constants.InvalidUploadId, "");
                }

                if (request.PartNumber <= 0)
                {
                    throw new ObsException(Constants.InvalidPartNumberMessage, ErrorType.Sender, Constants.InvalidPartNumber, "");
                }
            });
            response.PartNumber = request.PartNumber;

            return response;
        }



        /// <summary>
        /// Copy a part.
        /// </summary>
        /// <param name="request">Parameters in a request for copying a part</param>
        /// <returns> Response to a part copy request</returns>
        public CopyPartResponse CopyPart(CopyPartRequest request)
        {
            CopyPartResponse response = this.DoRequest<CopyPartRequest, CopyPartResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
                if (string.IsNullOrEmpty(request.UploadId))
                {
                    throw new ObsException(Constants.InvalidUploadIdMessage, ErrorType.Sender, Constants.InvalidUploadId, "");
                }

                if (request.PartNumber <= 0)
                {
                    throw new ObsException(Constants.InvalidPartNumberMessage, ErrorType.Sender, Constants.InvalidPartNumber, "");
                }

                if (string.IsNullOrEmpty(request.SourceBucketName))
                {
                    throw new ObsException(Constants.InvalidSourceBucketNameMessage, ErrorType.Sender, Constants.InvalidBucketName, "");
                }
                if (request.SourceObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidSourceObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
            response.PartNumber = request.PartNumber;
            return response;
        }

        /// <summary>
        /// Download an object. 
        /// </summary>
        /// <param name="request">Parameters in an object download request</param>
        /// <returns>Response to an object download request</returns>
        public GetObjectResponse GetObject(GetObjectRequest request)
        {
            GetObjectResponse response = this.DoRequest<GetObjectRequest, GetObjectResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
            response.BucketName = request.BucketName;
            response.ObjectKey = request.ObjectKey;
            return response;
        }

        /// <summary>
        /// Obtain object properties.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining object properties</param>
        /// <returns>Response to a request for obtaining object properties</returns>
        public GetObjectMetadataResponse GetObjectMetadata(GetObjectMetadataRequest request)
        {
            GetObjectMetadataResponse response = this.DoRequest<GetObjectMetadataRequest, GetObjectMetadataResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
            response.BucketName = request.BucketName;
            response.ObjectKey = request.ObjectKey;
            return response;
        }


        /// <summary>
        /// Initialize a multipart upload.
        /// </summary>
        /// <param name="request">Parameters in a request for initializing a multipart upload</param>
        /// <returns>Response to a request for initializing a multipart upload</returns>

        public InitiateMultipartUploadResponse InitiateMultipartUpload(InitiateMultipartUploadRequest request)
        {
            return this.DoRequest<InitiateMultipartUploadRequest, InitiateMultipartUploadResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
        }

        /// <summary>
        /// Obtain object properties.
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <returns>Response to a request for obtaining object properties</returns>
        public GetObjectMetadataResponse GetObjectMetadata(string bucketName, string objectKey)
        {
            GetObjectMetadataRequest request = new GetObjectMetadataRequest();
            request.BucketName = bucketName;
            request.ObjectKey = objectKey;
            return this.GetObjectMetadata(request);
        }


        /// <summary>
        /// Obtain object properties.
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        /// <param name="objectKey">Object name</param>
        /// <param name="versionId">Version ID</param>
        /// <returns>Response to a request for obtaining object properties</returns>
        public GetObjectMetadataResponse GetObjectMetadata(string bucketName, string objectKey, string versionId)
        {
            GetObjectMetadataRequest request = new GetObjectMetadataRequest();
            request.BucketName = bucketName;
            request.ObjectKey = objectKey;
            request.VersionId = versionId;
            return this.GetObjectMetadata(request);
        }


        /// <summary>
        /// Combine parts.
        /// </summary>
        /// <param name="request">Parameters in a request for combining parts</param>
        /// <returns>Response to the request for combining parts</returns>
        public CompleteMultipartUploadResponse CompleteMultipartUpload(CompleteMultipartUploadRequest request)
        {
            return this.DoRequest<CompleteMultipartUploadRequest, CompleteMultipartUploadResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
                if (string.IsNullOrEmpty(request.UploadId))
                {
                    throw new ObsException(Constants.InvalidUploadIdMessage, ErrorType.Sender, Constants.InvalidUploadId, "");
                }
            });
        }


        /// <summary>
        /// Abort a multipart upload.
        /// </summary>
        /// <param name="request">Parameters in a request for aborting a multipart upload</param>
        /// <returns>Response to the request for aborting a multipart upload</returns>
        public AbortMultipartUploadResponse AbortMultipartUpload(AbortMultipartUploadRequest request)
        {
            return this.DoRequest<AbortMultipartUploadRequest, AbortMultipartUploadResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
                if (string.IsNullOrEmpty(request.UploadId))
                {
                    throw new ObsException(Constants.InvalidUploadIdMessage, ErrorType.Sender, Constants.InvalidUploadId, "");
                }
            });
        }


        /// <summary>
        /// List uploaded parts.
        /// </summary>
        /// <param name="request">Parameters in a request for listing uploaded parts</param>
        /// <returns>Response to a request for listing uploaded parts</returns>
        public ListPartsResponse ListParts(ListPartsRequest request)
        {
            return this.DoRequest<ListPartsRequest, ListPartsResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
                if (string.IsNullOrEmpty(request.UploadId))
                {
                    throw new ObsException(Constants.InvalidUploadIdMessage, ErrorType.Sender, Constants.InvalidUploadId, "");
                }
            });
        }

        /// <summary>
        /// Delete an object.
        /// </summary>
        /// <param name="request">Parameters in an object deletion request</param>
        /// <returns>Response to the object deletion request</returns>
        public DeleteObjectResponse DeleteObject(DeleteObjectRequest request)
        {
            return this.DoRequest<DeleteObjectRequest, DeleteObjectResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
        }


        /// <summary>
        /// Delete objects in a batch.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting objects in a batch</param>
        /// <returns>Response to an object batch deletion request</returns>
        public DeleteObjectsResponse DeleteObjects(DeleteObjectsRequest request)
        {
            return this.DoRequest<DeleteObjectsRequest, DeleteObjectsResponse>(request);
        }

        /// <summary>
        /// Restore an Archive object.
        /// </summary>
        /// <param name="request">Parameters in a request for restoring an Archive object</param>
        /// <returns>Response to a request for restoring an Archive object</returns>
        public RestoreObjectResponse RestoreObject(RestoreObjectRequest request)
        {
            return this.DoRequest<RestoreObjectRequest, RestoreObjectResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
        }

        /// <summary>
        /// Obtain an object ACL.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining an object ACL</param>
        /// <returns>Response to a request for obtaining an object ACL</returns>
        public GetObjectAclResponse GetObjectAcl(GetObjectAclRequest request)
        {
            return this.DoRequest<GetObjectAclRequest, GetObjectAclResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
        }


        /// <summary>
        /// Set an object ACL.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring an object ACL</param>
        /// <returns>Response to a request for configuring an object ACL</returns>
        public SetObjectAclResponse SetObjectAcl(SetObjectAclRequest request)
        {
            return this.DoRequest<SetObjectAclRequest, SetObjectAclResponse>(request, delegate ()
            {
                if (request.ObjectKey == null)
                {
                    throw new ObsException(Constants.InvalidObjectKeyMessage, ErrorType.Sender, Constants.InvalidObjectKey, "");
                }
            });
        }

        /// <summary>
        /// Querying whether a object exists.
        /// </summary>
        /// <param name="request">Parameters in a request for querying whether a object exists</param>
        /// <returns>Response to a request for querying whether a object exists</returns>
        public bool HeadObject(HeadObjectRequest request)
        {
            try
            {
                this.DoRequest<HeadObjectRequest, ObsWebServiceResponse>(request);
                return true;
            }
            catch (ObsException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw e;
            }
        }
    }
}


