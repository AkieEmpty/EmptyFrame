using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace EmptyFrame
{
    /// <summary>
    /// Addressables 场景加载器
    /// <para>
    /// 基于 Addressables 实现场景加载与卸载
    /// </para>
    /// </summary>
    public class AddressablesSceneLoader : ISceneLoader
    {
        private readonly Dictionary<SceneLoadHandle, AsyncOperationHandle<SceneInstance>> asyncHandleDic = new Dictionary<SceneLoadHandle, AsyncOperationHandle<SceneInstance>>();

        public void LoadSceneSync(LoadSceneParms parms)
        {
            Addressables.LoadSceneAsync(parms.SceneKey, parms.LoadMode, true).WaitForCompletion();
        }
        public SceneLoadHandle LoadSceneAsync(LoadSceneParms parms)
        {
            SceneLoadHandle handle = new SceneLoadHandle(parms);

            MonoSystem.StartRoutine(DoLoadSceneAsync(handle,parms.LoadMode));

            return handle;
        }
        private IEnumerator DoLoadSceneAsync(SceneLoadHandle handle,LoadSceneMode loadModel)
        {
            AsyncOperationHandle<SceneInstance> asyncOpHandle = Addressables.LoadSceneAsync(handle.SceneKey, loadModel, false);
            asyncHandleDic.Add(handle, asyncOpHandle);

            //加载阶段
            handle.UpdateProgress(0);
            handle.ChangedState(SceneLoadState.Loading);
            while (!asyncOpHandle.IsDone)
            {
                handle.UpdateProgress(asyncOpHandle.PercentComplete); //更新进度
                yield return null;
            }

            //等待阶段
            if (!handle.IsActivationAllowed)
            {
                handle.ChangedState(SceneLoadState.Awaiting);

                while (!handle.IsActivationAllowed) yield return null;
            }

            //激活阶段
            handle.ChangedState(SceneLoadState.Activating);
            yield return asyncOpHandle.Result.ActivateAsync();


            //完成阶段
            handle.UpdateProgress(1);
            handle.ChangedState(SceneLoadState.Completed);

            yield break;
        }

        public void UnloadScene(SceneLoadHandle handle)
        {
            if(asyncHandleDic.Remove(handle,out AsyncOperationHandle<SceneInstance> asyncOpHandle))
            {
                Addressables.UnloadSceneAsync(asyncOpHandle);
            }
        }

       
    }
}