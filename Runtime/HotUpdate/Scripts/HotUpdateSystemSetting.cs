using System.Collections.Generic;
using System.Reflection;
using EmptyFrame.Core;
using Sirenix.OdinInspector;
using UnityEngine;


namespace EmptyFrame.HotUpdate
{
    [CreateAssetMenu(menuName = "EmptyFrame/Setting/HotUpdateSystemSetting",fileName = "HotUpdateSystemSetting")]
    [HideMonoScript]

    internal class HotUpdateSystemSetting : FrameSettingBase
    {
        public HotUpdateSystemSetting() => Title = "<b>热更新</b>";

        [LabelText("AOT程序集输出目录"), PropertySpace(SpaceBefore = 0)]
        public string AotBytesDir = "Assets/EmptyFrame/Runtime/BuiltIns/Dll/Aot";

        [LabelText("热更程序集输出目录")]
        public string HotUpdateBytesDir = "Assets/EmptyFrame/Runtime/BuiltIns/Dll/HotUpdate";

        [LabelText("AOT程序集"), PropertySpace()]
        public List<string> AotAssemblyNames = new List<string>()
        {
            "EmptyFrame.Core",
            "mscorlib",
            "System",
            "System.Core",
            "UnityEngine.CoreModule",
            "Unity.Addressables",
            "Unity.ResourceManager",
            "Unity.Netcode.Runtime",
            "Unity.Networking.Transport",
            "Unity.Collections",
        };

        [LabelText("热更程序集"), PropertySpace()]
        public List<string> HotUpdateAssemblyNames = new List<string>();

#if UNITY_EDITOR
        [OnInspectorGUI(nameof(DrawButtons))]
        private void DrawButtons()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("编译程序集", GUILayout.Height(30)))
            {
                InvokeBuilderMethod("Compile");
            }
            if (GUILayout.Button("部署程序集", GUILayout.Height(30)))
            {
                InvokeBuilderMethod("Deploy");
            }
            if (GUILayout.Button("编译并部署", GUILayout.Height(30)))
            {
                InvokeBuilderMethod("Build");
            }
            GUILayout.EndHorizontal();
        }

        private static void InvokeBuilderMethod(string methodName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType("EmptyFrame.Editor.HotUpdateBuilder");
                if (type != null)
                {
                    type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
                    break;
                }
            }
        }
#endif
    }
}
