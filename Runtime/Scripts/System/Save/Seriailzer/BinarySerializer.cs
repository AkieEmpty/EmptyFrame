using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EmptyFrame
{
    /// <summary>
    /// 二进制序列化
    /// </summary>
    public class BinarySerializer : ISaveSerializer
    {
        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        public byte[] Serialize<T>(T data)
        {
            if (data == null) return null;

            using (MemoryStream stream = new MemoryStream())
            {
                // 将对象序列化成对象写进文件
                binaryFormatter.Serialize(stream, data);
                return stream.ToArray();
            }
        }
        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default;

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                // 将二进制文件返序列化成对象
                object obj = binaryFormatter.Deserialize(stream);
                return obj is T result ? result : default;
            }
        }

       
    }
}
