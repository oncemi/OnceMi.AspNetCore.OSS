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
using OBS.Internal;
using System;
using System.Collections.Generic;

namespace OBS.Model
{
    /// <summary>
    /// Customized metadata information
    /// </summary>
    public sealed class MetadataCollection
    {
        
        private IDictionary<string, string> values = new Dictionary<string, string>();

        /// <summary>
        /// Customized metadata
        /// </summary>
        /// <param name="name">Metadata element name</param>
        /// <returns>Metadata element value</returns>
        public string this[string name]
        {
            get
            {
                string value;
                if (values.TryGetValue(name, out value))
                {
                    return value;
                }

                return null;
            }
            set
            {
                values[name] = value;
            }
        }

        /// <summary>
        /// Add customized metadata.
        /// </summary>
        /// <param name="name">Metadata element name</param>
        /// <param name="value">Metadata element value</param>
        public void Add(string name, string value)
        {
            this[name] = value;
        }

        /// <summary>
        /// Number of corresponding metadata headers
        /// </summary>
        public int Count
        {
            get { return this.values.Count; }
        }

        /// <summary>
        /// Set of metadata element names
        /// </summary>
        public ICollection<string> Keys
        {
            get { return values.Keys; }
        }

        /// <summary>
        /// Set of metadata element values
        /// </summary>
        public ICollection<string> Values
        {
            get { return values.Values; }
        }

        /// <summary>
        /// Metadata key-value pairs
        /// </summary>
        public IList<KeyValuePair<string, string>> KeyValuePairs
        {
            get { return new List<KeyValuePair<string, string>>(this.values); }
        }
    }
}


