using System;
using UnityEngine.AddressableAssets;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 窗口定义标注：标记在 UIWindowBase 子类上，声明窗口的注册信息。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIWindowDefinitionAttribute : Attribute
    {
        /// <summary>
        /// 窗口唯一标识
        /// </summary>
        public string WindowKey;

        /// <summary>
        /// 是否缓存窗口
        /// </summary>
        public bool IsCached;

        /// <summary>
        /// 所属层级索引
        /// </summary>
        public int Layer;

        public UIWindowDefinitionAttribute(string windowKey, bool isCached, int layer)
        {
            WindowKey = windowKey;
            IsCached = isCached;
            Layer = layer;
        }
    }
}
