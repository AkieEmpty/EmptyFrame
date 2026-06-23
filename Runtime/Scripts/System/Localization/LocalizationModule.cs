using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame
{
    public class LocalizationModule:MonoBehaviour
    {
        

        [SerializeField]
        [LabelText("语言")] private LanguageType languageType;

        private LocalizationSystemSetting setting;

        //运行时本地化数据缓存
        private Dictionary<string, string> localizationCacheDic = new Dictionary<string, string>();
        public event Action LanguageChangedAction;

        public LanguageType LanguageType
        {
            get=> languageType;
            set
            {
                if (languageType == value) return;
                languageType = value;

                OnLanguageValueChanged();
            }
        }
        private void OnValidate()
        {
            if (setting == null) return;
            OnLanguageValueChanged();
        }
        public void Init()
        {
            setting = EmptyFrameRoot.Setting.LocalizationSystemSetting;

            UpdateLocalizationCache(languageType);
        }

        private void UpdateLocalizationCache(LanguageType languageType)
        {
            localizationCacheDic.Clear();

            foreach (LocalizationConfig config in setting.localizationConfigDic.Values)
            {
                if (config == null)continue;

                foreach (var dic in config.LocalizationConfigDic)
                {
                    string key = dic.Key;

                    if (localizationCacheDic.ContainsKey(key))
                    {
                        Debug.LogError( $"[{nameof(LocalizationModule)}] 重复Key:{key},Config:{config.name}");
                        continue;
                    }

                    // 获取当前语言文本
                    if (dic.Value.TryGetValue(languageType, out string value))
                    {
                        localizationCacheDic.Add(key, value);
                    }
                    else Debug.LogWarning($"[{nameof(LocalizationModule)}] 缺少类型: [{languageType}] 的语言配置");
                }
            }
        }

        /// <summary>
        /// 添加本地化
        /// </summary>
        /// <param name="groupName">分组名</param>
        public void AddLocalization(string groupName, LocalizationConfig config)
        {
            setting.AddLocalizationConfig(groupName, config);

            OnLanguageValueChanged();
        }
        /// <summary>
        /// 移除本地化
        /// </summary>
        /// <param name="groupName">分组名</param>
        public void RemoveLocalization(string groupName)
        {
            setting.RemoveLocalizationConfig(groupName);

            OnLanguageValueChanged();
        }

        public void RegisterLanguageEvent(Action action)
        {
            LanguageChangedAction += action;
        }
        public void UnregisterLanguageEvent(Action action)
        {
            LanguageChangedAction -= action;
        }

        public string GetContent(string key)
        {
            if(localizationCacheDic.TryGetValue(key,out string content))
            {
                return content;
            }
            Debug.LogWarning($"[{nameof(LocalizationModule)}] 未找到本地化Key:{key}");
            return key;
        }

        private void OnLanguageValueChanged()
        {
            UpdateLocalizationCache(languageType);

            LanguageChangedAction?.Invoke();
        }
    }


}

