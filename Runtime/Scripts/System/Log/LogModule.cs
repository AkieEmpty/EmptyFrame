using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace EmptyFrame.Log
{
    public class LogModule 
    {
        private LogSystemSetting setting;
        private List<ILogHandler> outputs;
        private List<LogData> logDataCacheList = new List<LogData>();

        private bool ignoreUnityCallback;

        public List<LogData> LogDataCacheList { get => logDataCacheList; }

        public void Init(LogSystemSetting setting)
        {
            this.setting = setting;

            outputs = new List<ILogHandler>()
            {
                new ConsoleHandler(),
                new FileHandler(),
                new RuntimeWindowHandler(setting),
            };

            Application.logMessageReceived += OnUnityLog;

            Application.quitting += () =>
            {
                foreach (var output in outputs)
                {
                    output.OnEnd();
                }
                   
            };

            foreach (var output in outputs)
            {
                output.OnStart();
            }
        }
        [HideInCallstack]
        public void Write(LogType type, string message, string stackTrace = null)
        {
            if (string.IsNullOrEmpty(stackTrace) && setting.EnableStackTrace)
            {
                stackTrace = new StackTrace(2, true).ToString();
            }

            LogData logData = CreateLogData(type, LogSource.EmptyFrame, message, stackTrace);


            AddLog(logData);

            ignoreUnityCallback = true;

            try
            {
                foreach (var output in outputs)
                {
                    output.Write(logData);
                }
            }
            finally
            {
                ignoreUnityCallback = false;
            }
        }
        private void OnUnityLog(string condition, string stackTrace, UnityEngine.LogType type)
        {
            if (ignoreUnityCallback)
            {
                return;
            }

            LogData logData = CreateLogData(ConvertType(type), LogSource.Unity,condition, stackTrace);
           

            AddLog(logData);

            foreach (var output in outputs)
            {
                output.Write(logData);
            }
        }  
        private void AddLog(LogData data)
        {
            logDataCacheList.Add(data);

            while (logDataCacheList.Count > setting.MaxLogCount)
            {
                logDataCacheList.RemoveAt(0);
            }
        }
        public void ClearCache()
        {
            logDataCacheList.Clear();
        }
        private LogData CreateLogData(LogType type, LogSource source, string message, string stackTrace)
        {
            return new LogData
            {
                Type = type,
                Source = source,
                Message = message,
                Color = GetTextColor(type),
                StackTrace = stackTrace,
                Time = DateTime.Now,

            };
        }
        private Color GetTextColor(LogType type)
        {
            if (setting.logColorDic.TryGetValue(type, out Color color))
            {
                return color;
            }
            return Color.white;
        }
        private LogType ConvertType(UnityEngine.LogType type)
        {
            switch (type)
            {
                case UnityEngine.LogType.Error:
                    return LogType.Error;
                case UnityEngine.LogType.Assert:
                    return LogType.Assert;
                case UnityEngine.LogType.Exception:
                    return LogType.Exception;
                case UnityEngine.LogType.Warning:
                    return LogType.Warning;
                default:
                    return LogType.Log;
            }
        }
    }


}
