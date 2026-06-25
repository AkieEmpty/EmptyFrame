namespace EmptyFrame
{
    public enum SceneProvider
    {
        /// <summary>
        /// 使用Unity内置场景管理器加载
        /// </summary>
        SceneManager,
        /// <summary>
        /// 使用Addressables加载
        /// </summary>
        Addressables
    }
}
