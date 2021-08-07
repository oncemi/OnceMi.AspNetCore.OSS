using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public class IBaseOSSService
    {
        internal virtual string BuildPresignedObjectCacheKey(string bucketName, string objectName, PresignedObjectType type)
        {
            return Encrypt.MD5($"{this.GetType().FullName}_{bucketName}_{objectName}_{type.ToString().ToUpper()}");
        }

        internal virtual string FormatObjectName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName) || objectName == "/")
            {
                throw new ArgumentNullException(nameof(objectName));
            }
            if (objectName.StartsWith('/'))
            {
                return objectName.TrimStart('/');
            }
            return objectName;
        }
    }
}
