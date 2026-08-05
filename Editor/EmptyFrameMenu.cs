using System.IO;
using UnityEditor;
using UnityEngine;

namespace EmptyFrame.Editor
{
    public class EmptyFrameMenu
    {
        [MenuItem("EmptyFrame/文件夹/打开持久化路径", false, 0)]
        private static void OpenPersistentDataPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath + "/");
        }

        [MenuItem("EmptyFrame/文件夹/打开存档文件夹", false, 1)]
        private static void OpenSaveDirectory()
        {
            string path = Path.Combine(Application.persistentDataPath, "SaveData/");

            if (!Directory.Exists(path))
            {
                Debug.LogWarning($"打开文件夹失败,文件路径:{path}");
                return;
            }

            EditorUtility.RevealInFinder(path);
        }

    }
}
 