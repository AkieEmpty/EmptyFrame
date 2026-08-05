namespace EmptyFrame.Core
{
    internal interface ISceneLoader
    {
        /// <summary>
        /// 同步加载场景
        /// </summary>
        SceneHandle LoadSceneSync(LoadSceneOptions options);
        /// <summary>
        /// 异步加载场景
        /// </summary>
        LoadSceneOperation LoadSceneAsync(LoadSceneOptions options);
        /// <summary>
        /// 卸载场景
        /// </summary>
        void UnloadScene(SceneHandle handle);
    }
}
