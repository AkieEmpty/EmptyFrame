namespace EmptyFrame.Core
{
    /// <summary>
    /// UI窗口运行实例
    /// </summary>
    internal class UIWindowInstance
    {
        public UIWindowBase Window;
        public readonly UIWindowStateMachine StateMachine = new UIWindowStateMachine();

        /// <summary>
        /// 当前窗口状态
        /// </summary>
        public UIWindowState State => StateMachine.Current;
    }
}
