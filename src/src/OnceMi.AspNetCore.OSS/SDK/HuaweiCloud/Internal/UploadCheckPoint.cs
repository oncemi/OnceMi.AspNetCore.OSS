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
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using OBS.Model;

namespace OBS.Internal
{
    /// <summary>
    /// 断点续传上传文件的记录参数
    /// </summary>
    [XmlRoot("UploadCheckPoint")]
    public class UploadCheckPoint
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
        /// 上传文件路径
        /// </summary>
        [XmlElement("UploadFile")]
        public string UploadFile { get; set; }

        /// <summary>
        /// 上传数据流
        /// </summary>
        [XmlIgnore]
        public Stream UploadStream { get; set; }

        /// <summary>
        /// 多段上传任务号
        /// </summary>
        [XmlElement("UploadId")]
        public string UploadId { get; set; }

        /// <summary>
        /// UploadCheckPoint类的HashCode
        /// </summary>
        [XmlElement("Md5")]
        public string Md5 { get; set; }

        /// <summary>
        /// 上传文件的状态
        /// </summary>
        [XmlElement("FileStatus")]
        public FileStatus FileStatus { get; set; }

        /// <summary>
        /// 分段信息
        /// </summary>
		[XmlArray("UploadParts")]
        public List<UploadPart> UploadParts { get; set; }

        /// <summary>
        /// 已上传成功段列表
        /// </summary>
		[XmlArray("PartEtags")]
        public List<PartETag> PartEtags { get; set; }

        /// <summary>
        /// 上传段任务取消标志位
        /// </summary>
        [XmlIgnore]
        public volatile bool IsUploadAbort = false;

        [XmlIgnore]
        internal readonly object uploadlock = new object();

        /// <summary>
        /// 加载序列化文件CheckPointFile
        /// </summary>
        public void Load(string checkPointFile)
        {
            UploadCheckPoint temp = null;

            XmlSerializer serializer = new XmlSerializer(this.GetType());

            using (XmlTextReader fs = new XmlTextReader(checkPointFile))
            {
                temp = (UploadCheckPoint)serializer.Deserialize(fs);
            }
            Assign(temp);
        }

        /// <summary>
        /// 将序列化文件中字段信息赋值到类UploadCheckPoint中的字段
        /// </summary>
        /// <param name="temp"></param>
        public void Assign(UploadCheckPoint temp)
        {
            this.BucketName = temp.BucketName;
            this.ObjectKey = temp.ObjectKey;
            this.UploadFile = temp.UploadFile;
            this.UploadId = temp.UploadId;
            this.Md5 = temp.Md5;
            this.FileStatus = temp.FileStatus;
            this.UploadParts = temp.UploadParts;
            this.PartEtags = temp.PartEtags;
        }

        /// <summary>
        /// 将UploadCheckPoint中的字段数据写入CheckPointFile文件
        /// </summary>
        /// 多个线程都需要调用该方法，需保证线程安全性
        public void Record(string checkPointFile)
        {
            this.Md5 = ComputeHash.HashCode<UploadCheckPoint>(this);
            XmlSerializer serializer = new XmlSerializer(this.GetType());

            using (XmlTextWriter fs = new XmlTextWriter(checkPointFile, Encoding.UTF8))
            {
                fs.Formatting = Formatting.Indented;
                serializer.Serialize(fs, this);
            }
        }

