namespace EmptyFrame.Log
{
    /// <summary>
    /// 日志处理器
    /// </summary>
    public interface ILogHandler
    {
        void Write(LogData data);
        void OnStart();
        void OnEnd();
    }
}
