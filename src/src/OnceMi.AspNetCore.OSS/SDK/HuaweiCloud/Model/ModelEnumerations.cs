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
using System.Reflection;
using OBS.Internal;

namespace OBS.Model
{

    public enum AvailableZoneEnum
    {
        [StringValue("3az")]
        MultiAz
    }

    /// <summary>
    /// Mode for presenting the uploading progress
    /// </summary>
    public enum ProgressTypeEnum
    {
        /// <summary>
        /// Specify that the upload progress is refreshed each time a specified number of bytes is uploaded.
        /// </summary>
        ByBytes,
        /// <summary>
        /// Specify that the upload is refreshed every several seconds.
        /// </summary>
        BySeconds
    }

    /// <summary>
    /// Event type of resumable uploads
    /// </summary>
    public enum ResumableUploadEventTypeEnum
    {
        InitiateMultipartUploadSucceed,
        InitiateMultipartUploadFailed,
        UploadPartSucceed,
        UploadPartFailed,
        CompleteMultipartUploadSucceed,
        CompleteMultipartUploadFailed
    }

    /// <summary>
    /// Event type of resumable downloads
    /// </summary>
    public enum ResumableDownloadEventTypeEnum
    {
        DownloadPartSucceed,
        DownloadPartFailed,
    }


    /// <summary>
    /// Event type of notification messages
    /// </summary>
    public enum EventTypeEnum
    {
        /// <summary>
        /// All events for creating objects
        /// </summary>
        [StringValue("ObjectCreated:*")]
        ObjectCreatedAll,
        /// <summary>
        /// PUT Object events
        /// </summary>
        [StringValue("ObjectCreated:Put")]
        ObjectCreatedPut,
        /// <summary>
        /// POST Object events
        /// </summary>
        [StringValue("ObjectCreated:Post")]
        ObjectCreatedPost,

        /// <summary>
        /// Events for copying objects
        /// </summary>
        [StringValue("ObjectCreated:Copy")]
        ObjectCreatedCopy,
        /// <summary>
        /// Events for combining parts
        /// </summary>
        [StringValue("ObjectCreated:CompleteMultipartUpload")]
        ObjectCreatedCompleteMultipartUpload,
        /// <summary>
        /// All events for deleting objects
        /// </summary>
        [StringValue("ObjectRemoved:*")]
        ObjectRemovedAll,
        /// <summary>
        /// Events for deleting objects by specifying object version IDs
        /// </summary>
        [StringValue("ObjectRemoved:Delete")]
        ObjectRemovedDelete,
        /// <summary>
        /// Events for deleting objects without specifying version IDs after versioning is enabled
        /// </summary>
        [StringValue("ObjectRemoved:DeleteMarkerCreated")]
        ObjectRemovedDeleteMarkerCreated
    }

    /// <summary>
    /// HTTP method type
    /// </summary>
    public enum HttpVerb
    {
        /// <summary>
        /// HTTP GET request
        /// </summary>
        GET,
        /// <summary>
        /// HTTP HEAD request
        /// </summary>
        HEAD,
        /// <summary>
        /// HTTP PUT request
        /// </summary>
        PUT,
        /// <summary>
        /// HTTP POST request
        /// </summary>
        POST,
        /// <summary>
        /// HTTP DELETE request
        /// </summary>
        DELETE
    }

    /// <summary>
    /// Storage class
    /// </summary>
    public enum StorageClassEnum
    {
        /// <summary>
        /// OBS Standard
        /// </summary>
        [StringValue("STANDARD")]
        Standard,

        /// <summary>
        /// OBS Infrequent Access
        /// </summary>
        [StringValue("STANDARD_IA")]
        Warm,

        /// <summary>
        /// OBS Archive
        /// </summary>
        [StringValue("GLACIER")]
        Cold
    }

    /// <summary>
    /// Pre-defined access control policy
    /// </summary>
    public enum CannedAclEnum
    {
        /// <summary>
        /// Private read/write
        /// </summary>
        [StringValue("private")]
        Private,

        /// <summary>
        /// Public read and private write
        /// </summary>
        [StringValue("public-read")]
        PublicRead,

        /// <summary>
        /// Public read/write
        /// </summary>
        [StringValue("public-read-write")]
        PublicReadWrite,

        /// <summary>
        /// Public read on a bucket as well as objects in the bucket
        /// </summary>
        [StringValue("public-read-delivered")]
        PublicReadDelivered,

        /// <summary>
        /// Public read/write on a bucket as well as objects in the bucket
        /// </summary>
        [StringValue("public-read-write-delivered")]
        PublicReadWriteDelivered,

        /// <summary>
        /// Read for grantees and private write
        /// </summary>
        [StringValue("authenticated-read")]
        [Obsolete]
        AuthenticatedRead,

