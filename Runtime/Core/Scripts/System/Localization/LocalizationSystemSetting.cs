using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame.Core
{
    [CreateAssetMenu(menuName = "EmptyFrame/Setting/LocalizationSystemSetting",fileName = "LocalizationSystemSetting")]
    [HideMonoScript]

    internal class LocalizationSystemSetting : FrameSettingBase
    {
        public LocalizationSystemSetting() => Title = "<b>本地化</b>";


        [LabelText("当前语言"), PropertySpace(SpaceBefore = 0)]
        public LanguageType DefaultLanguage;

        [LabelText("本地化配置数据"),PropertySpace()]
        [DictionaryDrawerSettings(KeyLabel = "分组", ValueLabel = "配置", DisplayMode = DictionaryDisplayOptions.Foldout)]
        public Dictionary<string, LocalizationConfig> localizationConfigDic = new Dictionary<string, LocalizationConfig>();

        public void AddLocalizationConfig(string groupName, LocalizationConfig localizationConfig)
        {
            if (localizationConfig == null) return;

            if (!localizationConfigDic.ContainsKey(groupName))
            {
                localizationConfigDic.Add(groupName, localizationConfig);
            }
            else LogSystem.Warning($"分组重复 ：{groupName}");
        }

        public void RemoveLocalizationConfig(string groupName)
        {
            localizationConfigDic.Remove(groupName);
        }
    }
}
