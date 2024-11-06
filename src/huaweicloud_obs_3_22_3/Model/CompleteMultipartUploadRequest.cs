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
using System.Collections.Generic;

namespace OBS.Model
{
    /// <summary>
    /// Parameters in the request for combining parts
    /// </summary>
    public class CompleteMultipartUploadRequest : ObsBucketWebServiceRequest
    {
        internal override string GetAction()
        {
            return "CompleteMultipartUpload";
        }

        private IList<PartETag> partETags;


        /// <summary>
        /// Object name
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        ///  </para> 
        /// </remarks>
        public string ObjectKey
        {
            get;
            set;
        }


        /// <summary>
        /// List of parts to be combined
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        ///  </para> 
        /// </remarks>
        public IList<PartETag> PartETags
        {
            get
            {
                return this.partETags ?? (this.partETags = new List<PartETag>());
            }
            set { this.partETags = value; }
        }

        /// <summary>
        /// Add part information to the to-be-comibined part list.
        /// </summary>
        /// <param name="partETags">Information about the part to be added</param>
        public void AddPartETags(params PartETag[] partETags)
        {
            foreach (PartETag part in partETags)
            {
                this.PartETags.Add(part);
            }
        }

        /// <summary>
        /// Add part information to the to-be-comibined part list.
        /// </summary>
        /// <param name="partETags">Information about the part to be added</param>
        public void AddPartETags(IEnumerable<PartETag> partETags)
        {
            foreach (PartETag part in partETags)
            {
                this.PartETags.Add(part);
            }
        }

        /// <summary>
        /// Obtain the part information from the response to the multipart upload and add the information to the to-be-combined part list.
        /// </summary>
        ///  <param name="responses">Response to the multipart upload</param>
        public void AddPartETags(params UploadPartResponse[] responses)
        {
            foreach (UploadPartResponse response in responses)
            {
                this.PartETags.Add(new  PartETag(response.PartNumber, response.ETag));
            }
        }

        /// <summary>
        /// Obtain the part information from the response to the multipart upload and add the information to the to-be-combined part list.
        /// </summary>
        /// <param name="responses">Response to the multipart upload</param>
        public void AddPartETags(IEnumerable<UploadPartResponse> responses)
        {
            foreach (UploadPartResponse response in responses)
            {
                this.PartETags.Add(new PartETag(response.PartNumber, response.ETag));
            }
        }

        /// <summary>
        /// Obtain the part information from the response to the multipart copy and add the information to the to-be-combined part list.
        /// </summary>
        /// <param name="responses">Response to the multipart copy</param>
        public void AddPartETags(params CopyPartResponse[] responses)
        {
            foreach (CopyPartResponse response in responses)
            {
                this.PartETags.Add(new PartETag(response.PartNumber, response.ETag));
            }
        }

        /// <summary>
        /// Obtain the part information from the response to the multipart copy and add the information to the to-be-combined part list.
        /// </summary>
        /// <param name="responses">Response to the multipart copy</param>
        public void AddPartETags(IEnumerable<CopyPartResponse> responses)
        {
            foreach (CopyPartResponse response in responses)
            {
                this.PartETags.Add(new PartETag(response.PartNumber, response.ETag));
            }
        }

        /// <summary>
        /// Multipart upload ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mandatory parameter
        ///  </para> 
        /// </remarks>
        public string UploadId
        {
            get;
            set;
        }

    }
}
    