        /// <summary>
        /// 序列化记录文件的数据一致性校验
        /// Md5值；文件的名字、大小、最后修改时间；CheckSum值
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="uploadStream"></param>
        /// <param name="enableCheckSum"></param>
        /// <returns></returns>
        public bool IsValid(string uploadFile, Stream uploadStream, bool enableCheckSum, ResumableUploadTypeEnum uploadType)
        {
            if (this.Md5 != ComputeHash.HashCode<UploadCheckPoint>(this))
                return false;

            if (uploadType == ResumableUploadTypeEnum.UploadFile)
            {
                FileInfo upload = new FileInfo(uploadFile);
                if (this.FileStatus.Size != upload.Length || this.FileStatus.LastModified != upload.LastWriteTime)
                    return false;
            }
            else if(this.FileStatus.Size != uploadStream.Length - uploadStream.Position)
            {
                return false;
            }

            if (enableCheckSum)
            {
                if (this.FileStatus.CheckSum != null)
                {
                    try
                    {
                        if (uploadType == ResumableUploadTypeEnum.UploadFile)
                        {
                            using (FileStream fileStream = new FileStream(uploadFile, FileMode.Open))
                            {
                                //校验CheckSum值--UploadFile文件的一致性
                                return this.FileStatus.CheckSum.Equals(CommonUtil.Base64Md5(fileStream));
                            }
                        }
                        else
                        {
                            //校验CheckSum值--UploadStream流的一致性
                            long originPosition = uploadStream.Position;
                            bool flag =  this.FileStatus.CheckSum.Equals(CommonUtil.Base64Md5(uploadStream));
                            uploadStream.Seek(originPosition, SeekOrigin.Begin);
                            return flag;
                        }
                    }
                    catch (Exception ex)
                    {
                        ObsException e = new ObsException(ex.Message, ex.InnerException);
                        e.ErrorType = ErrorType.Sender;
                        throw e;
                    }
                }
            }
            return true;
        }
    }


    /// <summary>
    /// 上传文件的状态
    /// </summary>
    [XmlRoot("FileStatus")]
    public class FileStatus
    {
        /// <summary>
        /// 上传文件的上次修改时间
        /// </summary>
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// LastModified为null时，忽略序列化该对象
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeLastModified()
        {
            return LastModified.HasValue;
        }

        /// <summary>
        /// 上传文件的大小
        /// </summary>
        [XmlElement("Size")]
        public long Size { get; set; }

        /// <summary>
        /// 上传文件的MD5
        /// </summary>
        [XmlElement("CheckSum")]
        public string CheckSum { get; set; }

        /// <summary>
        /// 获取待上传文件/数据流的状态信息
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="uploadStream"></param>
        /// <param name="checkSum"></param>
        /// <returns></returns>
        public static FileStatus getFileStatus(string uploadFile, Stream uploadStream, bool checkSum, ResumableUploadTypeEnum uploadType)
        {
            FileStatus fileStatus = new FileStatus();

            if (uploadType == ResumableUploadTypeEnum.UploadFile)
            {
                FileInfo fileInfo = new FileInfo(uploadFile);
                fileStatus.Size = fileInfo.Length;
                fileStatus.LastModified = fileInfo.LastWriteTime;
            }
            else
            {
                fileStatus.Size = uploadStream.Length - uploadStream.Position;
                //数据流方式LastModified置为空
                fileStatus.LastModified = null;
            }

            //若开启文件内容校验
            if (checkSum)
            {
                try
                {
                    if (uploadType == ResumableUploadTypeEnum.UploadFile)
                    {
                        using (FileStream fileStream = new FileStream(uploadFile, FileMode.Open))
                        {
                            //计算UploadFile的hash值
                            fileStatus.CheckSum = CommonUtil.Base64Md5(fileStream);
                        }
                    }
                    else
                    {
                        //计算UploadStream的hash值
                        long originPosition = uploadStream.Position;
                        fileStatus.CheckSum = CommonUtil.Base64Md5(uploadStream);
                        uploadStream.Seek(originPosition, SeekOrigin.Begin);
                    }
                }
                catch(Exception ex)
                {
                    ObsException e = new ObsException(ex.Message, ex.InnerException);
                    e.ErrorType = ErrorType.Sender;
                    throw e;
                }
            }

            return fileStatus;
        }
    }

    /// <summary>
    /// 分段信息
    /// </summary>
    [XmlRoot("UploadPart")]
    public class UploadPart
    {
        /// <summary>
        /// 分段序号
        /// </summary>
        [XmlElement("PartNumber")]
        public int PartNumber { set; get; }

        /// <summary>
        /// 分段在文件中的偏移
        /// </summary>
        [XmlElement("Offset")]
        public long Offset { set; get; }

        /// <summary>
        /// 分段大小
        /// </summary>
        [XmlElement("Size")]
        public long Size { set; get; }

        /// <summary>
        /// 分段是否已上传完成
        /// </summary>
        [XmlElement("IsCompleted")]
        public bool IsCompleted { set; get; }

    }


    /// <summary>
    /// HashCode计算
    /// </summary>
    public static class ComputeHash
    {
        public static string HashCode<T>(T obj)
        {
            StringBuilder sb = new StringBuilder();

            Type type = obj.GetType();

            foreach (var property in type.GetProperties())
            {
                //UploadCheckPoint类的hash值计算不包含属性Md5和UploadStream
                if (property.Name.Equals("Md5") || property.Name.Equals("UploadStream"))
                    continue;

                sb.Append(property.Name + ":" + property.GetValue(obj, null));
            }

            byte[] content = Encoding.UTF8.GetBytes(sb.ToString());
            return CommonUtil.Base64Md5(content);
        }
    }


    /// <summary>
    /// 断点续传上传类型：文件/数据流
    /// </summary>
    public enum ResumableUploadTypeEnum
    {
        UploadFile,
        UploadStream,
    }

}
