using System;
using System.Collections.Generic;
using System.IO;
using EmptyFrame.HotUpdate;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace EmptyFrame.Editor
{
    /// <summary>
    /// 热更新部署
    /// 编译热更DLL,同步HybridCLR设置,搬运DLL,同步Addressables
    /// </summary>
    internal static class HotUpdateBuilder
    {
        /// <summary>
        /// 编译热更DLL并同步HybridCLR设置
        /// </summary>
        public static void Compile()
        {
            HotUpdateSystemSetting hotUpdateSetting = LoadHotUpdateSetting();
            if (hotUpdateSetting == null) return;

            //编译热更 DLL
            PrebuildCommand.GenerateAll();

            //同步 HybridCLR 设置
            SyncHybridCLRSettings(hotUpdateSetting);

            Debug.Log("热更程序集编译完成");
        }

        /// <summary>
        /// 搬运DLL并同步Addressables
        /// </summary>
        public static void Deploy()
        {
            HotUpdateSystemSetting hotUpdateSetting = LoadHotUpdateSetting();
            if (hotUpdateSetting == null) return;

            //搬运 DLL
            DeployDlls(hotUpdateSetting);
            AssetDatabase.Refresh();

            //同步 Addressables
            SyncAddressables(hotUpdateSetting);

            Debug.Log("程序集搬运完成");
        }

        /// <summary>
        /// 执行全部流程: 编译 + 搬运
        /// </summary>
        public static void Build()
        {
            Compile();
            Deploy();
        }

        private static HotUpdateSystemSetting LoadHotUpdateSetting()
        {
            string[] guids = AssetDatabase.FindAssets($"{nameof(HotUpdateSystemSetting)} t:ScriptableObject");
            if (guids.Length <= 0)
            {
                Debug.LogWarning($"找不到 {nameof(HotUpdateSystemSetting)} 配置文件。请通过 CreateAssetMenu 创建。");
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<HotUpdateSystemSetting>(path);
        }

        /// <summary>
        /// 同步HybridCLR设置
        /// </summary>
        /// <param name="hotUpdateSetting"></param>
        private static void SyncHybridCLRSettings(HotUpdateSystemSetting hotUpdateSetting)
        {
            if (hotUpdateSetting.AotAssemblyNames != null)
                HybridCLRSettings.Instance.patchAOTAssemblies = hotUpdateSetting.AotAssemblyNames.ToArray();
            if (hotUpdateSetting.HotUpdateAssemblyNames != null)
            {
                HybridCLRSettings.Instance.hotUpdateAssemblies = hotUpdateSetting.HotUpdateAssemblyNames.ToArray();
            }

            HybridCLRSettings.Save();
        }

        #region 搬运Dll
        /// <summary>
        /// 搬运Dll文件
        /// </summary>
        private static void DeployDlls(HotUpdateSystemSetting hotUpdateSetting)
        {
            string projectDir = Environment.CurrentDirectory;
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            string aotDllDir = Path.Combine(projectDir, SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget));
            string hotUpdateDllDir = Path.Combine(projectDir, SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget));

            string aotBytesDir = Path.Combine(projectDir, hotUpdateSetting.AotBytesDir);
            string hotUpdateBytesDir = Path.Combine(projectDir, hotUpdateSetting.HotUpdateBytesDir);
            
            Directory.CreateDirectory(aotBytesDir);
            Directory.CreateDirectory(hotUpdateBytesDir);

            DeployDllGroup(aotDllDir, aotBytesDir, "AOT", hotUpdateSetting.AotAssemblyNames);
            DeployDllGroup(hotUpdateDllDir, hotUpdateBytesDir, "HotUpdate", hotUpdateSetting.HotUpdateAssemblyNames);
        }

        private static void DeployDllGroup(string sourceDir, string targetDir, string groupLabel, IReadOnlyList<string> assemblyNames)
        {
            if (assemblyNames == null || assemblyNames.Count == 0)
            {
                Debug.LogWarning($"{groupLabel} 配置列表为空");
                return;
            }

            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError($"{groupLabel} 源目录不存在");
                return;
            }

            CheckOrCreateDirectory(targetDir);
            CleanOldBytesFiles(targetDir, groupLabel);

            List<string> filesToDeploy = GetDllPaths(sourceDir, assemblyNames, groupLabel);

            foreach (string dllPath in filesToDeploy)
            {
                string bytesFileName = Path.GetFileName(dllPath) + ".bytes";
                string bytesPath = Path.Combine(targetDir, bytesFileName);
                File.Copy(dllPath, bytesPath, true);
                AssetDatabase.ImportAsset(bytesPath.Replace("\\", "/"), ImportAssetOptions.ForceUpdate);
            }

            Debug.Log($"{groupLabel} 搬运完成: {filesToDeploy.Count} 个文件");
        }

        private static List<string> GetDllPaths(string sourceDir, IReadOnlyList<string> assemblyNames, string groupLabel)
        {
            List<string> result = new();

            foreach (string name in assemblyNames)
            {
                string dllPath = Path.Combine(sourceDir, name + ".dll");
                if (File.Exists(dllPath))
                {
                    result.Add(dllPath);
                }
                else
                {
                    Debug.LogWarning($"{groupLabel} 程序集未找到: {name}");
                }
            }

            return result;
        }

        private static void CheckOrCreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                Debug.Log($"已创建目录: {dirPath}");
            }
        }

        private static void CleanOldBytesFiles(string dirPath, string groupLabel)
        {
            string[] oldFiles = Directory.GetFiles(dirPath, "*.bytes");
            foreach (string file in oldFiles)
            {
                File.Delete(file);
            }

            if (oldFiles.Length > 0)
            {
                Debug.Log($"已清理 {groupLabel} 旧 .bytes 文件: {oldFiles.Length} 个");
            }
        }
        #endregion

        #region 同步 Addressables

        /// <summary>
        /// 同步Addressbels
        /// </summary>
        private static void SyncAddressables(HotUpdateSystemSetting hotUpdateSetting)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("未找到 Addressables 配置，请先初始化 Addressables");
                return;
            }

            SyncAddressablesGroup(settings, hotUpdateSetting.AotBytesDir, "AOT");
            SyncAddressablesGroup(settings, hotUpdateSetting.HotUpdateBytesDir, "HotUpdate");

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();
        }

        private static void SyncAddressablesGroup(AddressableAssetSettings settings, string bytesDir, string groupLabel)
        {
            if (!Directory.Exists(bytesDir)) return;

            AddressableAssetGroup group = settings.FindGroup(groupLabel)
                ?? settings.CreateGroup(groupLabel, false, false, true, null);

            ClearAddressablesGroup(settings, group);

            string[] bytesFiles = Directory.GetFiles(bytesDir, "*.bytes");
            foreach (string filePath in bytesFiles)
            {
                string assetPath = filePath.Replace("\\", "/");
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.SetAddress(Path.GetFileNameWithoutExtension(filePath), false);
            }

            Debug.Log($"{groupLabel} Addressables 同步完成: {bytesFiles.Length} 个条目");
        }

        private static void ClearAddressablesGroup(AddressableAssetSettings settings, AddressableAssetGroup group)
        {
            if (group == null || group.entries.Count == 0) return;

            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>(group.entries);
            foreach (AddressableAssetEntry entry in entries)
            {
                settings.RemoveAssetEntry(entry.guid);
            }

            Debug.Log($"{group.Name} 已清理 {entries.Count} 个旧条目");
        }

        #endregion
    }
}
