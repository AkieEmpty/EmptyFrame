using EmptyFrame.Core;
using EmptyFrame.HotUpdate;
using EmptyFrame.Network;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace EmptyFrame.Editor
{
    public class EmptyFrameSettingWindow : OdinMenuEditorWindow
    {
        [MenuItem("EmptyFrame/设置")]
        private static void Open()
        {
            var window = GetWindow<EmptyFrameSettingWindow>("EmptyFrame 设置");
            window.titleContent = new GUIContent("EmptyFrame 设置", EditorGUIUtility.IconContent("SettingsIcon").image);
            window.position = new Rect(Screen.width / 2 - 400, Screen.height / 2 - 300, 800, 600);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false);
            tree.Config.DrawSearchToolbar = true;

            AddSetting<LogSystemSetting>(tree, "日志");
            AddSetting<SaveSystemSetting>(tree, "存档");
            AddSetting<AudioSystemSetting>(tree, "音频");
            AddSetting<LocalizationSystemSetting>(tree, "本地化");
            AddSetting<UISystemSetting>(tree, "UI");
            AddSetting<HotUpdateSystemSetting>(tree, "热更新");
            AddSetting<NetworkSystemSetting>(tree, "网络");

            return tree;
        }

        private static void AddSetting<T>(OdinMenuTree tree, string path) where T : ScriptableObject
        {
            var setting = LoadSetting<T>();
            if (setting != null)
            {
                tree.Add(path, setting);
            }
            else
            {
                Debug.LogWarning($"未找到 {typeof(T).Name}");
            }
        }

        private static T LoadSetting<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"{typeof(T).Name} t:ScriptableObject");
            if (guids.Length <= 0) return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
