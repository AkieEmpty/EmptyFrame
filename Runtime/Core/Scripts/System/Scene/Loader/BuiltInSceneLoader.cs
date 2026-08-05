using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 内置场景加载器
    /// <para>
    /// 基于 UnityEngine.SceneManagement.SceneManager 实现场景加载与卸载
    /// </para>
    /// </summary>
    internal class BuiltInSceneLoader : ISceneLoader
    {
        private readonly Dictionary<SceneHandle, Scene> sceneInstanceDic = new Dictionary<SceneHandle, Scene>();

        public SceneHandle LoadSceneSync(LoadSceneOptions options)
        {
            if(!CheckSceneIsValid(options.SceneKey)) return default;

            SceneManager.LoadScene(options.SceneKey, options.Mode);

            SceneHandle handle = new SceneHandle(IdGenerator.Generate(), options.SceneKey,SceneProvider.BuiltIn);

            sceneInstanceDic.Add(handle, SceneManager.GetSceneAt(SceneManager.sceneCount - 1));

            return handle;
        }

        

        public LoadSceneOperation LoadSceneAsync(LoadSceneOptions options)
        {
            if (!CheckSceneIsValid(options.SceneKey)) return null;

            LoadSceneOperation operation = new LoadSceneOperation(options);

            MonoSystem.StartCoroutine(DoLoadSceneAsync(options.SceneKey,operation, options.Mode));

            return operation;
        }

        private IEnumerator DoLoadSceneAsync(string sceneKey,LoadSceneOperation operation, LoadSceneMode loadModel)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneKey, loadModel);
            asyncOp.allowSceneActivation = false;

            //加载阶段
            operation.UpdateProgress(0);
            operation.ChangedState(SceneLoadState.Loading);
            while (asyncOp.progress < 0.9f)
            {
                operation.UpdateProgress(asyncOp.progress);//更新进度
                yield return null;
            }

            //等待阶段
            if (!operation.IsActivationAllowed)
            {
                operation.ChangedState(SceneLoadState.Awaiting);
                //等待激活
                while (!operation.IsActivationAllowed) yield return null;
            }

            //激活阶段
            operation.ChangedState(SceneLoadState.Activating);
            asyncOp.allowSceneActivation = true;
            yield return asyncOp;

            //缓存场景实例
            SceneHandle sceneHandle = new SceneHandle(IdGenerator.Generate(), sceneKey,SceneProvider.BuiltIn);
            sceneInstanceDic.Add(sceneHandle, SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
            operation.Result = sceneHandle;

            //加载完成
            operation.UpdateProgress(1f);
            operation.ChangedState(SceneLoadState.Completed);

            yield break;
        }

        public void UnloadScene(SceneHandle handle)
        {
            if (SceneManager.sceneCount == 1)
            {
                LogSystem.Warning($"无法卸载当前唯一场景 ：{handle.SceneKey}");
                return;
            }

            if (!sceneInstanceDic.Remove(handle, out Scene scene))
            {
                LogSystem.Warning($"场景不存在：{handle.SceneKey}");
                return;
            }
            SceneManager.UnloadSceneAsync(scene);
        }

        private bool CheckSceneIsValid(string sceneKey)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneKey))
            {
                LogSystem.Warning($"场景无法加载：{sceneKey}");
                return false;
            }
            return true;

        }
    }
}
