using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame
{
    public class SaveSystemSetting
    {
        [LabelText("存档类型")][Tooltip("修改后可能影响已有存档")]
        public SaveType SaveType;
    }
}
