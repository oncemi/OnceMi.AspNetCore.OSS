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
using System.Globalization;

namespace OBS.Internal
{
    internal static class Constants
    {

        internal static class CommonHeaders
        {
            public const string Connection = "Connection";

            public const string Range = "Range";

            public const string LastModified = "Last-Modified";

            public const string Location = "Location";

            public const string Expires = "Expires";

            public const string Date = "Date";

            public const string ContentMd5 = "Content-MD5";

            public const string ContentLength = "Content-Length";

            public const string ContentEncoding = "Content-Encoding";

            public const string ContentDisposition = "Content-Disposition";

            public const string ContentType = "Content-Type";

            public const string ETag = "ETag";

            public const string CacheControl = "Cache-Control";

            public const string Authorization = "Authorization";

            public const string Host = "Host";

            public const string IfModifiedSince = "If-Modified-Since";

            public const string IfUnmodifiedSince = "If-Unmodified-Since";

            public const string IfMatch = "If-Match";

            public const string IfNoneMatch = "If-None-Match";

            public const string UserAgent = "User-Agent";

            public const string OriginHeader = "Origin";

            public const string AccessControlRequestHeader = "Access-Control-Request-Headers";

        }

        internal static class ObsRequestParams
        {
            public const string UploadId = "uploadId";
            public const string PartNumber = "partNumber";
            public const string Prefix = "prefix";
            public const string Delimiter = "delimiter";
            public const string Marker = "marker";
            public const string KeyMarker = "key-marker";
            public const string MaxKeys = "max-keys";
            public const string VersionIdMarker = "version-id-marker";
            public const string MaxUploads = "max-uploads";
            public const string UploadIdMarker = "upload-id-marker";
            public const string VersionId = "versionId";
            public const string MaxParts = "max-parts";
            public const string PartNumberMarker = "part-number-marker";
            public const string ImageProcess = "x-image-process";
            public const string ResponseContentType = "response-content-type";
            public const string ResponseContentLanguage = "response-content-language";
            public const string ResponseExpires = "response-expires";
            public const string ResponseCacheControl = "response-cache-control";
            public const string ResponseContentDisposition = "response-content-disposition";
            public const string ResponseContentEncoding = "response-content-encoding";
            public const string Position = "position";

        }

        public const string UrlEncodedContent = "application/x-www-form-urlencoded; charset=utf-8";

        public const string ISO8601DateFormat = "yyyy-MM-dd\\THH:mm:ss.fff\\Z";

        public const string ISO8601DateFormatMidNight = "yyyy-MM-dd\\T00:00:00\\Z";

        public const string ISO8601DateFormatNoMS = "yyyy-MM-dd\\THH:mm:ss\\Z";

        public const string LongDateFormat = "yyyyMMddTHHmmssZ";

        public const string ShortDateFormat = "yyyyMMdd";

        public const string RFC822DateFormat = "ddd, dd MMM yyyy HH:mm:ss \\G\\M\\T";

        public const string SubResourceApiVersion = "apiversion";

        public const int DefaultBufferSize = 8192;

        public const long DefaultProgressUpdateInterval = 102400;

        public const int DefaultMaxIdleTime = 30 * 1000;
        public const int DefaultReadWriteTimeout = 60 * 1000;
        public const int DefaultMaxErrorRetry = 3;
        public const int DefaultConnectTimeout = -1;
        public const int DefaultAsyncSocketTimeout = -1;
        public const int DefaultConnectionLimit = 1000;
        public const bool DefaultKeepAlive = true;
        public const bool DefaultAuthTypeNegotiation = true;

        public const AuthTypeEnum DefaultAuthType = AuthTypeEnum.OBS;

        public const string ObsHeaderPrefix = "x-obs-";

        public const string V2HeaderPrefix = "x-amz-";

        public const string ObsHeaderMetaPrefix = "x-obs-meta-";

        public const string V2HeaderMetaPrefix = "x-amz-meta-";

