using System;
using Sirenix.OdinInspector;

namespace EmptyFrame
{
    public class UIWindowData
    {
        public readonly string ConfigKey;
        public readonly bool IsCache;
        public readonly int Layer;

        [NonSerialized, ShowInInspector] public UIWindowBase Window;
        [NonSerialized, ShowInInspector] public UIWindowConfigBase Config;

        public UIWindowData(string configKey,bool isCache,int layer) 
        {
            ConfigKey = configKey;
            IsCache = isCache;
            Layer = layer;
        }
    }
}
