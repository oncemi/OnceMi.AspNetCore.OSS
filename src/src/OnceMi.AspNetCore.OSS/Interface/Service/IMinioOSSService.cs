using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public interface IMinioOSSService : IOSSService
    {
        Task<bool> RemoveIncompleteUploadAsync(string bucketName, string objectName);

        Task<List<ItemUploadInfo>> ListIncompleteUploads(string bucketName);

        Task<PolicyInfo> GetPolicyAsync(string bucketName);

        Task<bool> SetPolicyAsync(string bucketName, List<StatementItem> statements);

        Task<bool> RemovePolicyAsync(string bucketName);

        Task<bool> PolicyExistsAsync(string bucketName, StatementItem statement);
    }
}
