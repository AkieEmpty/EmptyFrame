namespace EmptyFrame
{
    /// <summary>
    /// 存档数据序列化接口
    /// </summary>
    public interface ISaveSerializer
    {
        byte[] Serialize<T>(T data);

        T Deserialize<T>(byte[] bytes);
    }
}
