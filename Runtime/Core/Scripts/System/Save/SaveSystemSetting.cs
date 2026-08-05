using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame.Core
{
    [CreateAssetMenu(menuName = "EmptyFrame/Setting/SaveSystemSetting", fileName = "SaveSystemSetting")]
    [HideMonoScript]

    internal class SaveSystemSetting : FrameSettingBase
    {
        public SaveSystemSetting() => Title = "<b>存档</b>";
     
        [LabelText("存档类型"), PropertySpace(SpaceBefore = 0)]
        [InfoBox("修改存档类型会导致之前的存档无法读取", InfoMessageType.Warning)]
        public SaveType SaveType;
    }
}
