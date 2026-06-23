using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace EmptyFrame
{
 
    public class LocalizationSystemSetting 
    {
        [DictionaryDrawerSettings( KeyLabel = "分组", ValueLabel = "配置", DisplayMode = DictionaryDisplayOptions.Foldout)]
        public Dictionary<string,LocalizationConfig> localizationConfigDic = new Dictionary<string,LocalizationConfig>();

        public void AddLocalizationConfig(string groupName,LocalizationConfig localizationConfig)
        {
            if (localizationConfig == null) return;

            if (!localizationConfigDic.ContainsKey(groupName))
            {
                localizationConfigDic.Add(groupName, localizationConfig);
            }
            else Debug.LogWarning($"[{nameof(LocalizationSystemSetting)} 分组重复 ：{groupName} ]");
        }

        public void RemoveLocalizationConfig(string groupName)
        {
            localizationConfigDic.Remove(groupName);
        }
    }
}
