using Minio;

namespace OnceMi.AspNetCore.OSS
{
    public interface IOSSServiceFactory
    {
        IOSSService Create();

        IOSSService Create(string name);
    }
}