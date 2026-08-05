using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame.Core
{
    [CreateAssetMenu(menuName = "EmptyFrame/Setting/LogSystemSetting",fileName = "UISystemSetting")]
    [HideMonoScript]

    internal class LogSystemSetting : FrameSettingBase
    {
        public LogSystemSetting() => Title = "<b>日志</b>";

        [LabelText("缓存最大数量"), PropertySpace(SpaceBefore = 0)]
        public int MaxLogCount = 1000;

        [LabelText("启用堆栈记录"), PropertySpace()]
        public bool EnableStackTrace = true;

        [LabelText("启用运行时日志窗口"), PropertySpace()]
        public GameObject LogWindowPrefab;

        [LabelText("日志颜色"), PropertySpace()]
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
