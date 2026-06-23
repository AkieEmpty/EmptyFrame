using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace EmptyFrame
{
    /// <summary>
    /// 资源系统
    /// </summary>
    public static class ResSystem
    {
        public static void Init() { }

        #region 资源加载

        /// <summary>
        /// 同步加载
        /// </summary>
        public static T LoadAssetSync<T>(string assetName) where T : UnityEngine.Object
        {
            return Addressables.LoadAssetAsync<T>(assetName).WaitForCompletion();
        }
        public static IList<T> LoadAssetsSync<T>(string label) where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            IList<T> result = handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ResSystem] 批量加载失败 (Label): {label}");
                return null;
            }

            return result;
        }
        public static IList<T> LoadAssetsSync<T>(IList<string> keys, Addressables.MergeMode mode = Addressables.MergeMode.Union) where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetsAsync<T>(keys, null, mode);
            IList<T> result = handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ResSystem] 批量加载失败 (Keys 列表)");
                return null;
            }

            return result;
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        public static void LoadAssetAsync<T>(string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            Addressables.LoadAssetAsync<T>(assetName).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) callback?.Invoke(handle.Result);
                else
                {
                    Debug.LogError($"[ResSystem] 资源加载失败: {assetName}");
                    callback?.Invoke(null);
                }
            };
        }
        public static void LoadAssetsAsync<T>(string label, Action<IList<T>> callback) where T : UnityEngine.Object
        {
            Addressables.LoadAssetsAsync<T>(label, null).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) callback?.Invoke(handle.Result);
                else
                {
                    Debug.LogError($"[ResSystem] 批量加载失败 (Label): {label}");
                    callback?.Invoke(null);
                }
            };
        }
        public static void LoadAssetsAsync<T>(IList<string> assetNames, Action<IList<T>> callback, Addressables.MergeMode mode = Addressables.MergeMode.Union) where T : UnityEngine.Object
        {
            Addressables.LoadAssetsAsync<T>(assetNames, null, mode).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) callback?.Invoke(handle.Result);
                else
                {
                    Debug.LogError($"[ResSystem] 批量加载失败 (Keys 列表)");
                    callback?.Invoke(null);
                }
            };
        }
        #endregion

        #region 资源释放
        /// <summary>
        /// 释放资源
        /// </summary>
        public static void ReleaseAsset<T>(T obj) => Addressables.Release(obj);
        /// <summary>
        /// 释放因批量加载而生成的句柄
        /// </summary>
        public static void ReleaseAssetsHandle<TObject>(AsyncOperationHandle<TObject> handle) => Addressables.Release(handle);
        /// <summary>
        /// 回收游戏对象
        /// </summary>
        public static void Recycle(GameObject gameObject)
        {
            PoolSystem.PushGameObject(gameObject);
        }
        /// <summary>
        /// 自动销毁并释放实游戏对象
        /// </summary>
        /// <param name="gameObject"></param>
        private static void AutoReleaseAssetAction(GameObject gameObject)
        {
            Addressables.ReleaseInstance(gameObject);
        }
        #endregion

        #region GameObject
        /// <summary>
        /// 实例化游戏对象 (本地普通 GameObject)
        /// </summary>
        public static GameObject Instantiate(GameObject gameObject, Transform parent = null)
        {
            GameObject obj = PoolSystem.GetGameObject(gameObject, parent);

            if (obj == null)
            {
                obj = GameObject.Instantiate(gameObject, parent);
                obj.name = gameObject.name;
            }
            return obj;
        }
        public static T Instantiate<T>(GameObject gameObject, Transform parent = null)where T : Component
        {
            GameObject go = Instantiate(gameObject, parent);
            return go.GetComponent<T>();
        }
        /// <summary>
        /// 同步加载实例化游戏对象 (Addressables)
        /// </summary>
        public static GameObject InstantiateSync(string assetName, Transform parent = null)
        {
            GameObject go = PoolSystem.GetGameObject(assetName, parent);

            if (go == null)
            {
                go = Addressables.InstantiateAsync(assetName, parent).WaitForCompletion();

                if (go != null) 
                {
                    go.name = assetName;

                    if (!go.TryGetComponent(out AddressableAutoReleaseHelper helper))
                    {
                        helper = go.AddComponent<AddressableAutoReleaseHelper>();
                        helper.hideFlags = HideFlags.HideInInspector;
                    }
                    helper.Init(AutoReleaseAssetAction);
                } 
                else Debug.LogError($"[ResSystem] 实例化失败，找不到资源: {assetName}");
            }

            return go;
        }
        public static T InstantiateSync<T>(string assetName, Transform parent = null) where T : Component
        {
            GameObject obj = InstantiateSync(assetName, parent);
            return obj?.GetComponent<T>();
        }
        /// <summary>
        /// 异步加载实例化游戏对象 (Addressables)
        /// </summary>
        public static void InstantiateAsync(string assetName, Transform parent = null, Action<GameObject> callback = null)
        {
            GameObject go = PoolSystem.GetGameObject(assetName, parent);

            if (go != null)
            {
                callback?.Invoke(go);
                return;
            }

            Addressables.InstantiateAsync(assetName, parent).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    go = handle.Result;
                    go.name = assetName;

                    if(!go.TryGetComponent(out AddressableAutoReleaseHelper helper))
                    {
                        helper = go.AddComponent<AddressableAutoReleaseHelper>();
                        helper.hideFlags = HideFlags.HideInInspector;
                    }
                    helper.Init(AutoReleaseAssetAction);
                }
                else Debug.LogError($"[ResSystem] 实例化失败: {assetName}");

                callback?.Invoke(go);
            };
        }
        public static void InstantiateAsync(string assetName, Action<GameObject> callback = null)
        {
            InstantiateAsync(assetName, null, callback);
        }
        public static void InstantiateAsync<T>(string assetName, Transform parent = null, Action<T> callback = null) where T : Component
        {
            InstantiateAsync(assetName, parent, (go) =>
            {
                callback?.Invoke(go?.GetComponent<T>());
            });
        }
        public static void InstantiateAsync<T>(string assetName, Action<T> callback = null) where T : Component
        {
            InstantiateAsync<T>(assetName, null, callback);
        }

        #endregion

       
    }
}