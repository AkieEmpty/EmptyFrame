using System.Text;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// Json序列化
    /// </summary>
    public class JsonSerializer : ISaveSerializer
    {
        public byte[] Serialize<T>(T data)
        {
            if (data == null) return null;
            // 对象 -> Json
            string jsonStr = JsonUtility.ToJson(data);
            // Json -> UTF8字节
            return Encoding.UTF8.GetBytes(jsonStr);
        }
        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default;
            // UTF8字节 -> Json
            string jsonStr = Encoding.UTF8.GetString(bytes);
            // Json -> 对象
            return JsonUtility.FromJson<T>(jsonStr);
        }
    }
}
