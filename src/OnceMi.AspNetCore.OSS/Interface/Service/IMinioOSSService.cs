using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public interface IMinioOSSService : IOSSService
    {
        Task<bool> RemoveIncompleteUploadAsync(string bucketName, string objectName);

        Task<IEnumerable<ItemUploadInfo>> ListIncompleteUploads(string bucketName, string prefix = null, bool recursive = false);

        Task<PolicyInfo> GetPolicyAsync(string bucketName);

        Task<bool> SetPolicyAsync(string bucketName, List<StatementItem> statements);

        Task<bool> RemovePolicyAsync(string bucketName);

        Task<bool> PolicyExistsAsync(string bucketName, StatementItem statement);
    }
}
