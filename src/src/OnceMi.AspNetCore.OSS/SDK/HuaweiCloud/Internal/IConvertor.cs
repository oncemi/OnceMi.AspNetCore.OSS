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
using System.Xml;
using OBS.Model;
using OBS.Internal.Negotiation;

namespace OBS.Internal
{
    internal interface IConvertor
    {
        HttpRequest Trans(ListBucketsRequest request);

        HttpRequest Trans(CreateBucketRequest request);

        HttpRequest Trans(HeadBucketRequest request);

        HttpRequest Trans(SetBucketStoragePolicyRequest request);

        HttpRequest Trans(GetBucketStoragePolicyRequest request);

        HttpRequest Trans(DeleteBucketRequest request);

        HttpRequest Trans(GetBucketMetadataRequest request);

        HttpRequest Trans(GetBucketLocationRequest request);

        HttpRequest Trans(GetBucketStorageInfoRequest request);

        HttpRequest Trans(ListObjectsRequest request);

        HttpRequest Trans(ListVersionsRequest request);

        HttpRequest Trans(SetBucketQuotaRequest request);

        HttpRequest Trans(GetBucketQuotaRequest request);

        HttpRequest Trans(SetBucketAclRequest request);

        HttpRequest Trans(GetBucketAclRequest request);

        HttpRequest Trans(ListMultipartUploadsRequest request);

        HttpRequest Trans(SetBucketLoggingRequest request);

        HttpRequest Trans(GetBucketLoggingRequest request);

        HttpRequest Trans(SetBucketPolicyRequest request);

        HttpRequest Trans(GetBucketPolicyRequest request);

        HttpRequest Trans(DeleteBucketPolicyRequest request);

        HttpRequest Trans(SetBucketCorsRequest request);

        HttpRequest Trans(GetBucketCorsRequest request);

        HttpRequest Trans(DeleteBucketCorsRequest request);

        HttpRequest Trans(SetBucketLifecycleRequest request);
        HttpRequest Trans(GetBucketLifecycleRequest request);
        HttpRequest Trans(DeleteBucketLifecycleRequest request);

        HttpRequest Trans(SetBucketWebsiteRequest request);
        HttpRequest Trans(GetBucketWebsiteRequest request);
        HttpRequest Trans(DeleteBucketWebsiteRequest request);

        HttpRequest Trans(SetBucketVersioningRequest request);
        HttpRequest Trans(GetBucketVersioningRequest request);

        HttpRequest Trans(SetBucketTaggingRequest request);
        HttpRequest Trans(GetBucketTaggingRequest request);
        HttpRequest Trans(DeleteBucketTaggingRequest request);

        HttpRequest Trans(SetBucketNotificationRequest request);
        HttpRequest Trans(GetBucketNotificationRequest request);

        HttpRequest Trans(AbortMultipartUploadRequest request);

        HttpRequest Trans(DeleteObjectRequest request);
        HttpRequest Trans(DeleteObjectsRequest request);

        HttpRequest Trans(RestoreObjectRequest request);

        HttpRequest Trans(ListPartsRequest request);

        HttpRequest Trans(CompleteMultipartUploadRequest request);

        HttpRequest Trans(SetObjectAclRequest request);
        HttpRequest Trans(GetObjectAclRequest request);

        HttpRequest Trans(PutObjectRequest request);

        HttpRequest Trans(CopyObjectRequest request);

        HttpRequest Trans(InitiateMultipartUploadRequest request);

        HttpRequest Trans(CopyPartRequest request);

        HttpRequest Trans(UploadPartRequest request);

        HttpRequest Trans(SetBucketReplicationRequest request);
        HttpRequest Trans(GetBucketReplicationRequest request);
        HttpRequest Trans(DeleteBucketReplicationRequest request);

        HttpRequest Trans(GetObjectMetadataRequest request);
        HttpRequest Trans(GetObjectRequest request);

        HttpRequest Trans(AppendObjectRequest request);

        HttpRequest Trans(GetApiVersionRequest request);

        HttpRequest Trans(HeadObjectRequest request);
    }

    internal delegate void TransContentDelegate(XmlWriter xmlWriter);
}
