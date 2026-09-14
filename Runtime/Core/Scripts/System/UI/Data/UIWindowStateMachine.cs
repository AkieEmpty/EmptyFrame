using System.Collections.Generic;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI窗口状态机
    /// </summary>
    internal class UIWindowStateMachine
    {
        public UIWindowState Current { get; private set; } = UIWindowState.None;

        // 合法转换路径表。Key=当前状态，Value=允许转换的目标状态集合。
        private static readonly Dictionary<UIWindowState, HashSet<UIWindowState>> transitionMap;

        static UIWindowStateMachine()
        {
            transitionMap = new Dictionary<UIWindowState, HashSet<UIWindowState>>
            {
                { UIWindowState.None,    new HashSet<UIWindowState> { UIWindowState.Showing } },
                // Showing→None: 异步加载失败时回滚
                { UIWindowState.Showing, new HashSet<UIWindowState> { UIWindowState.Shown, UIWindowState.None } },
                { UIWindowState.Shown,   new HashSet<UIWindowState> { UIWindowState.Hiding } },
                { UIWindowState.Hiding,  new HashSet<UIWindowState> { UIWindowState.Hidden } },
                // Hidden 可复用（回到 Showing）或销毁（回到 None）
                { UIWindowState.Hidden,  new HashSet<UIWindowState> { UIWindowState.Showing, UIWindowState.None } },
            };
        }

        /// <summary>
        /// 检查从当前状态能否转换到目标状态
        /// </summary>
        public bool CanTransition(UIWindowState target)
        {
            return transitionMap.TryGetValue(Current, out HashSet<UIWindowState> targets)
                   && targets.Contains(target);
        }

        /// <summary>
        /// 尝试转换到目标状态
        /// </summary>
        public bool ChangedState(UIWindowState target)
        {
            if (!CanTransition(target))
            {
                LogSystem.Warning($"无效转换: {Current} -> {target}");
                return false;
            }

            Current = target;
            return true;
        }
        
        /// <summary>
        /// 强制重置状态
        /// </summary>
        public void Reset()
        {
            Current = UIWindowState.None;
        }
    }
}