        /// <summary>
        /// Read for bucket owners and read/write for object owners
        /// </summary>
        [StringValue("bucket-owner-read")]
        [Obsolete]
        BucketOwnerRead,

        /// <summary>
        /// Read/Write for bucket owners and object owners
        /// </summary>
        [StringValue("bucket-owner-full-control")]
        [Obsolete]
        BucketOwnerFullControl,

        /// <summary>
        /// Write for the log delivery group
        /// </summary>
        [StringValue("log-delivery-write")]
        [Obsolete]
        LogDeliveryWrite

    }

    /// <summary>
    /// OBS bucket extension permission
    /// </summary>
    public enum ExtensionBucketPermissionEnum
    {
        /// <summary>
        /// Grant the read permission to all users belonging to the specified "domainId" for listing objects, listing multipart uploads, listing bucket versions, and obtaining bucket metadata. 
        /// </summary>
        GrantRead,

        /// <summary>
        /// Grant the write permission to all users belonging to the specified "domainId" so that the users can create, delete, overwrite objects in buckets, as well as initialize, upload, copy, and combine parts and abort multipart uploads.
        /// </summary>
        GrantWrite,

        /// <summary>
        /// Grant the READ_ACP permission to all users belonging to the specified "domainId" to obtain ACLs of objects. 
        /// </summary>
        GrantReadAcp,

        /// <summary>
        /// Grant the WRITE_ACP permission to all users belonging to the specified "domainId" to modify bucket ACLs.
        /// </summary>
        GrantWriteAcp,

        /// <summary>
        /// Grant full control permissions to all users belonging to the specified "domainId". 
        /// </summary>
        GrantFullControl,

        /// <summary>
        /// Grant the read permission to all users belonging to the specified "domainId". By default, these users have read permissions on all objects in the bucket. 
        /// </summary>
        GrantReadDelivered,

        /// <summary>
        /// Grant the full control permission to all users belonging to the specified "domainId". By default, these users have the full control permission on all objects in the bucket. 
        /// </summary>
        GrantFullControlDelivered
    }

    /// <summary>
    /// OBS bucket extension permission
    /// </summary>
    public enum ExtensionObjectPermissionEnum
    {
        /// <summary>
        /// Grant the read permission to all users belonging to the specified "domainId" to read objects and obtain object metadata. 
        /// </summary>
        GrantRead,

        /// <summary>
        /// Grant the READ_ACP permission to all users belonging to the specified "domainId" to read ACLs of objects. 
        /// </summary>
        GrantReadAcp,

        /// <summary>
        /// Grant the WRITE_ACP permission to all users belonging to the specified "domainId" to modify ACLs of objects. 
        /// </summary>
        GrantWriteAcp,

        /// <summary>
        /// Grant the full control permission to all users belonging to the specified "domainId" to read objects, obtain object metadata, as well as obtain and write object ACLs. 
        /// </summary>
        GrantFullControl
    }

    /// <summary>
    /// Rule status
    /// </summary>
    public enum RuleStatusEnum
    {
        /// <summary>
        /// Enabling rule
        /// </summary>
        Enabled,
        /// <summary>
        /// Disabling rule
        /// </summary>
        Disabled
    }

    /// <summary>
    /// Permission type
    /// </summary>
    public enum PermissionEnum
    {
        /// <summary>
        /// Read permission
        /// </summary>
        [StringValue("READ")]
        Read,

        /// <summary>
        /// Read permission
        /// </summary>
        [StringValue("WRITE")]
        Write,

        /// <summary>
        /// ACP read permission
        /// </summary>
        [StringValue("READ_ACP")]
        ReadAcp,

        /// <summary>
        /// ACP write permission
        /// </summary>
        [StringValue("WRITE_ACP")]
        WriteAcp,

        /// <summary>
        /// Full control permission
        /// </summary>
        [StringValue("FULL_CONTROL")]
        FullControl
    }

    /// <summary>
    /// Replication policy
    /// </summary>
    public enum MetadataDirectiveEnum
    {
        /// <summary>
        /// When copying an object, the object's properties are also copied.
        /// </summary>
        Copy,

        /// <summary>
        /// When copying an object, the object's properties are rewritten.
        /// </summary>
        Replace
    }

    /// <summary>
    /// Redirection protocol
    /// </summary>
    public enum ProtocolEnum
    {
        /// <summary>
        /// HTTP is used for redirection.
        /// </summary>
        Http,

        /// <summary>
        /// HTTPS is used for redirection.
        /// </summary>
        Https
    }

    /// <summary>
    /// Sub-resource type
    /// </summary>
    public enum SubResourceEnum
    {
        /// <summary>
        /// Obtain the bucket location information.
        /// </summary>
        [StringValue("location")]
        Location,

        /// <summary>
        /// Obtain bucket storage information.
        /// </summary>
        [StringValue("storageinfo")]
        StorageInfo,

        /// <summary>
        /// Obtain or set a bucket quota.
        /// </summary>
        [StringValue("quota")]
        Quota,