        public const string ObsSdkVersion = "3.20.7";

        public const string ObsApiHeader = "api";
        public const string ObsApiHeaderWithPrefix = ObsHeaderPrefix + ObsApiHeader;

        public const string SdkUserAgent = "obs-sdk-.net/" + Constants.ObsSdkVersion;

        public const string NullRequest = "NullRequest";
        public const string NullRequestMessage = "request is null";

        public const string InvalidBucketName = "InvalidBucketName";
        public const string InvalidBucketNameMessage = "bucket name is not valid";

        public const string InvalidObjectKey = "InvalidObjectKey";
        public const string InvalidObjectKeyMessage = "object key is null";

        public const string InvalidSourceBucketNameMessage = "source object key is null";
        public const string InvalidSourceObjectKeyMessage = "source bucket name is null";

        public const string InvalidUploadId = "InvalidUploadId";
        public const string InvalidUploadIdMessage = "upload id is not valid";

        public const string InvalidPartNumber = "InvalidPartNumber";
        public const string InvalidPartNumberMessage = "part number is not valid";

        public const string InvalidEndpoint = "InvalidEndpoint";
        public const string InvalidEndpointMessage = "endpoint is not valid";

        public const string DefaultEncoding = "utf-8";

        public const long DefaultStreamBufferThreshold = 0;

        public static readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("en-US");

        public const string RequestTimeout = "RequestTimeout";

        public const string AllowedInUrl = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~:'()!*";
        
        private static readonly object _lock = new object();

        private static volatile IList<string> _AllowedResponseHttpHeaders;

        public const string ThreeAz = "3az";

        public const string ObsHeadErrorCode = "x-obs-error-code";

        public const string ObsHeadErrorMessage = "x-obs-error-message";

        public static IList<string> AllowedResponseHttpHeaders
        {
            get
            {
                if (_AllowedResponseHttpHeaders == null)
                {
                    lock (_lock)
                    {
                        if (_AllowedResponseHttpHeaders == null)
                        {
                            IList<string> tempAllowedResponseHttpHeaders = new List<string>();
                            tempAllowedResponseHttpHeaders.Add("content-type");
                            tempAllowedResponseHttpHeaders.Add("content-md5");
                            tempAllowedResponseHttpHeaders.Add("content-length");
                            tempAllowedResponseHttpHeaders.Add("content-language");
                            tempAllowedResponseHttpHeaders.Add("expires");
                            tempAllowedResponseHttpHeaders.Add("origin");
                            tempAllowedResponseHttpHeaders.Add("cache-control");
                            tempAllowedResponseHttpHeaders.Add("content-disposition");
                            tempAllowedResponseHttpHeaders.Add("content-encoding");
                            tempAllowedResponseHttpHeaders.Add("x-default-storage-class");
                            tempAllowedResponseHttpHeaders.Add("location");
                            tempAllowedResponseHttpHeaders.Add("date");
                            tempAllowedResponseHttpHeaders.Add("etag");
                            tempAllowedResponseHttpHeaders.Add("host");
                            tempAllowedResponseHttpHeaders.Add("last-modified");
                            tempAllowedResponseHttpHeaders.Add("content-range");
                            tempAllowedResponseHttpHeaders.Add("x-reserved");
                            tempAllowedResponseHttpHeaders.Add("access-control-allow-origin");
                            tempAllowedResponseHttpHeaders.Add("access-control-allow-headers");
                            tempAllowedResponseHttpHeaders.Add("access-control-max-age");
                            tempAllowedResponseHttpHeaders.Add("access-control-allow-methods");
                            tempAllowedResponseHttpHeaders.Add("access-control-expose-headers");
                            tempAllowedResponseHttpHeaders.Add("connection");
                            _AllowedResponseHttpHeaders = tempAllowedResponseHttpHeaders;
                        }
                    }
                }
                return _AllowedResponseHttpHeaders;
            }

        }

        private static volatile IList<string> _AllowedRequestHttpHeaders;

