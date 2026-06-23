#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;

namespace EmptyFrame.Log
{
    /// <summary>
    /// 控制台日志重定向器
    /// </summary>
    public static class ConsoleLogRedirector
    {

        [OnOpenAsset(1)] // 优先级设为 1，确保在 Unity 默认打开逻辑之前执行
        public static bool OnOpenAsset(int instanceID, int line)
        {
            //检查当前双击的目标是否为控制台输出的类
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            if (!assetPath.EndsWith("ConsoleHandler.cs")) return false;

            //利用反射获取当前 Console 窗口实例及选中的日志文本
            Type consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            object consoleWindowInstance = fieldInfo.GetValue(null);

            if (consoleWindowInstance == null) return false;
            if ((EditorWindow)consoleWindowInstance != EditorWindow.focusedWindow) return false;

            //获取当前选中日志在底部面板显示的完整文本
            FieldInfo activeTextField = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            string activeText = activeTextField.GetValue(consoleWindowInstance) as string;

            if (string.IsNullOrEmpty(activeText)) return false;

            //正则解析 C# StackTrace 生成的文件路径和行号
            //格式通常为: "in 路径:line 行号" 或 "in 路径:行号"
            MatchCollection matches = Regex.Matches(activeText, @"in\s+(.*?):line\s+(\d+)", RegexOptions.IgnoreCase);
            if (matches.Count == 0)
            {
                matches = Regex.Matches(activeText, @"in\s+(.*?):(\d+)", RegexOptions.IgnoreCase);
            }

            if (matches.Count > 0)
            {
                Match firstMatch = matches[0];
                string filePath = firstMatch.Groups[1].Value.Trim();
                string lineStr = firstMatch.Groups[2].Value.Trim();

                if (int.TryParse(lineStr, out int lineNum))
                {
                    //将绝对路径裁剪转换为 Unity 相对路径
                    if (filePath.Contains("Assets")) filePath = filePath.Substring(filePath.IndexOf("Assets"));

                    //打开真正的业务脚本并精准定位到行
                    UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(filePath);
                    if (asset != null)
                    {
                        AssetDatabase.OpenAsset(asset, lineNum);
                        return true; 
                    }
                }
            }

            return false;
        }
    }
}
#endif
