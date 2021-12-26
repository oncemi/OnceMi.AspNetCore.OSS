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
using OBS.Model;
using OBS.Internal.Negotiation;

namespace OBS.Internal
{
    internal interface IParser
    {
        ListBucketsResponse ParseListBucketsResponse(HttpResponse httpResponse);

        GetBucketStoragePolicyResponse ParseGetBucketStoragePolicyResponse(HttpResponse httpResonse);

        GetBucketMetadataResponse ParseGetBucketMetadataResponse(HttpResponse httpResponse);

        GetBucketLocationResponse ParseGetBucketLocationResponse(HttpResponse httpResponse);

        GetBucketStorageInfoResponse ParseGetBucketStorageInfoResponse(HttpResponse httpResponse);

        ListObjectsResponse ParseListObjectsResponse(HttpResponse httpResponse);

        ListVersionsResponse ParseListVersionsResponse(HttpResponse httpResponse);

        GetBucketQuotaResponse ParseGetBucketQuotaResponse(HttpResponse httpResponse);

        GetBucketAclResponse ParseGetBucketAclResponse(HttpResponse httpResponse);

        ListMultipartUploadsResponse ParseListMultipartUploadsResponse(HttpResponse httpResponse);

        GetBucketLoggingResponse ParseGetBucketLoggingResponse(HttpResponse httpResponse);

        GetBucketPolicyResponse ParseGetBucketPolicyResponse(HttpResponse httpResponse);

        GetBucketCorsResponse ParseGetBucketCorsResponse(HttpResponse httpResponse);

        GetBucketLifecycleResponse ParseGetBucketLifecycleResponse(HttpResponse httpResponse);

        GetBucketWebsiteResponse ParseGetBucketWebsiteResponse(HttpResponse httpResponse);

        GetBucketVersioningResponse ParseGetBucketVersioningResponse(HttpResponse httpResponse);

        GetBucketTaggingResponse ParseGetBucketTaggingResponse(HttpResponse httpResponse);

        GetBucketNotificationReponse ParseGetBucketNotificationReponse(HttpResponse httpResponse);

        DeleteObjectResponse ParseDeleteObjectResponse(HttpResponse httpResponse);

        DeleteObjectsResponse ParseDeleteObjectsResponse(HttpResponse httpResponse);

        ListPartsResponse ParseListPartsResponse(HttpResponse httpResponse);

        CompleteMultipartUploadResponse ParseCompleteMultipartUploadResponse(HttpResponse httpResponse);

        GetObjectAclResponse ParseGetObjectAclResponse(HttpResponse httpResponse);

        PutObjectResponse ParsePutObjectResponse(HttpResponse httpResponse);

        CopyObjectResponse ParseCopyObjectResponse(HttpResponse httpResponse);

        InitiateMultipartUploadResponse ParseInitiateMultipartUploadResponse(HttpResponse httpResponse);

        CopyPartResponse ParseCopyPartResponse(HttpResponse httpResponse);

        UploadPartResponse ParseUploadPartResponse(HttpResponse httpResponse);

        GetBucketReplicationResponse ParseGetBucketReplicationResponse(HttpResponse httpResponse);

        GetObjectMetadataResponse ParseGetObjectMetadataResponse(HttpResponse httpResponse);

        GetObjectResponse ParseGetObjectResponse(HttpResponse httpResponse);

        AppendObjectResponse ParseAppendObjectResponse(HttpResponse httpResponse);
    }
}