        /// <summary>
        /// Obtain or set the ACL of a bucket (object).
        /// </summary>
        [StringValue("acl")]
        Acl,

        /// <summary>
        /// Obtain or set the logging configuration of a bucket.
        /// </summary>
        [StringValue("logging")]
        Logging,

        /// <summary>
        /// Obtain, set, or delete bucket policies.
        /// </summary>
        [StringValue("policy")]
        Policy,

        /// <summary>
        /// Obtain, set, or delete bucket lifecycle rules.
        /// </summary>
        [StringValue("lifecycle")]
        Lifecyle,

        /// <summary>
        /// Obtain, set, or delete the logging configuration of a bucket.
        /// </summary>
        [StringValue("website")]
        Website,

        /// <summary>
        /// Obtain or set the versioning status of a bucket.
        /// </summary>
        [StringValue("versioning")]
        Versioning,

        /// <summary>
        /// Obtain or set the storage class of a bucket. 
        /// </summary>
        [StringValue("storageClass")]
        StorageClass,

        /// <summary>
        /// Obtain or set bucket storage policies.
        /// </summary>
        [StringValue("storagePolicy")]
        [Obsolete]
        StoragePolicy,

        /// <summary>
        /// Perform an appendable upload.
        /// </summary>
        [StringValue("append")]
        Append,

        /// <summary>
        /// Obtain, set, or delete the CORS configuration of a bucket.
        /// </summary>
        [StringValue("cors")]
        Cors,

        /// <summary>
        /// List or initialize multipart uploads.
        /// </summary>
        [StringValue("uploads")]
        Uploads,

        /// <summary>
        /// List versioning objects in a bucket.
        /// </summary>
        [StringValue("versions")]
        Versions,

        /// <summary>
        /// Delete objects in a batch.
        /// </summary>
        [StringValue("delete")]
        Delete,

        /// <summary>
        /// Restore an Archive object.
        /// </summary>
        [StringValue("restore")]
        Restore,

        /// <summary>
        /// Obtain, set, or delete bucket tags.
        /// </summary>
        [StringValue("tagging")]
        Tagging,

        /// <summary>
        /// Configure bucket notification or obtain bucket notification configuration.
        /// </summary>
        [StringValue("notification")]
        Notification,

        /// <summary>
        /// Configure cross-region replication for a bucket, or obtain or delete the cross-region replication configuration of a bucket.
        /// </summary>
        [StringValue("replication")]
        Replication,

    }


    /// <summary>
    /// Authorized user group information
    /// </summary>
    public enum GroupGranteeEnum
    {
        /// <summary>
        /// Anonymous user group, indicating all users
        /// </summary>
        [StringValue("http://acs.amazonaws.com/groups/global/AllUsers")]
        AllUsers,

        /// <summary>
        /// OBS grantee group, indicating all users that owning OBS accounts
        /// </summary>
        [StringValue("http://acs.amazonaws.com/groups/global/AuthenticatedUsers")]
        [Obsolete]
        AuthenticatedUsers,

        /// <summary>
        /// Log delivery user group, indicating common users that can configure access logs
        /// </summary>
        [StringValue("http://acs.amazonaws.com/groups/s3/LogDelivery")]
        [Obsolete]
        LogDelivery
    }


    /// <summary>
    /// Restore option
    /// </summary>
    public enum RestoreTierEnum
    {
        /// <summary>
        ///Expedited restoration, which restores an object in 1 to 5 minutes
        /// </summary>
        Expedited,

        /// <summary>
        ///Standard restoration, which restores an object in 3 to 5 hours
        /// </summary>
        Standard,

        /// <summary>
        ///Batch restoration, which restores objects in 5 to 12 hours
        /// </summary>
        [Obsolete]
        Bulk

    }

    /// <summary>
    /// SSE-C encryption algorithm type
    /// </summary>
    public enum SseCAlgorithmEnum
    {
        /// <summary>
        /// AES256 algorithm
        /// </summary>
        Aes256
    }

    /// <summary>
    /// SSE-KMS encryption algorithm type
    /// </summary>
    public enum SseKmsAlgorithmEnum
    {
        /// <summary>
        /// Basic KMS algorithm
        /// </summary>
        [StringValue("kms")]
        Kms
    }

    /// <summary>
    /// Versioning status
    /// </summary>
    public enum VersionStatusEnum
    {
        /// <summary>
        /// Enable versioning.
        /// </summary>
        Enabled,
        /// <summary>
        /// Suspend versioning.
        /// </summary>
        Suspended
    }


    /// <summary>
    /// Filtering method
    /// </summary>
    public enum FilterNameEnum
    {
        /// <summary>
        /// Filtering by prefix
        /// </summary>
        Prefix,

        /// <summary>
        /// Filtering by suffix
        /// </summary>
        Suffix

    }

}

