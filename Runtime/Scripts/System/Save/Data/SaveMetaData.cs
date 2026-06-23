

using System;

namespace EmptyFrame
{
    /// <summary>
    /// 存档元数据
    /// </summary>
    [Serializable]
    public class SaveMetaData:ISaveData
    {
        /// <summary>
        /// 存档唯一ID,用于对应存档目录
        /// </summary>
        public int Id;
        /// <summary>
        /// 存档名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 存档创建时间
        /// </summary>
        public long CreateTime;
        /// <summary>
        /// 存档最后写入时间
        /// </summary>
        public long LastWriteTime;
        public SaveMetaData(int id, string name, long createTime, long lastWriteTime) 
        {
            Id = id;
            Name = name;
            CreateTime = createTime;
            LastWriteTime = lastWriteTime;
        }
    }
}
