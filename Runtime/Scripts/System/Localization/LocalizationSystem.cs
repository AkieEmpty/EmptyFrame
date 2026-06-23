using System;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 本地化系统
    /// </summary>
    public static class LocalizationSystem
    {
        private static LocalizationModule module;
        /// <summary>
        /// 当前语言
        /// </summary>
        public static LanguageType LanguageType { get=> module.LanguageType; set=> module.LanguageType = value; }

        /// <summary>
        /// 语言更新事件
        /// </summary>
        public static event Action LanguageChanged
        {
            add => module.LanguageChangedAction += value;
            remove => module.LanguageChangedAction -= value;
        }
        public static void Init()
        {
            module = EmptyFrameRoot.RootTransform.GetComponentInChildren<LocalizationModule>();
            if (module == null) Debug.LogError($"[LocalizationSystem] ，未找到组件: {nameof(LocalizationModule)} 。");
            module.Init();
        }
        /// <summary>
        /// 添加本地化配置
        /// </summary>
        /// <param name="groupName">分组</param>
        public static void AddLocalization(string groupName, LocalizationConfig config)
        {
            module.AddLocalization(groupName, config);
        }
        /// <summary>
        /// 移除本地化配置
        /// </summary>
        /// <param name="groupName">分组</param>
        public static void RemoveLocalization(string groupName)
        {
            module.RemoveLocalization(groupName);
        }
       
        /// <summary>
        /// 获取本地化文本内容
        /// </summary>
        /// <param name="localizationKey">本地化Key</param>
        public static string GetContent(string localizationKey)
        {
            return module.GetContent(localizationKey);
        }
    }
}