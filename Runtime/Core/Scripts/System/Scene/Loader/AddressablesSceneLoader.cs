using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace EmptyFrame.Core
{
    /// <summary>
    /// Addressables 场景加载器
    /// <para>
    /// 基于 Addressables 实现场景加载与卸载
    /// </para>
    /// </summary>
    internal class AddressablesSceneLoader : ISceneLoader
    {
        private readonly Dictionary<SceneHandle, SceneInstance> sceneInstanceDic = new Dictionary<SceneHandle, SceneInstance>();

        public SceneHandle LoadSceneSync(LoadSceneOptions options)
        {
            if (!CheckSceneIsValid(options.SceneKey)) return default;

            AsyncOperationHandle<SceneInstance> asyncOperation = Addressables.LoadSceneAsync(options.SceneKey, options.Mode, true);
            SceneInstance sceneInstance = asyncOperation.WaitForCompletion();

            SceneHandle handle = new SceneHandle(IdGenerator.Generate(), options.SceneKey, SceneProvider.Addressables);

            sceneInstanceDic.Add(handle, sceneInstance);

            return handle;
        }
        public LoadSceneOperation LoadSceneAsync(LoadSceneOptions options)
        {
            if (!CheckSceneIsValid(options.SceneKey)) return null;

            LoadSceneOperation handle = new LoadSceneOperation(options);

            MonoSystem.StartCoroutine(DoLoadSceneAsync(options.SceneKey, handle, options.Mode));

            return handle;
        }
        private IEnumerator DoLoadSceneAsync(string sceneKey, LoadSceneOperation operation, LoadSceneMode loadModel)
        {
            AsyncOperationHandle<SceneInstance> asyncOpHandle = Addressables.LoadSceneAsync(sceneKey, loadModel, false);

            //加载阶段
            operation.UpdateProgress(0);
            operation.ChangedState(SceneLoadState.Loading);
            while (!asyncOpHandle.IsDone)
            {
                operation.UpdateProgress(asyncOpHandle.PercentComplete); //更新进度
                yield return null;
            }

            //等待阶段
            if (!operation.IsActivationAllowed)
            {
                operation.ChangedState(SceneLoadState.Awaiting);

                while (!operation.IsActivationAllowed) yield return null;
            }

            //激活阶段
            operation.ChangedState(SceneLoadState.Activating);
            yield return asyncOpHandle.Result.ActivateAsync();
            if (asyncOpHandle.Status != AsyncOperationStatus.Succeeded)
            {
                LogSystem.Warning($"场景加载失败：{sceneKey}");
                yield break;
            }

            //缓存场景实例
            SceneHandle sceneHandle = new SceneHandle(IdGenerator.Generate(), sceneKey, SceneProvider.Addressables);
            sceneInstanceDic.Add(sceneHandle, asyncOpHandle.Result);
            operation.Result = sceneHandle;

            //完成阶段
            operation.UpdateProgress(1);
            operation.ChangedState(SceneLoadState.Completed);

            yield break;
        }

        public void UnloadScene(SceneHandle handle)
        {
            if (SceneManager.sceneCount == 1)
            {
                LogSystem.Warning($"无法卸载当前唯一场景：{handle.SceneKey}");
                return;
            }

            if (!sceneInstanceDic.Remove(handle, out SceneInstance scene))
            {
                LogSystem.Warning($"场景不存在：{handle.SceneKey}");
                return;
            }

            Addressables.UnloadSceneAsync(scene);
        }

        private bool CheckSceneIsValid(string sceneKey)
        {
            //检查当前需要加载的场景资源是否存在
            AsyncOperationHandle<IList<IResourceLocation>> asyncOpHandle = Addressables.LoadResourceLocationsAsync(sceneKey);
            try
            {
               asyncOpHandle.WaitForCompletion();

               bool valid = asyncOpHandle.Status == AsyncOperationStatus.Succeeded
               && asyncOpHandle.Result != null
               && asyncOpHandle.Result.Count > 0;

                if (!valid) LogSystem.Warning($"场景无法加载：{sceneKey}");
                return valid;
            }
            finally
            {
                Addressables.Release(asyncOpHandle);
            }
        }
    }
}