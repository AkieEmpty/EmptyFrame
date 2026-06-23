using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmptyFrame.Log
{
    /// <summary>
    /// 运行时窗口处理器
    /// </summary>
    public class RuntimeWindowHandler : ILogHandler
    {
        private UI_LogWindow window;
        private LogSystemSetting logSetting;

        private List<LogData> logDataList = new List<LogData>();

        public RuntimeWindowHandler(LogSystemSetting logSetting) 
        {
            this.logSetting = logSetting;
        }

        public void OnStart()
        {
            window = GameObject.Instantiate(logSetting.LogWindowPrefab,EmptyFrameRoot.RootTransform).GetComponent<UI_LogWindow>();
            window.gameObject.name = nameof(UI_LogWindow);
            window.Init(logDataList);
        }

     

        public void OnEnd() { }

        public void Write(LogData data)
        {
            logDataList.Add(data);

            while (logDataList.Count > logSetting.MaxLogCount)
            {
                logDataList.RemoveAt(0);
            }

            window.AddLog(data);
        }
       

    }
}
