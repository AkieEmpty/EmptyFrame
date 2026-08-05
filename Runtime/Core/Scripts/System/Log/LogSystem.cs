using System;
using UnityEngine;

namespace EmptyFrame.Core
{
   

    public static class LogSystem
    {
        private static LogModule logModule;

        public static void Init()
        {
            var setting = Resources.Load<LogSystemSetting>("LogSystemSetting");
            if (setting == null)
            {
                UnityEngine.Debug.LogError("找不到 LogSystemSetting 配置文件，请确保 Resources 文件夹中存在该资源");
                return;
            }
            logModule = new LogModule();
            logModule.Init(setting);
        }

        [HideInCallstack]
        public static void Debug(string message)
        {
            logModule.Write(LogType.Debug, message);
        }
        [HideInCallstack]
        public static void Log(string message)
        {
            logModule.Write(LogType.Log,message);
        }
        [HideInCallstack]
        public static void Warning(string message)
        {
            logModule.Write(LogType.Warning, message);
        }
        [HideInCallstack]
        public static void Error(string message)
        {
            logModule.Write(LogType.Error, message);
        }
        [HideInCallstack]
        public static void Exception(Exception exception)
        {
            logModule.Write(LogType.Exception, exception.Message, exception.StackTrace);
        }
        [HideInCallstack]
        public static void Assert(bool condition,string message)
        {
            if (condition) return;
            logModule.Write(LogType.Assert, message);
        }
    }
}
