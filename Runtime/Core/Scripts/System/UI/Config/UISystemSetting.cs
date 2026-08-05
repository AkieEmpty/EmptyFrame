using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EmptyFrame.Core
{
    [CreateAssetMenu(menuName = "EmptyFrame/Setting/UISystemSetting",fileName = "UISystemSetting")]
    [HideMonoScript]

    internal partial class UISystemSetting : FrameSettingBase
    {
        public UISystemSetting() => Title = "<b>UI</b>";


        [LabelText("窗口定义映射"), PropertySpace(SpaceBefore = 0)]
        [DictionaryDrawerSettings(KeyLabel = "窗口类名", ValueLabel = "窗口定义")]
        public Dictionary<string, UIWindowDefinition> WindowDefinitionDic = new Dictionary<string, UIWindowDefinition>();
    }

#if UNITY_EDITOR

    internal partial class UISystemSetting
    {
        private const string UISystemSettingKey = "UISystemSetting";

        [PropertySpace]
        [Button("重新加载", ButtonSizes.Small,ButtonHeight = 25)]
        [InitializeOnLoadMethod]
        public static void InitForEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string[] guids = AssetDatabase.FindAssets($"{UISystemSettingKey} t:ScriptableObject");
            if (guids.Length <= 0)
            {
                Debug.LogWarning($"找不到 {nameof(UISystemSetting)} 配置文件。");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var setting = AssetDatabase.LoadAssetAtPath<UISystemSetting>(path);
            setting.InitUIWindowDataDicOnEditor();
        }

        public void Reset()
        {
            InitUIWindowDataDicOnEditor();
        }

        public void InitUIWindowDataDicOnEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            WindowDefinitionDic.Clear();

            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            Type baseType = typeof(UIWindowBase);

            foreach (Assembly assembly in asms)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (!baseType.IsAssignableFrom(type) || type.IsAbstract) continue;

                    IEnumerable<UIWindowDefinitionAttribute> attributes = type.GetCustomAttributes<UIWindowDefinitionAttribute>();
                    foreach (UIWindowDefinitionAttribute attribute in attributes)
                    {
                        UIWindowDefinition def = new UIWindowDefinition
                        {
                            WindowKey = attribute.WindowKey,
                            ConfigKey = attribute.ConfigKey,
                            IsCached = attribute.IsCached,
                            Layer = attribute.Layer,
                        };

                        WindowDefinitionDic.Add(attribute.WindowKey, def);
                    }
                }
            }
        }
    }
#endif
}
