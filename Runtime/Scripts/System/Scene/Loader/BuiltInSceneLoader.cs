using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace EmptyFrame
{
    /// <summary>
    /// 内置场景加载器
    /// <para>
    /// 基于 UnityEngine.SceneManagement.SceneManager 实现场景加载与卸载
    /// </para>
    /// </summary>
    public class BuiltInSceneLoader : ISceneLoader
    {
        private readonly Dictionary<SceneLoadHandle, Scene> sceneInstanceDic = new Dictionary<SceneLoadHandle, Scene>();

        public void LoadSceneSync(LoadSceneParms parms)
        {
            SceneManager.LoadScene(parms.SceneKey, parms.LoadMode);
        }

        public SceneLoadHandle LoadSceneAsync(LoadSceneParms parms)
        {
            SceneLoadHandle handle = new SceneLoadHandle(parms);

            MonoSystem.StartRoutine(DoLoadSceneAsync(handle,parms.LoadMode));

            return handle;
        }

        private IEnumerator DoLoadSceneAsync(SceneLoadHandle handle, LoadSceneMode loadModel)
        {
            var asyncOp = SceneManager.LoadSceneAsync(handle.SceneKey, loadModel);
            asyncOp.allowSceneActivation = false;

            //加载阶段
            handle.UpdateProgress(0);
            handle.ChangedState(SceneLoadState.Loading);
            while (asyncOp.progress < 0.9f)
            {
                handle.UpdateProgress(asyncOp.progress);//更新进度
                yield return null;
            }

            //等待阶段
            if (!handle.IsActivationAllowed)
            {
                handle.ChangedState(SceneLoadState.Awaiting);
                //等待激活
                while (!handle.IsActivationAllowed) yield return null;
            }

            //激活阶段
            handle.ChangedState(SceneLoadState.Activating);
            asyncOp.allowSceneActivation = true;
            yield return asyncOp;

            //缓存
            int sceneIndex = SceneManager.sceneCount - 1; //如果是Additive模式, 新加入的场景排在最后面
            Scene loadedScene = SceneManager.GetSceneAt(sceneIndex);
            sceneInstanceDic.Add(handle, loadedScene);

            //加载完成
            handle.UpdateProgress(1f);
            handle.ChangedState(SceneLoadState.Completed);

            yield break;
        }

        public void UnloadScene(SceneLoadHandle handle)
        {
            if (handle == null) return;

            if (sceneInstanceDic.Remove(handle, out Scene scene))
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }


    }
}
