using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame
{
    public class LogSystemSetting
    {
        [LabelText("缓存最大数量")]public int MaxLogCount = 1000;

        [LabelText("启用堆栈记录")] public bool EnableStackTrace = true;

        [LabelText("启用运行时日志窗口")] public GameObject LogWindowPrefab ;

        [HideInInspector]
        public Dictionary<LogType, Color> logColorDic = new Dictionary<LogType, Color>()
        {
            { LogType.Debug, Color.white },
            { LogType.Log, Color.green },
            { LogType.Warning, Color.yellow },
            { LogType.Error, Color.red },
            { LogType.Assert, Color.red },
            { LogType.Exception, Color.red },
        };
    }
}
