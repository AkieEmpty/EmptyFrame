using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 存档系统
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveDirName = "Saves";
        private const string SettingDirName = "Setting";
        private static string saveRootPath;      //存档文件夹根路径
        private static string settingDirPath;    //系统设置文件夹
        private static string fileExtension;     //文件后缀
        private static ISaveSerializer serializer;
        

        //存档缓存
        private static Dictionary<int, Dictionary<string, ISaveData>> saveCacheDic = new Dictionary<int, Dictionary<string, ISaveData>>();

        public static void Init()
        {
            saveRootPath = Path.Combine(Application.persistentDataPath, Path.Combine("SaveData", SaveDirName));
            settingDirPath = Path.Combine(Application.persistentDataPath, Path.Combine("SaveData", SettingDirName));
            if (!Directory.Exists(saveRootPath)) Directory.CreateDirectory(saveRootPath);
            if (!Directory.Exists(settingDirPath)) Directory.CreateDirectory(settingDirPath);

             

            switch (EmptyFrameRoot.Setting.SaveSystemSetting.SaveType)
            {
                case SaveType.Binary:
                    serializer = new BinarySerializer();
                    fileExtension = ".bytes";
                    break;
                case SaveType.Json:
                    serializer = new JsonSerializer();
                    fileExtension = ".json";
                    break;
                default:
                    Debug.LogError($"[{nameof(SaveSystem)}] 未定义的存档文件类型 : {EmptyFrameRoot.Setting.SaveSystemSetting.SaveType}");
                    serializer = null;
                    break;
            }


        }

        #region 存档数据
        /// <summary>
        /// 创建一个新的存档
        /// </summary>
        /// <param name="saveName">存档名称</param>
        /// <returns>存档元数据</returns>
        public static SaveMetaData CreateSave(string saveName)
        {
            if (serializer == null) return null;

            if (!Directory.Exists(saveRootPath)) Directory.CreateDirectory(saveRootPath);

            //获取存档目录,查找分配新的存档ID
            int newId = GenerateSaveId();

            //创建存档槽文件夹
            string saveDirPath = GetSaveDirPath(newId);
            Directory.CreateDirectory(saveDirPath);

            //获取当前时间
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //生成存档数据
            SaveMetaData metaData = new SaveMetaData(newId, saveName, currentTime, currentTime);

            //写入磁盘
            string path = GetSaveMetaDataFilePath(newId);
            if (WriteFile(path, metaData))
            {
                return metaData;
            }
            return null;
        }
        /// <summary>
        /// 删除指定ID的存档
        /// </summary>
        public static void DeleteSave(int saveID)
        {
            string saveDirPath = GetSaveDirPath(saveID);
            if (Directory.Exists(saveDirPath)) Directory.Delete(saveDirPath, true);

            RemoveCacheData(saveID);
        }
        /// <summary>
        /// 获取所有存档元数据,不进行任何排序。
        /// </summary>
        public static List<SaveMetaData> GetAllSaves()
        {
            List<SaveMetaData> result = new List<SaveMetaData>();

            if (serializer == null || !Directory.Exists(saveRootPath)) return result;
            //获取存档目录
            string[] dirs = Directory.GetDirectories(saveRootPath);
            foreach (string dir in dirs)
            {
                string metaDataPath = GetSaveMetaDataFilePath(dir); ;

                if (!File.Exists(metaDataPath)) continue;

                //反序列化存档数据
                SaveMetaData metaData = ReadFile<SaveMetaData>(metaDataPath);
                if (metaData != null) result.Add(metaData);
            }

            return result;
        }
        /// <summary>
        /// 获取所有存档元数据，并按照最后写入时间排序
        /// </summary>
        /// <param name="sortType">排序方式</param>
        public static List<SaveMetaData> GetAllSavesByLastWriteTime(SortType sortType)
        {
            return GetSortedSaves((a, b) =>
            {
                return sortType == SortType.Ascending
               ? a.LastWriteTime.CompareTo(b.LastWriteTime)
               : b.LastWriteTime.CompareTo(a.LastWriteTime);
            });
        }
        /// <summary>
        /// 获取所有存档元数据，并按照创建时间排序
        /// </summary>
        /// <param name="sortType">排序方式</param>
        public static List<SaveMetaData> GetAllSavesByCreateTime(SortType sortType)
        {
            return GetSortedSaves((a, b) =>
            {
                return sortType == SortType.Ascending
                ? a.CreateTime.CompareTo(b.CreateTime)
                : b.CreateTime.CompareTo(a.CreateTime);
            });
        }
        /// <summary>
        /// 获取所有存档元数据，并按照名称排序
        /// </summary>
        /// <param name="sortType">排序方式</param>
        public static List<SaveMetaData> GetAllSavesByName(SortType sortType)
        {
            return GetSortedSaves((a, b) =>
            {
                return sortType == SortType.Ascending
                ? string.Compare(a.Name, b.Name)
                : string.Compare(b.Name, a.Name);
            });
        }
        /// <summary>
        /// 获取所有存档元数据，并使用自定义排序器进行排序
        /// </summary>
        /// <param name="comparer">自定义排序器</param>
        public static List<SaveMetaData> GetAllSavesByComparer(IComparer<SaveMetaData> comparer)
        {
            if (comparer == null)
            {
                Debug.LogWarning($"[{nameof(SaveSystem)}] 自定义排序器为空");
                return null;
            }
            return GetSortedSaves(comparer.Compare);
        }
        /// <summary>
        /// 重命名存档
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="newName">新存档名</param>
        public static void RenameSave(int saveId, string newName)
        {
            if (serializer == null) return;

            string path = GetSaveMetaDataFilePath(saveId);

            SaveMetaData data = LoadObject<SaveMetaData>(path);
            if (data == null || data.Name == newName) return;
            data.Name = newName;
            //直接修改时间，这里保存失败不会污染磁盘中的数据
            data.LastWriteTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            SaveObject<SaveMetaData>(path, data);
        }
        #endregion

        #region 存档缓存
        /// <summary>
        /// 设置存档缓存数据,若缓存已存在则覆盖原数据
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="fileName">文件名</param>
        /// <param name="data">缓存数据</param>
        private static void SetCacheData<T>(int saveID, string fileName, T data) where T : class, ISaveData
        {
            if (!saveCacheDic.TryGetValue(saveID, out var dic))
            {
                dic = new Dictionary<string, ISaveData>();
                saveCacheDic[saveID] = dic;
            }
            dic[fileName] = data;
        }
        /// <summary>
        /// 获取存档缓存数据
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="fileName">文件名</param>
        private static T GetCacheData<T>(int saveID, string fileName) where T : class, ISaveData
        {
            if (saveCacheDic.TryGetValue(saveID, out var dic))
            {
                if (dic.TryGetValue(fileName, out ISaveData data))
                {
                    return data as T;
                }
            }
            return null;
        }
        /// <summary>
        /// 移除指定存档的全部缓存数据
        /// </summary>
        /// <param name="saveID">存档ID</param>
        private static void RemoveCacheData(int saveID)
        {
            saveCacheDic.Remove(saveID);
        }
        /// <summary>
        /// 移除指定存档中的缓存数据
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="fileName">文件名</param>
        private static void RemoveCacheData(int saveID, string fileName)
        {
            if(saveCacheDic.TryGetValue(saveID,out var dic))
            {
                dic.Remove(fileName);
                if(dic.Count==0) saveCacheDic.Remove(saveID);
            }
        }
        /// <summary>
        /// 清空所有存档缓存。
        /// </summary>
        public static void ClearCache()
        {
            saveCacheDic.Clear();
        }
        #endregion

        #region 业务数据
        /// <summary>
        /// 保存数据到指定存档中
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="fileName">文件名</param>
        /// <param name="data">待保存数据</param>
        public static void SaveData<T>(int saveId, string fileName, T data) where T : class, ISaveData
        {
            string saveDirPath = GetSaveDirPath(saveId);
            if (!Directory.Exists(saveDirPath)) Directory.CreateDirectory(saveDirPath);

            //获取数据文件路径
            string filePath = GetDataFilePath(saveId, fileName);

            //写入数据,成功则更新存档写入时间
            if (SaveObject(filePath, data))
            {
                UpdateLastWriteTime(saveId);

                //添加缓存
                SetCacheData<T>(saveId, fileName, data);
            }

        }
        public static void SaveData<T>(int saveId, T data) where T : class, ISaveData
        {
            SaveData<T>(saveId, typeof(T).Name, data);
        }

        /// <summary>
        /// 从指定存档中加载数据。
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="fileName">文件名</param>
        public static T LoadData<T>(int saveId, string fileName) where T : class, ISaveData
        {
            //获取缓存
            T data = GetCacheData<T>(saveId, fileName);
            if (data != null) return data;

            //从磁盘中加载
            string filePath = GetDataFilePath(saveId, fileName);
            data = LoadObject<T>(filePath);

            //添加缓存
            if (data != null) SetCacheData<T>(saveId, fileName, data);

            return data;
        }
        public static T LoadData<T>(int saveId) where T : class, ISaveData
        {
            return LoadData<T>(saveId, typeof(T).Name);
        }
        /// <summary>
        /// 从指定存档中删除数据。
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="fileName">文件名</param>
        public static void DeleteData<T>(int saveId, string fileName) where T : class, ISaveData
        {
            //移除缓存
            RemoveCacheData(saveId, fileName);

            string filePath = GetDataFilePath(saveId, fileName);

            if (File.Exists(filePath)) File.Delete(filePath);
        }
        public static void DeleteData<T>(int saveId) where T : class, ISaveData
        {
            DeleteData<T>(saveId, typeof(T).Name);
        }
        public static void DeleteData<T>(SaveMetaData saveData, string fileName) where T : class, ISaveData
        {
            DeleteData<T>(saveData.Id, fileName);
        }
        public static void DeleteData<T>(SaveMetaData saveData) where T : class, ISaveData
        {
            DeleteData<T>(saveData.Id, typeof(T).Name);
        }
        #endregion

        #region 系统设置
        /// <summary>
        /// 保存系统设置
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="data">数据</param>
        public static void SaveSetting<T>(string fileName, T data) where T : class, ISaveData
        {
            string filePath = GetSettingFilePath(fileName);

            SaveObject(filePath, data);
        }
        public static void SaveSetting<T>(T data) where T : class, ISaveData
        {
            SaveSetting(typeof(T).Name, data);
        }

        /// <summary>
        /// 读取系统设置
        /// </summary>
        /// <param name="fileName">文件名</param>
        public static T LoadSetting<T>(string fileName) where T : class, ISaveData
        {
            string filePath = GetSettingFilePath(fileName);
            return LoadObject<T>(filePath);
        }
        public static T LoadSetting<T>() where T : class, ISaveData
        {
            return LoadSetting<T>(typeof(T).Name);
        }
        #endregion

        #region 对象持久化
        /// <summary>
        /// 将对象保存到指定文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="data">待保存的数据</param>
        private static bool SaveObject<T>(string filePath, T data) where T : class, ISaveData
        {
            if (serializer == null) return false;

            if (data == null)
            {
                Debug.LogWarning($"[{nameof(SaveSystem)}] 试图保存空数据");
                return false;
            }

            return WriteFile(filePath, data);
        }
        /// <summary>
        /// 从指定文件读取对象
        /// </summary>
        /// <param name="filePath">文件路径</param>
        private static T LoadObject<T>(string filePath) where T : class, ISaveData
        {
            if (serializer == null) return null;

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[{nameof(SaveSystem)}] 找不到文件,路径: {filePath}");
                return null;
            }

            return ReadFile<T>(filePath);
        }
        #endregion

        #region 文件读写
        /// <summary>
        /// 写入文件
        /// </summary>
        private static bool WriteFile<T>(string filePath, T data) where T : class, ISaveData
        {
            try
            {
                File.WriteAllBytes(filePath, serializer.Serialize(data));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(SaveSystem)}] 写入失败，路径: {filePath}。原因: {e.Message}");
                return false;
            }
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        private static T ReadFile<T>(string filePath) where T : class, ISaveData
        {
            try
            {
                return serializer.Deserialize<T>(File.ReadAllBytes(filePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(SaveSystem)}] 读取失败，路径: {filePath}。原因: {e.Message}");
                return null;
            }
        }
        #endregion

        #region 路径
        /// <summary>
        /// 获取指定存档中业务数据文件的完整路径
        /// </summary>
        /// <param name="saveId">存档ID</param>
        /// <param name="fileName">文件名</param>
        private static string GetDataFilePath(int saveId, string fileName)
        {
            return Path.Combine(GetSaveDirPath(saveId), fileName + fileExtension);
        }
        /// <summary>
        /// 获取指定存档元数据文件的完整路径
        /// </summary>
        /// <param name="saveId">存档ID</param>
        private static string GetSaveMetaDataFilePath(int saveId)
        {
            string saveDirPath = GetSaveDirPath(saveId);
            return GetSaveMetaDataFilePath(saveDirPath);
        }
        /// <summary>
        /// 根据存档目录路径获取元数据文件的完整路径
        /// </summary>
        /// <param name="saveDirPath">存档目录路径</param>
        private static string GetSaveMetaDataFilePath(string saveDirPath)
        {
            return Path.Combine(saveDirPath, nameof(SaveMetaData) + fileExtension);
        }
        /// <summary>
        /// 获取系统设置文件完整路径
        /// </summary>
        /// <param name="fileName">文件名</param>
        private static string GetSettingFilePath(string fileName)
        {
            return Path.Combine(settingDirPath, fileName + fileExtension);
        }
        /// <summary>
        /// 获取指定存档对应的目录路径
        /// </summary>
        /// <param name="saveId">存档ID</param>
        private static string GetSaveDirPath(int saveId)
        {
            return Path.Combine(saveRootPath, saveId.ToString());
        }
        #endregion

        #region 工具
        /// <summary>
        /// 更新存档最后的写入时间
        /// </summary>
        /// <param name="saveId">存档ID</param>
        private static void UpdateLastWriteTime(int saveId)
        {
            string path = GetSaveMetaDataFilePath(saveId);

            SaveMetaData metaData = LoadObject<SaveMetaData>(path);
            if (metaData == null) return;

            metaData.LastWriteTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            WriteFile(path, metaData);
        }
        /// <summary>
        /// 获取排序后的存档元数据集合
        /// </summary>
        /// <param name="comparison">排序比较器</param>
        private static List<SaveMetaData> GetSortedSaves(Comparison<SaveMetaData> comparison)
        {
            List<SaveMetaData> saves = GetAllSaves();

            saves.Sort(comparison);

            return saves;
        }
        /// <summary>
        /// 生成一个新的可用存档ID
        /// </summary>
        private static int GenerateSaveId()
        {
            string[] dirs = Directory.GetDirectories(saveRootPath);
            List<int> usedIds = new List<int>();

            // 收集所有已使用的存档ID
            foreach (string dir in dirs)
            {
                if (int.TryParse(Path.GetFileName(dir), out int id))
                {
                    usedIds.Add(id);
                }
            }

            //没有任何存档时，从 1 开始
            if (usedIds.Count == 0) return 1;
            usedIds.Sort();

            //依次查找最小可用ID
            int nextId = 1;
            foreach (int id in usedIds)
            {
                if (id == nextId) nextId++;
                else return nextId;
            }

            //没有空缺时，返回最大值 + 1
            return nextId;
        }
        #endregion
    }
}
