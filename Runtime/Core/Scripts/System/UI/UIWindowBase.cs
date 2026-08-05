using System;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI 窗口基类
    /// </summary>
    public abstract class UIWindowBase : MonoBehaviour
    {
        private UIWindowConfigBase windowConfig;

        protected UIWindowConfigBase WindowConfig => windowConfig;

        #region 内部初始化

        
        internal void Init(UIWindowConfigBase windowConfig)
        {
            this.windowConfig = windowConfig;
        }
        internal void Uninit()
        {
            this.windowConfig = null;
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 窗口创建时调用
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// 窗口进入显示过渡阶段时调用
        /// </summary>
        public virtual void OnShowing(Action onComplete) => onComplete?.Invoke();

        /// <summary>
        /// 窗口显示完成时调用
        /// </summary>
        public virtual void OnShow() { }

        /// <summary>
        /// 窗口隐藏时调用
        /// </summary>
        public virtual void OnHide() { }

        /// <summary>
        /// 窗口进入隐藏过渡阶段时调用
        /// </summary>
        public virtual void OnHiding(Action onComplete) => onComplete?.Invoke();

        /// <summary>
        /// 窗口销毁前调用
        /// </summary>
        public virtual void OnRelease() { }

        #endregion
    }
}
