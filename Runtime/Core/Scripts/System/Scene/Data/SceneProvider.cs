namespace EmptyFrame.Core
{
    public enum SceneProvider
    {
        None,
        /// <summary>
        /// 使用Unity内置场景管理器加载
        /// </summary>
        BuiltIn,
        /// <summary>
        /// 使用Addressables加载
        /// </summary>
        Addressables
    }
}
