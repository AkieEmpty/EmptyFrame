using System.IO;
using UnityEditor;
using UnityEngine;

namespace EmptyFrame.Editor
{
    public class EmptyFrameMenu
    {
        [MenuItem("Unity/存档/打开存档文件夹")]
        private static void OpenSaveDirectory()
        {
            string savePath = Path.Combine(Application.persistentDataPath,"SaveData");

            if (!Directory.Exists(savePath))
            {
                Debug.LogWarning($"[{nameof(EmptyFrameMenu)}] 打开文件夹失败,文件路径:{savePath}");
                return;
            }

            EditorUtility.RevealInFinder(savePath);
        }
    }
}
