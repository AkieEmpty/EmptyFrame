using System;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 窗口定义标注：标记在 UIWindowBase 子类上，声明窗口的注册信息。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIWindowDefinitionAttribute : Attribute
    {
        /// <summary>窗口唯一标识（通常与类名一致）</summary>
        public string WindowKey;

        /// <summary>Addressables 资源 Key，用于加载窗口配置</summary>
        public string ConfigKey;

        /// <summary>是否缓存窗口（隐藏后不销毁，复用实例）</summary>
        public bool IsCached;

        /// <summary>所属层级索引</summary>
        public int Layer;

        public UIWindowDefinitionAttribute(string windowKey, string configKey, bool isCached, int layer)
        {
            WindowKey = windowKey;
            ConfigKey = configKey;
            IsCached = isCached;
            Layer = layer;
        }
    }
}