        public static IList<string> AllowedRequestHttpHeaders
        {
            get
            {
                if (_AllowedRequestHttpHeaders == null)
                {
                    lock (_lock)
                    {
                        if (_AllowedRequestHttpHeaders == null)
                        {
                            IList<string> tempAllowedRequestHttpHeaders = new List<string>();
                            tempAllowedRequestHttpHeaders.Add("content-type");
                            tempAllowedRequestHttpHeaders.Add("content-md5");
                            tempAllowedRequestHttpHeaders.Add("content-length");
                            tempAllowedRequestHttpHeaders.Add("content-language");
                            tempAllowedRequestHttpHeaders.Add("expires");
                            tempAllowedRequestHttpHeaders.Add("origin");
                            tempAllowedRequestHttpHeaders.Add("cache-control");
                            tempAllowedRequestHttpHeaders.Add("content-disposition");
                            tempAllowedRequestHttpHeaders.Add("content-encoding");
                            tempAllowedRequestHttpHeaders.Add("access-control-request-method");
                            tempAllowedRequestHttpHeaders.Add("access-control-request-headers");
                            tempAllowedRequestHttpHeaders.Add("success-action-redirect");
                            tempAllowedRequestHttpHeaders.Add("x-default-storage-class");
                            tempAllowedRequestHttpHeaders.Add("location");
                            tempAllowedRequestHttpHeaders.Add("date");
                            tempAllowedRequestHttpHeaders.Add("etag");
                            tempAllowedRequestHttpHeaders.Add("range");
                            tempAllowedRequestHttpHeaders.Add("host");
                            tempAllowedRequestHttpHeaders.Add("if-modified-since");
                            tempAllowedRequestHttpHeaders.Add("if-unmodified-since");
                            tempAllowedRequestHttpHeaders.Add("if-match");
                            tempAllowedRequestHttpHeaders.Add("if-none-match");
                            tempAllowedRequestHttpHeaders.Add("last-modified");
                            tempAllowedRequestHttpHeaders.Add("content-range");
                            _AllowedRequestHttpHeaders = tempAllowedRequestHttpHeaders;
                        }
                    }
                }
                return _AllowedRequestHttpHeaders;
            }

        }

        private static volatile IList<string> _AllowedResourceParameters;

        public static IList<string> AllowedResourceParameters
        {
            get
            {

                if (_AllowedResourceParameters == null)
                {
                    lock (_lock)
                    {
                        if (_AllowedResourceParameters == null)
                        {
                            IList<string> tempAllowedResourceParameters = new List<string>();
                            tempAllowedResourceParameters.Add("acl");
                            tempAllowedResourceParameters.Add("backtosource");
                            tempAllowedResourceParameters.Add("policy");
                            tempAllowedResourceParameters.Add("torrent");
                            tempAllowedResourceParameters.Add("logging");
                            tempAllowedResourceParameters.Add("location");
                            tempAllowedResourceParameters.Add("storageinfo");
                            tempAllowedResourceParameters.Add("quota");
                            tempAllowedResourceParameters.Add("storagepolicy");
                            tempAllowedResourceParameters.Add("storageclass");
                            tempAllowedResourceParameters.Add("requestpayment");
                            tempAllowedResourceParameters.Add("versions");
                            tempAllowedResourceParameters.Add("versioning");
                            tempAllowedResourceParameters.Add("versionid");
                            tempAllowedResourceParameters.Add("uploads");
                            tempAllowedResourceParameters.Add("uploadid");
                            tempAllowedResourceParameters.Add("partnumber");
                            tempAllowedResourceParameters.Add("website");
                            tempAllowedResourceParameters.Add("notification");
                            tempAllowedResourceParameters.Add("lifecycle");
                            tempAllowedResourceParameters.Add("delete");
                            tempAllowedResourceParameters.Add("cors");
                            tempAllowedResourceParameters.Add("restore");
                            tempAllowedResourceParameters.Add("tagging");
                            tempAllowedResourceParameters.Add("append");
                            tempAllowedResourceParameters.Add("position");
                            tempAllowedResourceParameters.Add("replication");
                            tempAllowedResourceParameters.Add("response-content-type");
                            tempAllowedResourceParameters.Add("response-content-language");
                            tempAllowedResourceParameters.Add("response-expires");
                            tempAllowedResourceParameters.Add("response-cache-control");
                            tempAllowedResourceParameters.Add("response-content-disposition");
                            tempAllowedResourceParameters.Add("response-content-encoding");
                            tempAllowedResourceParameters.Add("x-image-process");
                            tempAllowedResourceParameters.Add("x-oss-process");
                            _AllowedResourceParameters = tempAllowedResourceParameters;
                        }
                    }
                }
                return _AllowedResourceParameters;
            }

        }

