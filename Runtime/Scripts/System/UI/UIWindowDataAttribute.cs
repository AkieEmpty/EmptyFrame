using System;

namespace EmptyFrame
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIWindowDataAttribute : Attribute
    {
        public string WindowKey;
        public string ConfigKey;

        public bool IsCache;
        public int Layer;
        public UIWindowDataAttribute(string windowKey, string configKey,bool isCache ,int layer)
        {
            WindowKey = windowKey;
            ConfigKey = configKey;
            IsCache = isCache;
            Layer = layer;
        }
    }
}
