using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Sirenix.OdinInspector;
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
        
        [LabelText("窗口定义映射"), PropertySpace(SpaceBefore = 0),ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "窗口Key", ValueLabel = "窗口定义")]
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
            UISystemSetting setting = AssetDatabase.LoadAssetAtPath<UISystemSetting>(path);
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

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type baseType = typeof(UIWindowBase);

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (!baseType.IsAssignableFrom(type) || type.IsAbstract) continue;

                    IEnumerable<UIWindowDefinitionAttribute> attributes = type.GetCustomAttributes<UIWindowDefinitionAttribute>();
                    foreach (UIWindowDefinitionAttribute attribute in attributes)
                    {
                        UIWindowDefinition definition = new UIWindowDefinition(attribute.WindowKey, attribute.Layer, attribute.IsCached);

                        WindowDefinitionDic.Add(attribute.WindowKey, definition);
                    }
                }
            }
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

    }
#endif
}
