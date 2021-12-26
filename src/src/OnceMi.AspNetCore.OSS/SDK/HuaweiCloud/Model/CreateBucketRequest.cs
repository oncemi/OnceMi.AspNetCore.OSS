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

namespace OBS.Model
{
    /// <summary>
    /// Parameters in a bucket creation request
    /// </summary>
    public class CreateBucketRequest : ObsBucketWebServiceRequest
    {

        private IDictionary<ExtensionBucketPermissionEnum, IList<string>> extensionPermissionMap;

        /// <summary>
        /// Grant the OBS extension permissions to a user.
        /// </summary>
        /// <param name="domainId">ID of the domain to which the user belongs</param>
        /// <param name="extensionPermissionEnum">OBS extension permissions</param>
        public void GrantExtensionPermission(string domainId, ExtensionBucketPermissionEnum extensionPermissionEnum)
        {
            if(string.IsNullOrEmpty(domainId))
            {
                return;
            }

            IList<string> domainIds;

            ExtensionPermissionMap.TryGetValue(extensionPermissionEnum, out domainIds);

            if(domainIds == null)
            {
                domainIds = new List<string>();
                ExtensionPermissionMap.Add(extensionPermissionEnum, domainIds);
            }
            domainId = domainId.Trim();
            if (!domainIds.Contains(domainId))
            {
                domainIds.Add(domainId);
            }

        }

        /// <summary>
        /// Withdraw the user's OBS extension permissions.
        /// </summary>
        /// <param name="domainId">ID of the domain to which the user belongs</param>
        /// <param name="extensionPermissionEnum">OBS extension permissions</param>
        public void WithDrawExtensionPermission(string domainId, ExtensionBucketPermissionEnum extensionPermissionEnum)
        {
            if (string.IsNullOrEmpty(domainId))
            {
                return;
            }

            IList<string> domainIds;
            ExtensionPermissionMap.TryGetValue(extensionPermissionEnum, out domainIds);
            domainId = domainId.Trim();
            if (domainIds != null && domainIds.Contains(domainId))
            {
                domainIds.Remove(domainId);
            }
        }

        internal IDictionary<ExtensionBucketPermissionEnum, IList<string>> ExtensionPermissionMap
        {
            get
            {
                return extensionPermissionMap ?? (extensionPermissionMap = new Dictionary<ExtensionBucketPermissionEnum, IList<string>>());
            }
        }

        /// <summary>
        /// Bucket storage class that can be pre-defined during the bucket creation
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public StorageClassEnum? StorageClass
        {
            get;
            set;
        }


        /// <summary>
        /// ACL that can be pre-defined during the bucket creation
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// </para>
        /// </remarks>
        public CannedAclEnum? CannedAcl
        {
            get;
            set;
        }



        public AvailableZoneEnum? AvailableZone
        {
            get;
            set;
        }

        /// <summary>
        /// Bucket name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        /// A bucket name must comply with the following rules:
        /// 1. Contains 3 to 63 characters, starts with a digit or letter, and supports lowercase letters, digits, hyphens (-), and periods (.).
        /// 2. Cannot be an IP address.
        /// 3. Cannot start or end with a hyphen (-) or period (.).
        /// 4. Cannot contain two consecutive periods (.), for example, "my..bucket".
        /// 5. Cannot contain periods (.) and hyphens (-) adjacent to each other, for example, "my-.bucket" or "my.-bucket".
        /// </para>
        /// </remarks>
        public override string BucketName
        {
            get;
            set;
        }


        /// <summary>
        /// Bucket location
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional parameter
        /// Bucket location. This parameter is mandatory unless the endpoint belongs to the default region. 
        /// </para>
        /// </remarks>
        public string Location
        {
            get;
            set;
        }

        internal override string GetAction()
        {
            return "CreateBucket";
        }

    }
}
    
