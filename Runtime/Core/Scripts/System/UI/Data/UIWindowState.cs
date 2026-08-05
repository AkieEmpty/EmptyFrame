namespace EmptyFrame.Core
{
    /// <summary>
    /// 窗口生命周期状态。
    /// <para>None → Showing → Shown → Hiding → Hidden → (Showing 或 None)</para>
    /// </summary>
    internal enum UIWindowState
    {
        /// <summary>
        /// 窗口未创建
        /// </summary>
        None,
        /// 
        /// <summary>
        /// 正在显示
        /// </summary>
        Showing,
        /// <summary>
        /// 已显示
        /// </summary>
        Shown,
        /// <summary>
        /// 正在隐藏
        /// </summary>
        Hiding,
        /// <summary>
        /// 已隐藏
        /// </summary>
        Hidden,
    }
}
