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
using System.Collections.Generic;
using System.Text;

namespace OBS.Internal
{
    internal class ObsHeaders : IHeaders
    {

        private static ObsHeaders instance = new ObsHeaders();

        private ObsHeaders()
        {

        }

        public static IHeaders GetInstance()
        {
            return instance;
        }

        public string AclHeader()
        {
            return this.HeaderPrefix() + "acl";
        }

        public string AzRedundancyHeader()
        {
            return this.HeaderPrefix() + "az-redundancy";
        }

        public string BucketRegionHeader()
        {
            return this.HeaderPrefix() + "bucket-location";
        }

        public string ContentSha256Header()
        {
            return null;
        }

        public string CopySourceHeader()
        {
            return this.HeaderPrefix() + "copy-source";
        }

        public string CopySourceIfMatchHeader()
        {
            return this.HeaderPrefix() + "copy-source-if-match";
        }

        public string CopySourceIfModifiedSinceHeader()
        {
            return this.HeaderPrefix() + "copy-source-if-modified-since";
        }

        public string CopySourceIfNoneMatchHeader()
        {
            return this.HeaderPrefix() + "copy-source-if-none-match";
        }

        public string CopySourceIfUnmodifiedSinceHeader()
        {
            return this.HeaderPrefix() + "copy-source-if-unmodified-since";
        }

        public string CopySourceRangeHeader()
        {
            return this.HeaderPrefix() + "copy-source-range";
        }

        public string CopySourceSseCHeader()
        {
            return this.HeaderPrefix() + "copy-source-server-side-encryption-customer-algorithm";
        }

        public string CopySourceSseCKeyHeader()
        {
            return this.HeaderPrefix() + "copy-source-server-side-encryption-customer-key";
        }

        public string CopySourceSseCKeyMd5Header()
        {
            return this.HeaderPrefix() + "copy-source-server-side-encryption-customer-key-MD5";
        }

        public string CopySourceVersionIdHeader()
        {
            return this.HeaderPrefix() + "copy-source-version-id";
        }

        public string DateHeader()
        {
            return this.HeaderPrefix() + "date";
        }

        public string DefaultStorageClassHeader()
        {
            return this.HeaderPrefix() + "storage-class";
        }

        public string DeleteMarkerHeader()
        {
            return this.HeaderPrefix() + "delete-marker";
        }

        public string ExpirationHeader()
        {
            return this.HeaderPrefix() + "expiration";
        }

        public string ExpiresHeader()
        {
            return this.HeaderPrefix() + "expires";
        }

        public string GrantFullControlDeliveredHeader()
        {
            return this.HeaderPrefix() + "grant-full-control-delivered";
        }

        public string GrantFullControlHeader()
        {
            return this.HeaderPrefix() + "grant-full-control";
        }

        public string GrantReadAcpHeader()
        {
            return this.HeaderPrefix() + "grant-read-acp";
        }

        public string GrantReadDeliveredHeader()
        {
            return this.HeaderPrefix() + "grant-read-delivered";
        }

        public string GrantReadHeader()
        {
            return this.HeaderPrefix() + "grant-read";
        }

        public string GrantWriteAcpHeader()
        {
            return this.HeaderPrefix() + "grant-write-acp";
        }

        public string GrantWriteHeader()
        {
            return this.HeaderPrefix() + "grant-write";
        }

        public string HeaderMetaPrefix()
        {
            return Constants.ObsHeaderMetaPrefix;
        }

        public string HeaderPrefix()
        {
            return Constants.ObsHeaderPrefix;
        }

        public string LocationHeader()
        {
            return null;
        }

        public string MetadataDirectiveHeader()
        {
            return this.HeaderPrefix() + "metadata-directive";
        }

        public string NextPositionHeader()
        {
            return this.HeaderPrefix() + "next-append-position";
        }

        public string ObjectTypeHeader()
        {
            return this.HeaderPrefix() + "object-type";
        }

        public string RequestId2Header()
        {
            return this.HeaderPrefix() + "id-2";
        }

        public string RequestIdHeader()
        {
            return this.HeaderPrefix() + "request-id";
        }

        public string RestoreHeader()
        {
            return this.HeaderPrefix() + "restore";
        }

        public string SecurityTokenHeader()
        {
            return this.HeaderPrefix() + "security-token";
        }

        public string ServerVersionHeader()
        {
            return this.HeaderPrefix() + "version";
        }

        public string SseCHeader()
        {
            return this.HeaderPrefix() + "server-side-encryption-customer-algorithm";
        }

        public string SseCKeyHeader()
        {
            return this.HeaderPrefix() + "server-side-encryption-customer-key";
        }

        public string SseCKeyMd5Header()
        {
            return this.HeaderPrefix() + "server-side-encryption-customer-key-MD5";
        }

        public string SseKmsHeader()
        {
            return this.HeaderPrefix() + "server-side-encryption";
        }

        public string SseKmsKeyHeader()
        {
            return this.HeaderPrefix() + "server-side-encryption-kms-key-id";
        }

        public string StorageClassHeader()
        {
            return this.HeaderPrefix() + "storage-class";
        }

        public string SuccessRedirectLocationHeader()
        {
            return "success-action-redirect";
        }

        public string VersionIdHeader()
        {
            return this.HeaderPrefix() + "version-id";
        }

        public string WebsiteRedirectLocationHeader()
        {
            return this.HeaderPrefix() + "website-redirect-location";
        }

    }
}
