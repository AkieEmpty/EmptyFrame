namespace EmptyFrame.Core
{
    /// <summary>
    /// 日志处理器
    /// </summary>
    internal interface ILogHandler
    {
        void Write(LogData data);
        void OnStart();
        void OnEnd();
    }
}
