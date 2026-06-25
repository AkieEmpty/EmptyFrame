using System;

namespace EmptyFrame
{
    public interface ISceneLoader
    { 
        /// <summary>
        /// 同步加载场景
        /// </summary>
        void LoadSceneSync(LoadSceneParms parms);
        /// <summary>
        /// 异步加载场景
        /// </summary>
        SceneLoadHandle LoadSceneAsync(LoadSceneParms parms);
        /// <summary>
        /// 异步卸载场景
        /// </summary>
        void UnloadScene(SceneLoadHandle handle);
    }
}
