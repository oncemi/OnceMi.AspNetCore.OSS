using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS.Models.Qiniu
{
    class QiniuObjectMetadata
    {
        /// <summary>
        /// 文件大小，int64 类型，单位为字节（Byte）。
        /// </summary>
        public long fsize { get; set; }

        /// <summary>
        /// 文件HASH值，string 类型。
        /// </summary>
        public string hash { get; set; }

        /// <summary>
        /// 文件 md5 值，string类型，32 位 16 进制组成的字符串，只有通过直传文件和追加文件 API 上传的文件，服务端确保有该字段返回，如请求时服务端没有返回md5字段，可以通过请求qhash/md5 方法来获取，比如 http://test.com/test.mp4?qhash/md5
        /// </summary>
        public string md5 { get; set; }

        /// <summary>
        /// 文件MIME类型，string 类型。
        /// </summary>
        public string mimeType { get; set; }

        /// <summary>
        /// 文件上传时间，int64 类型，Unix时间戳格式，单位为 100 纳秒。
        /// 例如：值为13603956734587420的时间，对应的实际时间为2013-02-09 15:41:13。
        /// </summary>
        public long putTime { get; set; }

        /// <summary>
        /// 文件存储类型，uint32 类型，2 表示归档存储，1 表示低频存储，0表示普通存储。
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 归档存储文件的解冻状态，uint3 2类型，2表示解冻完成，1表示解冻中；归档文件冻结时，不返回该字段。
        /// </summary>
        public int restoreStatus { get; set; }

        /// <summary>
        /// 文件状态，uint32 类型。1 表示禁用；只有禁用状态的文件才会返回该字段。
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 文件过期删除日期，int64 类型，Unix 时间戳格式，具体文件过期日期计算参考 生命周期管理。文件在设置过期时间后才会返回该字段（通过生命周期规则设置文件过期时间，仅对该功能发布后满足规则条件新上传文件返回该字段；历史文件想要返回该字段需要在功能发布后可通过 修改文件过期删除时间 API 或者 修改文件生命周期 API 指定过期时间；对于已经设置过过期时间的历史文件，到期都会正常过期删除，只是服务端没有该字段返回)
        /// 例如：值为1568736000的时间，表示文件会在2019/9/18当天内删除。
        /// </summary>
        public long expiration { get; set; }

        /// <summary>
        /// 文件生命周期中转为低频存储的日期，int64 类型，Unix 时间戳格式 ，具体日期计算参考 生命周期管理。文件在设置转低频后才会返回该字段（通过生命周期规则设置文件转低频，仅对该功能发布后满足规则条件新上传文件返回该字段；历史文件想要返回该字段需要在功能发布后可通过 修改文件生命周期 API 指定转低频时间；对于已经设置过转低频时间的历史文件，到期都会正常执行，只是服务端没有该字段返回)
        /// 例如：值为1568736000的时间，表示文件会在2019/9/18当天转为低频存储类型。
        /// </summary>
        public long transitionToIA { get; set; }

        /// <summary>
        /// 文件生命周期中转为归档存储的日期，int64 类型，Unix 时间戳格式 ，具体日期计算参考 生命周期管理。文件在设置转归档后才会返回该字段（通过生命周期规则设置文件转归档，仅对该功能发布后满足规则条件新上传文件返回该字段；历史文件想要返回该字段需要在功能发布后可通过 修改文件生命周期 API 指定转归档时间；对于已经设置过转归档时间的历史文件，到期都会正常执行，只是服务端没有该字段返回)
        /// 例如：值为1568736000的时间，表示文件会在2019/9/18当天转为低归档储类型。
        /// </summary>
        public long transitionToARCHIVE { get; set; }
    }
}
