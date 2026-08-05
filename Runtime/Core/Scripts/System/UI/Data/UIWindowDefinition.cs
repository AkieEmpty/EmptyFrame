using System;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI窗口定义：从 UIWindowDefinitionAttribute 提取的静态配置信息，运行期不可变。
    /// 序列化在 UISystemSetting 上，编辑器期由反射自动生成。
    /// </summary>
    [Serializable]
    internal class UIWindowDefinition
    {
        /// <summary>窗口唯一标识</summary>
        public string WindowKey;

        /// <summary>资源 Key，用于加载窗口配置</summary>
        public string ConfigKey;

        /// <summary>是否缓存窗口</summary>
        public bool IsCached;

        /// <summary>所属层级索引</summary>
        public int Layer;
    }
}
