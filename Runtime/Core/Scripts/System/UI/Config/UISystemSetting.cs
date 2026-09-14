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

        [LabelText("预制体路径")]public string WindowPrefabPath = "Assets/UI";

        [LabelText("窗口定义映射"), PropertySpace(SpaceBefore = 0)]
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
                        UIWindowDefinition definition = new UIWindowDefinition
                        {
                            WindowKey = attribute.WindowKey,
                            IsCached = attribute.IsCached,
                            PrefabRef = GetPrefabReference(attribute.WindowKey),
                            Layer = attribute.Layer,
                        };

                        WindowDefinitionDic.Add(attribute.WindowKey, definition);
                    }
                }
            }
        }

        private AssetReferenceUIWindow GetPrefabReference(string windowKey)
        {
            if (string.IsNullOrEmpty(WindowPrefabPath))
            {
                Debug.LogError($"窗口预制体目录未配置");
                return null;
            }
            
            // 如果目录不存在，则逐级自动创建
            if (!AssetDatabase.IsValidFolder(WindowPrefabPath))
            {
                string[] folders = WindowPrefabPath.Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string nextPath = $"{currentPath}/{folders[i]}";

                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }

                    currentPath = nextPath;
                }

                AssetDatabase.Refresh();
            }
            
            string path = $"{WindowPrefabPath}/{windowKey}.prefab";
            string guid = AssetDatabase.AssetPathToGUID(path);
            
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"找不到窗口预制体：{path}");
                return null;
            }

            return new AssetReferenceUIWindow(guid);
        }
    }
#endif
}
