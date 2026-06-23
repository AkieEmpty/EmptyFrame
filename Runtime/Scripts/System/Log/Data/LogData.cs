using System;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 日志数据
    /// </summary>
    public class LogData
    {
        public LogType Type;

        public LogSource Source;

        public string Message;

        public string StackTrace;

        public DateTime Time;

        public Color Color;
    }
}
