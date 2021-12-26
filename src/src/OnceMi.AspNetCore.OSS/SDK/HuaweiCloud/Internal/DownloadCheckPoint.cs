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
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using OBS.Model;

namespace OBS.Internal
{
    /// <summary>
    /// 断点续传下载文件的记录参数
    /// </summary>
    [XmlRoot("DownloadCheckPoint")]
    public class DownloadCheckPoint
    {
        /// <summary>
        /// 桶名
        /// </summary>
        [XmlElement("BucketName")]
        public string BucketName { get; set; }

        /// <summary>
        /// 对象名
        /// </summary>
        [XmlElement("ObjectKey")]
        public string ObjectKey { get; set; }

        /// <summary>
        /// 下载文件路径
        /// </summary>
        [XmlElement("DownloadFile")]
        public string DownloadFile { get; set; }

        /// <summary>
        /// 对象版本号
        /// </summary>
        [XmlElement("VersionId")]
        public string VersionId { get; set; }

        /// <summary>
        /// DownloadCheckPoint类的HashCode
        /// </summary>
        [XmlElement("Md5")]
        public string Md5 { get; set; }

        /// <summary>
        /// 对象状态
        /// </summary>
        [XmlElement("ObjectStatus")]
        public ObjectStatus ObjectStatus { get; set; }

        /// <summary>
        /// 临时文件状态
        /// </summary>
        [XmlElement("TmpFileStatus")]
        public TmpFileStatus TmpFileStatus { get; set; }

        /// <summary>
        /// 分段信息
        /// </summary>
        [XmlArray("DownloadParts")]
        public List<DownloadPart> DownloadParts { get; set; }

        /// <summary>
        /// 下载段任务取消标志位
        /// </summary>
        [XmlIgnore]
        public volatile bool IsDownloadAbort = false;

        //同步锁
        [XmlIgnore]
        internal readonly object downloadlock = new object();

        /// <summary>
        /// 加载序列化记录文件
        /// </summary>
        public void Load(string checkPointFile)
        {
            try
            {
                DownloadCheckPoint temp = null;
                XmlSerializer serializer = new XmlSerializer(this.GetType());

                using (XmlTextReader fs = new XmlTextReader(checkPointFile))
                {
                    temp = (DownloadCheckPoint)serializer.Deserialize(fs);
                }
                Assign(temp);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将序列化文件中字段信息赋值到类DownloadCheckPoint中的字段
        /// </summary>
        /// <param name="temp"></param>
        public void Assign(DownloadCheckPoint temp)
        {
            this.BucketName = temp.BucketName;
            this.ObjectKey = temp.ObjectKey;
            this.DownloadFile = temp.DownloadFile;
            this.VersionId = temp.VersionId;
            this.Md5 = temp.Md5;
            this.ObjectStatus = temp.ObjectStatus;
            this.TmpFileStatus = temp.TmpFileStatus;
            this.DownloadParts = temp.DownloadParts;
        }

        /// <summary>
        /// 序列化记录文件的数据一致性校验
        /// Md5值；临时文件的名字、大小；对象的大小、最后修改时间、Etag值
        /// </summary>
        /// <param name="tmpFilePath"></param>
        /// <param name="obsClient"></param>
        /// <returns></returns>
        public bool IsValid(string tmpFilePath, ObsClient obsClient)
        {
            if (this.Md5 != ComputeHash.HashCode<DownloadCheckPoint>(this))
                return false;

            FileInfo fileInfo = new FileInfo(tmpFilePath);
            if (!this.TmpFileStatus.TmpFilePath.Equals(tmpFilePath) || this.TmpFileStatus.Size != fileInfo.Length)
                return false;

            GetObjectMetadataResponse response = obsClient.GetObjectMetadata(BucketName, ObjectKey, VersionId);
            if (!this.ObjectStatus.Etag.Equals(response.ETag) || this.ObjectStatus.Size != response.ContentLength || this.ObjectStatus.LastModified != response.LastModified)
                return false;

            return true;
        }


        /// <summary>
        /// 出现网络异常时,更新临时文件的修改时间
        /// </summary>
        /// <param name="tmpFilePath"></param>
        public void UpdateTmpFile(string tmpFilePath)
        {
            this.TmpFileStatus.LastModified = File.GetLastWriteTime(tmpFilePath);
        }

        /// <summary>
        /// 将DownloadCheckPoint中的字段数据写入CheckPointFile文件
        /// </summary>
        /// 多个线程都需要调用该方法，需保证线程安全性
        /// <param name="checkPointFile"></param>
        public void Record(string checkPointFile)
        {
            this.Md5 = ComputeHash.HashCode<DownloadCheckPoint>(this);

            try
            {
                XmlSerializer serializer = new XmlSerializer(this.GetType());

                using (XmlTextWriter fs = new XmlTextWriter(checkPointFile, Encoding.UTF8))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    /// <summary>
    /// 对象状态
    /// </summary>
    [XmlRoot("ObjectStatus")]
    public class ObjectStatus
    {
        /// <summary>
        /// 对象大小
        /// </summary>
        [XmlElement("Size")]
        public long Size { get; set; }

        /// <summary>
        /// 对象的最后修改时间
        /// </summary>
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// 对象的Etag值
        /// </summary>
        [XmlElement("Etag")]
        public string Etag { get; set; }

       
    }

    /// <summary>
    /// 临时文件状态
    /// </summary>
    [XmlRoot("TmpFileStatus")]
    public class TmpFileStatus
    {
        /// <summary>
        /// 临时文件的大小
        /// </summary>
        [XmlElement("Size")]
        public long Size { get; set; }

        /// <summary>
        /// 临时文件的最后修改时间
        /// </summary>
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// 临时文件的路径
        /// </summary>
        [XmlElement("TmpFilePath")]
        public string TmpFilePath { get; set; }

    }

    /// <summary>
    /// 分段信息
    /// </summary>
    [XmlRoot("DownloadPart")]
    public class DownloadPart
    {
        /// <summary>
        /// 分段序号
        /// </summary>
        [XmlElement("PartNumber")]
        public int PartNumber { set; get; }

        /// <summary>
        /// 分段在文件中的起始位置
        /// </summary>
        [XmlElement("Start")]
        public long Start { set; get; }

        /// <summary>
        /// 分段在文件中的结束位置
        /// </summary>
        [XmlElement("End")]
        public long End { set; get; }

        /// <summary>
        /// 分段是否已下载完成
        /// </summary>
        [XmlElement("IsCompleted")]
        public bool IsCompleted { set; get; }

    }
}
