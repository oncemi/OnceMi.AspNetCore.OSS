using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OnceMi.AspNetCore.OSS
{
    public class Bucket
    {

        /// <summary>
        /// Bucket location getter/setter
        /// </summary>
        public string Location { get; internal set; }

        /// <summary>
        /// Bucket name getter/setter
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Bucket <see cref="Owner" /> getter/setter
        /// </summary>
        public Owner Owner { get; internal set; }

        /// <summary>
        /// Bucket creation time getter/setter
        /// </summary>
        public string CreationDate { get; internal set; }

        public DateTime CreationDateDateTime
        {
            get
            {
                return DateTime.Parse(this.CreationDate, CultureInfo.InvariantCulture);
            }
        }
    }
}