        public static volatile IDictionary<string, string> _MimeTypes;


        public static IDictionary<string, string> MimeTypes
        {
            get
            {
                if (_MimeTypes == null)
                {
                    lock (_lock)
                    {
                        if (_MimeTypes == null)
                        {
                            IDictionary<string, string> tempMimeTypes = new Dictionary<string, string>();
                            tempMimeTypes.Add("7z", "application/x-7z-compressed");
                            tempMimeTypes.Add("aac", "audio/x-aac");
                            tempMimeTypes.Add("ai", "application/postscript");
                            tempMimeTypes.Add("aif", "audio/x-aiff");
                            tempMimeTypes.Add("asc", "text/plain");
                            tempMimeTypes.Add("asf", "video/x-ms-asf");
                            tempMimeTypes.Add("atom", "application/atom+xml");
                            tempMimeTypes.Add("avi", "video/x-msvideo");
                            tempMimeTypes.Add("bmp", "image/bmp");
                            tempMimeTypes.Add("bz2", "application/x-bzip2");
                            tempMimeTypes.Add("cer", "application/pkix-cert");
                            tempMimeTypes.Add("crl", "application/pkix-crl");
                            tempMimeTypes.Add("crt", "application/x-x509-ca-cert");
                            tempMimeTypes.Add("css", "text/css");
                            tempMimeTypes.Add("csv", "text/csv");
                            tempMimeTypes.Add("cu", "application/cu-seeme");
                            tempMimeTypes.Add("deb", "application/x-debian-package");
                            tempMimeTypes.Add("doc", "application/msword");
                            tempMimeTypes.Add("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                            tempMimeTypes.Add("dvi", "application/x-dvi");
                            tempMimeTypes.Add("eot", "application/vnd.ms-fontobject");
                            tempMimeTypes.Add("eps", "application/postscript");
                            tempMimeTypes.Add("epub", "application/epub+zip");
                            tempMimeTypes.Add("etx", "text/x-setext");
                            tempMimeTypes.Add("flac", "audio/flac");
                            tempMimeTypes.Add("flv", "video/x-flv");
                            tempMimeTypes.Add("gif", "image/gif");
                            tempMimeTypes.Add("gz", "application/gzip");
                            tempMimeTypes.Add("htm", "text/html");
                            tempMimeTypes.Add("html", "text/html");
                            tempMimeTypes.Add("ico", "image/x-icon");
                            tempMimeTypes.Add("ics", "text/calendar");
                            tempMimeTypes.Add("ini", "text/plain");
                            tempMimeTypes.Add("iso", "application/x-iso9660-image");
                            tempMimeTypes.Add("jar", "application/java-archive");
                            tempMimeTypes.Add("jpe", "image/jpeg");
                            tempMimeTypes.Add("jpeg", "image/jpeg");
                            tempMimeTypes.Add("jpg", "image/jpeg");
                            tempMimeTypes.Add("js", "text/javascript");
                            tempMimeTypes.Add("json", "application/json");
                            tempMimeTypes.Add("latex", "application/x-latex");
                            tempMimeTypes.Add("log", "text/plain");
                            tempMimeTypes.Add("m4a", "audio/mp4");
                            tempMimeTypes.Add("m4v", "video/mp4");
                            tempMimeTypes.Add("mid", "audio/midi");
                            tempMimeTypes.Add("midi", "audio/midi");
                            tempMimeTypes.Add("mov", "video/quicktime");
                            tempMimeTypes.Add("mp3", "audio/mpeg");
                            tempMimeTypes.Add("mp4", "video/mp4");
                            tempMimeTypes.Add("mp4a", "audio/mp4");
                            tempMimeTypes.Add("mp4v", "video/mp4");
                            tempMimeTypes.Add("mpe", "video/mpeg");
                            tempMimeTypes.Add("mpeg", "video/mpeg");
                            tempMimeTypes.Add("mpg", "video/mpeg");
                            tempMimeTypes.Add("mpg4", "video/mp4");
                            tempMimeTypes.Add("oga", "audio/ogg");
                            tempMimeTypes.Add("ogg", "audio/ogg");
                            tempMimeTypes.Add("ogv", "video/ogg");
                            tempMimeTypes.Add("ogx", "application/ogg");
                            tempMimeTypes.Add("pbm", "image/x-portable-bitmap");
                            tempMimeTypes.Add("pdf", "application/pdf");
                            tempMimeTypes.Add("pgm", "image/x-portable-graymap");
                            tempMimeTypes.Add("png", "image/png");
                            tempMimeTypes.Add("pnm", "image/x-portable-anymap");
                            tempMimeTypes.Add("ppm", "image/x-portable-pixmap");
                            tempMimeTypes.Add("ppt", "application/vnd.ms-powerpoint");
                            tempMimeTypes.Add("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
                            tempMimeTypes.Add("ps", "application/postscript");
                            tempMimeTypes.Add("qt", "video/quicktime");
                            tempMimeTypes.Add("rar", "application/x-rar-compressed");
                            tempMimeTypes.Add("ras", "image/x-cmu-raster");
                            tempMimeTypes.Add("rss", "application/rss+xml");
                            tempMimeTypes.Add("rtf", "application/rtf");
                            tempMimeTypes.Add("sgm", "text/sgml");
                            tempMimeTypes.Add("sgml", "text/sgml");
                            tempMimeTypes.Add("svg", "image/svg+xml");
                            tempMimeTypes.Add("swf", "application/x-shockwave-flash");
                            tempMimeTypes.Add("tar", "application/x-tar");
                            tempMimeTypes.Add("tif", "image/tiff");
                            tempMimeTypes.Add("tiff", "image/tiff");
                            tempMimeTypes.Add("torrent", "application/x-bittorrent");
                            tempMimeTypes.Add("ttf", "application/x-font-ttf");
                            tempMimeTypes.Add("txt", "text/plain");
                            tempMimeTypes.Add("wav", "audio/x-wav");
                            tempMimeTypes.Add("webm", "video/webm");
                            tempMimeTypes.Add("wma", "audio/x-ms-wma");
                            tempMimeTypes.Add("wmv", "video/x-ms-wmv");
                            tempMimeTypes.Add("woff", "application/x-font-woff");
                            tempMimeTypes.Add("wsdl", "application/wsdl+xml");
                            tempMimeTypes.Add("xbm", "image/x-xbitmap");
                            tempMimeTypes.Add("xls", "application/vnd.ms-excel");
                            tempMimeTypes.Add("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                            tempMimeTypes.Add("xml", "application/xml");
                            tempMimeTypes.Add("xpm", "image/x-xpixmap");
                            tempMimeTypes.Add("xwd", "image/x-xwindowdump");
                            tempMimeTypes.Add("yaml", "text/yaml");
                            tempMimeTypes.Add("yml", "text/yaml");
                            tempMimeTypes.Add("zip", "application/zip");
                            _MimeTypes = tempMimeTypes;
                        }
                    }
                }
                return _MimeTypes;
            }

        }

    }
}
