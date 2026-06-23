using System.Collections.Generic;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// UI主窗口基类
    /// </summary>
    public abstract class UIWindowBase : MonoBehaviour
    {
        private UIWindowConfigBase windowConfig;
        protected UIWindowConfigBase WindowConfig { get=> windowConfig;}

        public virtual void Init(UIWindowConfigBase windowConfig)
        {
            this.windowConfig = windowConfig;
        }
        public virtual void Uninit()
        {
            this.windowConfig = null;
        }
        public virtual void OnShow() { }
        public virtual void OnClose() { }

   
    }
}
