using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace EmptyFrame
{
    [CreateAssetMenu(menuName ="Unity/Setting/EmptyFrameSetting",fileName = "EmptyFrameSetting")]
    [HideMonoScript]
    public partial class EmptyFrameSetting : SerializedScriptableObject
    {
        [Title("框架设置", titleAlignment: TitleAlignments.Centered)]

        [OdinSerialize, HideReferenceObjectPicker, Space(5)]
        [LabelText("存档设置")] public SaveSystemSetting SaveSystemSetting;

        [OdinSerialize, HideReferenceObjectPicker, Space(5)]
        [LabelText("UI设置")] public UISystemSetting UISystemSetting;

        [OdinSerialize, HideReferenceObjectPicker, Space(5)]
        [LabelText("本地化设置")] public  LocalizationSystemSetting LocalizationSystemSetting;


        [OdinSerialize, HideReferenceObjectPicker, Space(5)]
        [LabelText("日志设置")] public LogSystemSetting LogSystemSetting;
        

    } 
}
