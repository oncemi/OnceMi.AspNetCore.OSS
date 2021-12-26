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

namespace OBS.Internal
{
    internal interface IHeaders
    {
        string DefaultStorageClassHeader();
        string AclHeader();
        string RequestIdHeader();
        string RequestId2Header();
        string BucketRegionHeader();
        string LocationHeader();
        string StorageClassHeader();
        string WebsiteRedirectLocationHeader();
        string SuccessRedirectLocationHeader();
        string SseKmsHeader();
        string SseKmsKeyHeader();
        string SseCHeader();
        string SseCKeyHeader();
        string SseCKeyMd5Header();
        string ExpiresHeader();
        string VersionIdHeader();
        string CopySourceHeader();
        string CopySourceRangeHeader();
        string CopySourceVersionIdHeader();
        string CopySourceSseCHeader();
        string CopySourceSseCKeyHeader();
        string CopySourceSseCKeyMd5Header();
        string MetadataDirectiveHeader();
        string DateHeader();
        string DeleteMarkerHeader();
        string HeaderPrefix();
        string HeaderMetaPrefix();
        string SecurityTokenHeader();
        string ContentSha256Header();

        string ExpirationHeader();
        string RestoreHeader();

        string ServerVersionHeader();

        string GrantReadHeader();
        string GrantWriteHeader();
        string GrantReadAcpHeader();
        string GrantWriteAcpHeader();
        string GrantFullControlHeader();
        string GrantReadDeliveredHeader();
        string GrantFullControlDeliveredHeader();

        string CopySourceIfModifiedSinceHeader();
        string CopySourceIfUnmodifiedSinceHeader();
        string CopySourceIfNoneMatchHeader();
        string CopySourceIfMatchHeader();

        string ObjectTypeHeader();
        string NextPositionHeader();

        string AzRedundancyHeader();
    }
}
