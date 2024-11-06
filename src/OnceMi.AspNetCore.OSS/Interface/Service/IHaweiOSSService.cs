
using OBS.Model;
using OnceMi.AspNetCore.OSS.Models.Huawei;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS
{
    public interface IHaweiOSSService : IOSSService
    {
        /// <summary>
        /// 获取桶存量信息
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        Task<BucketStorageInfo> GetBucketStorageInfoAsync(string bucketName);

        /// <summary>
        /// 设置桶存储类型
        /// </summary>
        /// <param name="bucketName">储存桶名称</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        /// <remarks>
        /// 标准存储(StorageClassEnum.Standard) 标准存储拥有低访问时延和较高的吞吐量，适用于有大量热点对象（平均一个月多次）或小对象（<1MB），且需要频繁访问数据的业务场景。
        /// 低频访问存储(StorageClassEnum.Warm) 低频访问存储适用于不频繁访问（平均一年少于12次）但在需要时也要求能够快速访问数据的业务场景。
        /// 归档存储(StorageClassEnum.Cold) 归档存储适用于很少访问（平均一年访问一次）数据的业务场景。
        /// </remarks>
        Task<bool> SetBucketStoragePolicyAsync(string bucketName, StorageClassEnum type);

        /// <summary>
        /// 获取桶存储类型
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>
        /// 标准存储(StorageClassEnum.Standard) 标准存储拥有低访问时延和较高的吞吐量，适用于有大量热点对象（平均一个月多次）或小对象（<1MB），且需要频繁访问数据的业务场景。
        /// 低频访问存储(StorageClassEnum.Warm) 低频访问存储适用于不频繁访问（平均一年少于12次）但在需要时也要求能够快速访问数据的业务场景。
        /// 归档存储(StorageClassEnum.Cold) 归档存储适用于很少访问（平均一年访问一次）数据的业务场景。
        /// </remarks>
        Task<StorageClassEnum> GetBucketStoragePolicyAsync(string bucketName);
    }
}
