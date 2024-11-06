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
using System;
using System.Net;
using OBS.Model;


namespace OBS
{
    public partial class ObsClient
    {
        /// <summary>
        /// Obtain the bucket list. In the list, bucket names are displayed in lexicographical order.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket list</param>
        /// <returns>Response to the request for obtaining the bucket list</returns>
        public ListBucketsResponse ListBuckets(ListBucketsRequest request)
        {
            return this.DoRequest<ListBucketsRequest, ListBucketsResponse>(request);
        }

        /// <summary>
        /// Create a bucket.
        /// A bucket name must be unique in OBS.
        /// If a user repeatedly creates buckets with the same name in one region, status code "200" is returned.
        /// In other cases, status code "409" is returned. Each user can create a maximum of 100 buckets.
        /// </summary>
        /// <param name="request">Parameters in the bucket creation request</param>
        /// 
        /// <returns>Response to the bucket creation request</returns>
        public CreateBucketResponse CreateBucket(CreateBucketRequest request)
        {
            return this.DoRequest<CreateBucketRequest, CreateBucketResponse>(request);
        }

        /// <summary>
        /// Check whether a bucket exists. If the returned HTTP status code is "200", the bucket exists. If the returned HTTP status code is "404", the bucket does not exist.
        /// </summary>
        /// <param name="request">Parameters in a request for checking whether a bucket exists</param>
        /// 
        /// <returns>Response to the request for querying whether a bucket exists</returns>
        /// 
        public bool HeadBucket(HeadBucketRequest request)
        {
            try
            {
                this.DoRequest<HeadBucketRequest, ObsWebServiceResponse>(request);
                return true;
            }catch(ObsException e)
            {
                if(e.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw e;
            }
        }

        /// <summary>
        /// Obtain bucket metadata.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket metadata</param>
        /// 
        /// <returns> Response to a request for obtaining bucket metadata</returns>
        /// 
        public GetBucketMetadataResponse GetBucketMetadata(GetBucketMetadataRequest request)
        {
            return DoRequest<GetBucketMetadataRequest, GetBucketMetadataResponse>(request);
        }


        /// <summary>
        /// Set the bucket quota.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring a bucket quota</param>
        /// <returns>Response to a request for configuring a bucket quota</returns>
        public SetBucketQuotaResponse SetBucketQuota(SetBucketQuotaRequest request)
        {
            return DoRequest<SetBucketQuotaRequest, SetBucketQuotaResponse>(request);
        }



        /// <summary>
        /// Set the bucket ACL.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring a bucket ACL</param>
        /// 
        /// <returns>Response to a request for configuring a bucket ACL</returns>
        public SetBucketAclResponse SetBucketAcl(SetBucketAclRequest request)
        {
            return DoRequest<SetBucketAclRequest, SetBucketAclResponse>(request);
        }


        /// <summary>
        /// Obtain the bucket location.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket location</param>
        /// <returns>Response to a request for obtaining the bucket location</returns>
        public GetBucketLocationResponse GetBucketLocation(GetBucketLocationRequest request)
        {
            return DoRequest<GetBucketLocationRequest, GetBucketLocationResponse>(request);
        }


        /// <summary>
        /// List objects in a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for listing objects in a bucket</param>
        /// <returns>Response to a request for listing objects in a bucket</returns>
        public ListObjectsResponse ListObjects(ListObjectsRequest request)
        {
            return DoRequest<ListObjectsRequest, ListObjectsResponse>(request);
        }

        /// <summary>
        /// List versioning objects in a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for listing versioning objects in a bucket</param>
        /// <returns>Response to a request for listing versioning objects in a bucket</returns>
        public ListVersionsResponse ListVersions(ListVersionsRequest request)
        {
            return this.DoRequest<ListVersionsRequest, ListVersionsResponse>(request);
        }

        /// <summary>
        /// Obtain storage information about a bucket, including the bucket size and number of objects in the bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket storage information</param>
        /// <returns>Response to a request for obtaining bucket storage information</returns>
        public GetBucketStorageInfoResponse GetBucketStorageInfo(GetBucketStorageInfoRequest request)
        {
            return DoRequest<GetBucketStorageInfoRequest, GetBucketStorageInfoResponse>(request);
        }

        /// <summary>
        /// Obtain the bucket quota. Value "0" indicates that no upper limit is set for the bucket quota.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining a bucket quota</param>
        /// <returns> Response to a request for obtaining a bucket quota</returns>
        public GetBucketQuotaResponse GetBucketQuota(GetBucketQuotaRequest request)
        {
            return DoRequest<GetBucketQuotaRequest, GetBucketQuotaResponse>(request);
        }


        /// <summary>
        /// Obtain a bucket ACL.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining a bucket ACL</param>
        /// <returns>Response to a request for obtaining a bucket ACL</returns>
        public GetBucketAclResponse GetBucketAcl(GetBucketAclRequest request)
        {
            return DoRequest<GetBucketAclRequest, GetBucketAclResponse>(request);
        }

        /// <summary>
        /// List multipart uploads.
        /// </summary>
        /// <param name="request">Parameters in a request for listing multipart uploads</param>
        /// 
        /// <returns> Response to a request for listing multipart uploads</returns>
        public ListMultipartUploadsResponse ListMultipartUploads(ListMultipartUploadsRequest request)
        {
            return this.DoRequest<ListMultipartUploadsRequest, ListMultipartUploadsResponse>(request);
        }

        /// <summary>
        /// Delete a bucket. The bucket to be deleted must be empty (containing no objects, noncurrent object versions, or part fragments).
        /// </summary>
        /// <param name="request">Parameters in a bucket deletion request</param>
        /// <returns>Response to the bucket deletion request</returns>
        public DeleteBucketResponse DeleteBucket(DeleteBucketRequest request)
        {
            return this.DoRequest<DeleteBucketRequest, DeleteBucketResponse>(request);
        }


        /// <summary>
        /// Configure bucket logging.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring bucket logging</param>
        /// <returns>Response to a request for configuring bucket logging</returns>
        public SetBucketLoggingResponse SetBucketLogging(SetBucketLoggingRequest request)
        {
            return this.DoRequest<SetBucketLoggingRequest, SetBucketLoggingResponse>(request);
        }

        /// <summary>
        /// Obtain bucket logging configuration.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket logging configuration</param>
        /// <returns>Response to a request for obtaining bucket logging configuration</returns>
        public GetBucketLoggingResponse GetBucketLogging(GetBucketLoggingRequest request)
        {
            return this.DoRequest<GetBucketLoggingRequest, GetBucketLoggingResponse>(request);
        }


        /// <summary>
        /// Set a bucket policy. If the bucket already has a policy, the policy will be overwritten by the one specified in this request.
        /// </summary>
        /// <param name="request">Parameters in a request for setting bucket policies</param>
        /// <returns>Response to a request for setting bucket policies</returns>
        public SetBucketPolicyResponse SetBucketPolicy(SetBucketPolicyRequest request)
        {
            return this.DoRequest<SetBucketPolicyRequest, SetBucketPolicyResponse>(request);
        }

        /// <summary>
        /// Obtain bucket policies. 
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket policies</param>
        /// <returns>Response to the request for obtaining bucket policies</returns>
        public GetBucketPolicyResponse GetBucketPolicy(GetBucketPolicyRequest request)
        {
            return this.DoRequest<GetBucketPolicyRequest, GetBucketPolicyResponse>(request);
        }

        /// <summary>
        /// Delete bucket policies. 
        /// </summary>
        /// <param name="request">Parameters in a request for deleting bucket policies</param>
        /// <returns>Response to a request for deleting bucket policies</returns>
        public DeleteBucketPolicyResponse DeleteBucketPolicy(DeleteBucketPolicyRequest request)
        {
            return this.DoRequest<DeleteBucketPolicyRequest, DeleteBucketPolicyResponse>(request);
        }



        /// <summary>
        /// Set CORS rules for a bucket to allow client browsers to send cross-domain requests.
        /// </summary>
        /// <param name="request">Parameters in a request for setting CORS rules for a bucket</param>
        /// <returns>Response to a request for setting CORS rules for a bucket</returns>
        public SetBucketCorsResponse SetBucketCors(SetBucketCorsRequest request)
        {
            return this.DoRequest<SetBucketCorsRequest, SetBucketCorsResponse>(request);
        }


        /// <summary>
        /// Obtain the CORS rules of a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the CORS rules of a bucket</param> 
        /// <returns>Response to a request for obtaining the CORS rules of a bucket</returns>
        public GetBucketCorsResponse GetBucketCors(GetBucketCorsRequest request)
        {
            return this.DoRequest<GetBucketCorsRequest, GetBucketCorsResponse>(request);
        }



        /// <summary>
        /// Delete the CORS rules from a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting the CORS rules from a specified bucket</param>
        /// <returns>Response to a request for deleting the CORS rules from a specified bucket</returns>
        public DeleteBucketCorsResponse DeleteBucketCors(DeleteBucketCorsRequest request)
        {
            return this.DoRequest<DeleteBucketCorsRequest, DeleteBucketCorsResponse>(request);
        }



        /// <summary>
        /// Obtain the lifecycle rules of a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket lifecycle rules</param>
        /// <returns>Response to a request for obtaining bucket lifecycle rules</returns>
        public GetBucketLifecycleResponse GetBucketLifecycle(GetBucketLifecycleRequest request)
        {
            return this.DoRequest<GetBucketLifecycleRequest, GetBucketLifecycleResponse>(request);
        }



        /// <summary>
        /// Set lifecycle rules for a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for setting lifecycle rules for a bucket</param> 
        /// <returns>Response to a request for setting lifecycle rules for a bucket</returns>
        public SetBucketLifecycleResponse SetBucketLifecycle(SetBucketLifecycleRequest request)
        {
            return this.DoRequest<SetBucketLifecycleRequest, SetBucketLifecycleResponse>(request);
        }



        /// <summary>
        /// Delete the bucket lifecycle rules from a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting the bucket lifecycle rules from a bucket</param>
        /// <returns>Response to a request for deleting the bucket lifecycle rules from a bucket</returns>
        public DeleteBucketLifecycleResponse DeleteBucketLifecycle(DeleteBucketLifecycleRequest request)
        {
            return this.DoRequest<DeleteBucketLifecycleRequest, DeleteBucketLifecycleResponse>(request);
        }


        /// <summary>
        /// Obtain the website hosting configuration of a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket website hosting configuration</param>
        /// <returns>Response to a request for obtaining the bucket website hosting configuration</returns>
        public GetBucketWebsiteResponse GetBucketWebsite(GetBucketWebsiteRequest request)
        {
            return this.DoRequest<GetBucketWebsiteRequest, GetBucketWebsiteResponse>(request);
        }


        /// <summary>
        /// Configure website hosting for a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring website hosting for a bucket</param>
        /// <returns>Response to a request for configuring website hosting for a bucket</returns>
        public SetBucketWebsiteResponse SetBucketWebsiteConfiguration(SetBucketWebsiteRequest request)
        {
            return this.DoRequest<SetBucketWebsiteRequest, SetBucketWebsiteResponse>(request);
        }



        /// <summary>
        /// Delete the website hosting configuration from a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting the website hosting configuration from a bucket</param>
        /// <returns>Response to a request for deleting the website hosting configuration from a bucket</returns>
        public DeleteBucketWebsiteResponse DeleteBucketWebsite(DeleteBucketWebsiteRequest request)
        {
            return this.DoRequest<DeleteBucketWebsiteRequest, DeleteBucketWebsiteResponse>(request);
        }

        /// <summary>
        /// Set bucket versioning.
        /// </summary>
        /// <param name="request">Parameters in a request for setting bucket versioning</param>
        /// <returns>Response to a request for setting bucket versioning</returns>
        public SetBucketVersioningResponse SetBucketVersioning(SetBucketVersioningRequest request)
        {
            return this.DoRequest<SetBucketVersioningRequest, SetBucketVersioningResponse>(request);
        }



        /// <summary>
        /// Obtain the versioning status of a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the versioning status of a bucket</param>
        /// <returns>Response to a request for obtaining the versioning status of a bucket</returns>
        public GetBucketVersioningResponse GetBucketVersioning(GetBucketVersioningRequest request)
        {
            return this.DoRequest<GetBucketVersioningRequest, GetBucketVersioningResponse>(request);
        }

        /// <summary>
        /// Set bucket tags.
        /// </summary>
        /// <param name="request">Parameters in a request for setting bucket tags</param>
        /// <returns>Response to a request for setting bucket tags</returns>
        public SetBucketTaggingResponse SetBucketTagging(SetBucketTaggingRequest request)
        {
            return this.DoRequest<SetBucketTaggingRequest, SetBucketTaggingResponse>(request);
        }

        /// <summary>
        /// Obtain bucket tags.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket tags</param>
        /// <returns> Response to a request for obtaining bucket tags</returns>
        public GetBucketTaggingResponse GetBucketTagging(GetBucketTaggingRequest request)
        {
            return this.DoRequest<GetBucketTaggingRequest, GetBucketTaggingResponse>(request);
        }



        /// <summary>
        /// Delete bucket tags.
        /// </summary>
        /// <param name="request">Parameters in a bucket tag deletion request</param>
        /// <returns>Response to a bucket tag deletion request</returns>
        public DeleteBucketTaggingResponse DeleteBucketTagging(DeleteBucketTaggingRequest request)
        {
            return this.DoRequest<DeleteBucketTaggingRequest, DeleteBucketTaggingResponse>(request);
        }


        /// <summary>
        /// Configure cross-region replication for a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring cross-region replication for a bucket</param>
        /// <returns>Response to a request for configuring cross-region replication of a bucket</returns>
        public SetBucketReplicationResponse SetBucketReplication(SetBucketReplicationRequest request)
        {
            return this.DoRequest<SetBucketReplicationRequest, SetBucketReplicationResponse>(request);
        }

        /// <summary>
        /// Obtain the cross-region replication configuration of a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the cross-region replication configuration of a bucket</param>
        /// <returns>Response to a request for obtaining the cross-region replication configuration of a bucket</returns>
        public GetBucketReplicationResponse GetBucketReplication(GetBucketReplicationRequest request)
        {
            return this.DoRequest<GetBucketReplicationRequest, GetBucketReplicationResponse>(request);
        }



        /// <summary>
        /// Delete the cross-region replication configuration from a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting the cross-region replication configuration from a bucket</param>
        /// <returns>Response to a request for deleting the cross-region replication configuration from a bucket</returns>
        public DeleteBucketReplicationResponse DeleteBucketReplication(DeleteBucketReplicationRequest request)
        {
            return this.DoRequest<DeleteBucketReplicationRequest, DeleteBucketReplicationResponse>(request);
        }


        /// <summary>
        /// Configure bucket notification.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring bucket notification</param>
        /// <returns>Response to a request for configuring bucket notification</returns>
        public SetBucketNotificationResponse SetBucketNotification(SetBucketNotificationRequest request)
        {
            return this.DoRequest<SetBucketNotificationRequest, SetBucketNotificationResponse>(request);
        }

        /// <summary>
        /// Obtain the notification configuration of a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket notification configuration</param>
        /// <returns>Response to a request for obtaining the bucket notification configuration</returns>
        public GetBucketNotificationReponse GetBucketNotification(GetBucketNotificationRequest request)
        {
            return this.DoRequest<GetBucketNotificationRequest, GetBucketNotificationReponse>(request);
        }

        /// <summary>
        /// Set storage class for a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for setting a bucket storage class</param>
        /// <returns>Response to a request for setting a bucket storage class</returns>
        public SetBucketStoragePolicyResponse SetBucketStoragePolicy(SetBucketStoragePolicyRequest request)
        {
            return DoRequest<SetBucketStoragePolicyRequest, SetBucketStoragePolicyResponse>(request);
        }

        /// <summary>
        /// Obtain bucket storage policies.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket storage policies</param>
        /// <returns>Response to a request for obtaining bucket storage policies</returns>
        public GetBucketStoragePolicyResponse GetBucketStoragePolicy(GetBucketStoragePolicyRequest request)
        {
            return DoRequest<GetBucketStoragePolicyRequest, GetBucketStoragePolicyResponse>(request);
        }






    }
}


