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
using System.IO;
using System.Net;
using OBS.Internal;
using OBS.Model;


namespace OBS
{
    public partial class ObsClient
    {

        /// <summary>
        /// Start the asynchronous request for obtaining the bucket list.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket list</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginListBuckets(ListBucketsRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<ListBucketsRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the bucket list.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to the request for obtaining the bucket list</returns>
        public ListBucketsResponse EndListBuckets(IAsyncResult ar)
        {
            return this.EndDoRequest<ListBucketsRequest, ListBucketsResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for creating a bucket.
        /// </summary>
        /// <param name="request">Parameters in the bucket creation request</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginCreateBucket(CreateBucketRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<CreateBucketRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for creating a bucket.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to the bucket creation request</returns>
        public CreateBucketResponse EndCreateBucket(IAsyncResult ar)
        {

            HttpObsAsyncResult result = ar as HttpObsAsyncResult;
            try
            {
                return this.EndDoRequest<CreateBucketRequest, CreateBucketResponse>(ar, false);
            }
            catch (ObsException ex)
            {
                if (result != null && result.HttpContext != null 
                    && result.HttpRequest != null && 
                    ex.StatusCode == HttpStatusCode.BadRequest
                    && "Unsupported Authorization Type".Equals(ex.ErrorMessage)
                    && this.ObsConfig.AuthTypeNegotiation
                    && result.HttpContext.AuthType == AuthTypeEnum.OBS)
                {
                    if (result.HttpRequest.Content != null && result.HttpRequest.Content.CanSeek)
                    {
                        result.HttpRequest.Content.Seek(0, SeekOrigin.Begin);
                    }
                    result.HttpContext.AuthType = AuthTypeEnum.V2;
                    HttpObsAsyncResult retryResult = this.httpClient.BeginPerformRequest(result.HttpRequest, result.HttpContext,
                        result.AsyncCallback, result.AsyncState);

                    retryResult.AdditionalState = result.AdditionalState;
                    return this.EndDoRequest<CreateBucketRequest, CreateBucketResponse>(retryResult);
                }
                throw ex;
            }
            finally
            {
                CommonUtil.CloseIDisposable(result);
            }
        }

        /// <summary>
        /// Start the asynchronous request for checking whether a bucket exists.
        /// </summary>
        /// <param name="request">Parameters in a request for checking whether a bucket exists</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginHeadBucket(HeadBucketRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<HeadBucketRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for checking whether a bucket exists.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to the request for querying whether a bucket exists</returns>
        public bool EndHeadBucket(IAsyncResult ar)
        {
            try
            {
                this.EndDoRequest<HeadBucketRequest, ObsWebServiceResponse>(ar);
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

        /// <summary>
        /// Start the asynchronous request for obtaining bucket metadata.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketMetadata(GetBucketMetadataRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<GetBucketMetadataRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining bucket metadata.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns> Response to a request for obtaining bucket metadata</returns>
        public GetBucketMetadataResponse EndGetBucketMetadata(IAsyncResult ar)
        {
            return EndDoRequest<GetBucketMetadataRequest, GetBucketMetadataResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for configuring a bucket quota.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring a bucket quota</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketQuota(SetBucketQuotaRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<SetBucketQuotaRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for configuring a bucket quota.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for configuring a bucket quota</returns>
        public SetBucketQuotaResponse EndSetBucketQuota(IAsyncResult ar)
        {
            return EndDoRequest<SetBucketQuotaRequest, SetBucketQuotaResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for configuring a bucket ACL.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring a bucket ACL</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketAcl(SetBucketAclRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<SetBucketAclRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for configuring a bucket ACL.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for configuring a bucket ACL</returns>
        public SetBucketAclResponse EndSetBucketAcl(IAsyncResult ar)
        {
            return EndDoRequest<SetBucketAclRequest, SetBucketAclResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the bucket location.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket location</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketLocation(GetBucketLocationRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<GetBucketLocationRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the bucket location.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining the bucket location</returns>
        public GetBucketLocationResponse EndGetBucketLocation(IAsyncResult ar)
        {
            return EndDoRequest<GetBucketLocationRequest, GetBucketLocationResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for listing objects in a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for listing objects in a bucket</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginListObjects(ListObjectsRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<ListObjectsRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for listing objects in a bucket.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for listing objects in a bucket</returns>
        public ListObjectsResponse EndListObjects(IAsyncResult ar)
        {
            return EndDoRequest<ListObjectsRequest, ListObjectsResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for listing versioning objects in a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for listing versioning objects in a bucket</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginListVersions(ListVersionsRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<ListVersionsRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for listing versioning objects in a bucket.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for listing versioning objects in a bucket</returns>
        public ListVersionsResponse EndListVersions(IAsyncResult ar)
        {
            return this.EndDoRequest<ListVersionsRequest, ListVersionsResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the bucket storage information.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket storage information</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketStorageInfo(GetBucketStorageInfoRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<GetBucketStorageInfoRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the bucket storage information.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining bucket storage information</returns>
        public GetBucketStorageInfoResponse EndGetBucketStorageInfo(IAsyncResult ar)
        {
            return EndDoRequest<GetBucketStorageInfoRequest, GetBucketStorageInfoResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining a bucket quota.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining a bucket quota</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketQuota(GetBucketQuotaRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<GetBucketQuotaRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining a bucket quota.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns> Response to a request for obtaining a bucket quota</returns>
        public GetBucketQuotaResponse EndGetBucketQuota(IAsyncResult ar)
        {
            return EndDoRequest<GetBucketQuotaRequest, GetBucketQuotaResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining a bucket ACL.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining a bucket ACL</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketAcl(GetBucketAclRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<GetBucketAclRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining a bucket ACL.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining a bucket ACL</returns>
        public GetBucketAclResponse EndGetBucketAcl(IAsyncResult ar)
        {
            return EndDoRequest<GetBucketAclRequest, GetBucketAclResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for listing multipart uploads.
        /// </summary>
        /// <param name="request">Parameters in a request for listing multipart uploads</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginListMultipartUploads(ListMultipartUploadsRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<ListMultipartUploadsRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for listing multipart uploads.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns> Response to a request for listing multipart uploads</returns>
        public ListMultipartUploadsResponse EndListMultipartUploads(IAsyncResult ar)
        {
            return this.EndDoRequest<ListMultipartUploadsRequest, ListMultipartUploadsResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for deleting a bucket.
        /// </summary>
        /// <param name="request">Parameters in a bucket deletion request</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginDeleteBucket(DeleteBucketRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<DeleteBucketRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for deleting a bucket.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to the bucket deletion request</returns>
        public DeleteBucketResponse EndDeleteBucket(IAsyncResult ar)
        {
            return this.EndDoRequest<DeleteBucketRequest, DeleteBucketResponse>(ar);
        }


        /// <summary>
        /// Start the asynchronous request for configuring bucket logging
        /// </summary>
        /// <param name="request">Parameters in a request for configuring bucket logging</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketLogging(SetBucketLoggingRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketLoggingRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for configuring bucket logging
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for configuring bucket logging</returns>
        public SetBucketLoggingResponse EndSetBucketLogging(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketLoggingRequest, SetBucketLoggingResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the bucket logging configuration.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket logging configuration</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketLogging(GetBucketLoggingRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketLoggingRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the bucket logging configuration.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining bucket logging configuration</returns>
        public GetBucketLoggingResponse EndGetBucketLogging(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketLoggingRequest, GetBucketLoggingResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for setting bucket policies.
        /// </summary>
        /// <param name="request">Parameters in a request for setting bucket policies</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketPolicy(SetBucketPolicyRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketPolicyRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for setting bucket policies.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for setting bucket policies</returns>
        public SetBucketPolicyResponse EndSetBucketPolicy(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketPolicyRequest, SetBucketPolicyResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining bucket policies.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket policies</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketPolicy(GetBucketPolicyRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketPolicyRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining bucket policies.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to the request for obtaining bucket policies</returns>
        public GetBucketPolicyResponse EndGetBucketPolicy(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketPolicyRequest, GetBucketPolicyResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for deleting bucket policies.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting bucket policies</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginDeleteBucketPolicy(DeleteBucketPolicyRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<DeleteBucketPolicyRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for deleting bucket policies.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for deleting bucket policies</returns>
        public DeleteBucketPolicyResponse EndDeleteBucketPolicy(IAsyncResult ar)
        {
            return this.EndDoRequest<DeleteBucketPolicyRequest, DeleteBucketPolicyResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for configuring bucket CORS.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring bucket CORS</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketCors(SetBucketCorsRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketCorsRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for configuring bucket CORS.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for configuring bucket CORS</returns>
        public SetBucketCorsResponse EndSetBucketCors(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketCorsRequest, SetBucketCorsResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the bucket CORS configuration.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket CORS configuration</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketCors(GetBucketCorsRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketCorsRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the bucket CORS configuration.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining bucket CORS configuration</returns>
        public GetBucketCorsResponse EndGetBucketCors(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketCorsRequest, GetBucketCorsResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for deleting the CORS configuration from a specified bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting the CORS configuration from a specified bucket</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginDeleteBucketCors(DeleteBucketCorsRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<DeleteBucketCorsRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for deleting the CORS configuration from a specified bucket.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for deleting the CORS configuration from a specified bucket</returns>
        public DeleteBucketCorsResponse EndDeleteBucketCors(IAsyncResult ar)
        {
            return this.EndDoRequest<DeleteBucketCorsRequest, DeleteBucketCorsResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the bucket lifecycle rules.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket lifecycle rules</param>
         /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketLifecycle(GetBucketLifecycleRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketLifecycleRequest>(request, callback, state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining bucket lifecycle rules</returns>
        public GetBucketLifecycleResponse EndGetBucketLifecycle(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketLifecycleRequest, GetBucketLifecycleResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for setting the bucket lifecycle rules.
        /// </summary>
        /// <param name="request">Parameters in a request for setting the bucket lifecycle rules</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketLifecycle(SetBucketLifecycleRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketLifecycleRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for setting the bucket lifecycle rules.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for setting the bucket lifecycle rules</returns>
        public SetBucketLifecycleResponse EndSetBucketLifecycle(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketLifecycleRequest, SetBucketLifecycleResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for deleting the bucket lifecycle rules.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting the bucket lifecycle rules</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginDeleteBucketLifecycle(DeleteBucketLifecycleRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<DeleteBucketLifecycleRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for deleting the bucket lifecycle rules.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for deleting the bucket lifecycle rules</returns>
        public DeleteBucketLifecycleResponse EndDeleteBucketLifecycle(IAsyncResult ar)
        {
            return this.EndDoRequest<DeleteBucketLifecycleRequest, DeleteBucketLifecycleResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the website hosting configuration.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket website hosting configuration</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketWebsite(GetBucketWebsiteRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketWebsiteRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the website hosting configuration.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining the bucket website hosting configuration</returns>
        public GetBucketWebsiteResponse EndGetBucketWebsite(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketWebsiteRequest, GetBucketWebsiteResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for configuring bucket website hosting.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring bucket website hosting</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketWebsiteConfiguration(SetBucketWebsiteRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketWebsiteRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for configuring bucket website hosting.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for configuring bucket website hosting</returns>
        public SetBucketWebsiteResponse EndSetBucketWebsiteConfiguration(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketWebsiteRequest, SetBucketWebsiteResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for deleting the website hosting configuration.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting the bucket website hosting configuration</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginDeleteBucketWebsite(DeleteBucketWebsiteRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<DeleteBucketWebsiteRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for deleting the website hosting configuration.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for deleting the bucket website hosting configuration</returns>
        public DeleteBucketWebsiteResponse EndDeleteBucketWebsite(IAsyncResult ar)
        {
            return this.EndDoRequest<DeleteBucketWebsiteRequest, DeleteBucketWebsiteResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for configuring bucket versioning.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring bucket versioning</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketVersioning(SetBucketVersioningRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketVersioningRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for configuring bucket versioning.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for setting bucket versioning</returns>
        public SetBucketVersioningResponse EndSetBucketVersioning(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketVersioningRequest, SetBucketVersioningResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the bucket versioning configuration.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket versioning configuration</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketVersioning(GetBucketVersioningRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketVersioningRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the bucket versioning configuration.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining the bucket versioning configuration</returns>
        public GetBucketVersioningResponse EndGetBucketVersioning(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketVersioningRequest, GetBucketVersioningResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for setting bucket tags.
        /// </summary>
        /// <param name="request">Parameters in a request for setting bucket tags</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketTagging(SetBucketTaggingRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketTaggingRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for setting bucket tags.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for setting bucket tags</returns>
        public SetBucketTaggingResponse EndSetBucketTagging(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketTaggingRequest, SetBucketTaggingResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining bucket tags.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket tags</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketTagging(GetBucketTaggingRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketTaggingRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining bucket tags.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns> Response to a request for obtaining bucket tags</returns>
        public GetBucketTaggingResponse EndGetBucketTagging(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketTaggingRequest, GetBucketTaggingResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for deleting bucket tags.
        /// </summary>
        /// <param name="request">Parameters in a bucket tag deletion request</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginDeleteBucketTagging(DeleteBucketTaggingRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<DeleteBucketTaggingRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for deleting bucket tags.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a bucket tag deletion request</returns>
        public DeleteBucketTaggingResponse EndDeleteBucketTagging(IAsyncResult ar)
        {
            return this.EndDoRequest<DeleteBucketTaggingRequest, DeleteBucketTaggingResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for configuring cross-region replication for a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring cross-region replication for a bucket</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketReplication(SetBucketReplicationRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketReplicationRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for configuring cross-region replication for a bucket.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for configuring cross-region replication of a bucket</returns>
        public SetBucketReplicationResponse EndSetBucketReplication(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketReplicationRequest, SetBucketReplicationResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the cross-region replication configuration of a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the cross-region replication configuration of a bucket</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketReplication(GetBucketReplicationRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketReplicationRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the cross-region replication configuration of a bucket.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining the cross-region copy configuration of a bucket</returns>
        public GetBucketReplicationResponse EndGetBucketReplication(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketReplicationRequest, GetBucketReplicationResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for deleting the cross-region replication configuration from a bucket.
        /// </summary>
        /// <param name="request">Parameters in a request for deleting the cross-region replication configuration from a bucket</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginDeleteBucketReplication(DeleteBucketReplicationRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<DeleteBucketReplicationRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for deleting the cross-region replication configuration from a bucket.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for deleting the cross-region replication configuration from a bucket</returns>
        public DeleteBucketReplicationResponse EndDeleteBucketReplication(IAsyncResult ar)
        {
            return this.EndDoRequest<DeleteBucketReplicationRequest, DeleteBucketReplicationResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for configuring bucket notification.
        /// </summary>
        /// <param name="request">Parameters in a request for configuring bucket notification</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketNotification(SetBucketNotificationRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<SetBucketNotificationRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for configuring bucket notification.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for configuring bucket notification</returns>
        public SetBucketNotificationResponse EndSetBucketNotification(IAsyncResult ar)
        {
            return this.EndDoRequest<SetBucketNotificationRequest, SetBucketNotificationResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining the bucket notification configuration.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining the bucket notification configuration</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketNotification(GetBucketNotificationRequest request, AsyncCallback callback, object state)
        {
            return this.BeginDoRequest<GetBucketNotificationRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining the bucket notification configuration.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining the bucket notification configuration</returns>
        public GetBucketNotificationReponse EndGetBucketNotification(IAsyncResult ar)
        {
            return this.EndDoRequest<GetBucketNotificationRequest, GetBucketNotificationReponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for setting a bucket storage class.
        /// </summary>
        /// <param name="request">Parameters in a request for setting a bucket storage class</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginSetBucketStoragePolicy(SetBucketStoragePolicyRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<SetBucketStoragePolicyRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for setting a bucket storage class.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for setting a bucket storage class</returns>
        public SetBucketStoragePolicyResponse EndSetBucketStoragePolicy(IAsyncResult ar)
        {
            return EndDoRequest<SetBucketStoragePolicyRequest, SetBucketStoragePolicyResponse>(ar);
        }

        /// <summary>
        /// Start the asynchronous request for obtaining bucket storage policies.
        /// </summary>
        /// <param name="request">Parameters in a request for obtaining bucket storage policies</param>
        /// <param name="callback">Asynchronous request callback function</param>
        /// <param name="state">Asynchronous request status object</param>
        /// <returns>Response to the asynchronous request</returns>
        public IAsyncResult BeginGetBucketStoragePolicy(GetBucketStoragePolicyRequest request, AsyncCallback callback, object state)
        {
            return BeginDoRequest<GetBucketStoragePolicyRequest>(request, callback, state);
        }

        /// <summary>
        /// End the asynchronous request for obtaining bucket storage policies.
        /// </summary>
        /// <param name="ar">Response to the asynchronous request</param>
        /// <returns>Response to a request for obtaining bucket storage policies</returns>
        public GetBucketStoragePolicyResponse EndGetBucketStoragePolicy(IAsyncResult ar)
        {
            return EndDoRequest<GetBucketStoragePolicyRequest, GetBucketStoragePolicyResponse>(ar);
        }

    }
}


