using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 控制台处理器
    /// </summary>
    public class ConsoleHandler : ILogHandler
    {
        [HideInCallstack]
        public void Write(LogData data)
        {
            if (data.Source == LogSource.Unity) return;
  

            string typeStr = $"[{data.Type}]";
            string message = $"{data.Message}";
            string colorCode = "#" + ColorUtility.ToHtmlStringRGBA(data.Color);
            string log = $"<color={colorCode}>{typeStr} {message}</color>";



            if (!string.IsNullOrEmpty(data.StackTrace))
            {
                log += "\n" + data.StackTrace;
            }
  

            switch (data.Type) 
            {
                case LogType.Debug:
                case LogType.Log:
                    Debug.Log(log);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(log);
                    break;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    Debug.LogError(log);
                    break;
            }
        }

        public void OnEnd() { }

        public void OnStart() { }
    }
}
