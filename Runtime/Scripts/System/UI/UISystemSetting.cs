using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace EmptyFrame
{

    public partial class UISystemSetting 
    {
        [LabelText("窗口数据映射")]
        [ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "窗口类名",ValueLabel = "窗口数据")]
        public Dictionary<string, UIWindowData> WindowDataDic = new Dictionary<string, UIWindowData>();

        public bool TryGetWindowData(string windowKey,out UIWindowData windowData)
        {
            if (WindowDataDic.TryGetValue(windowKey,out windowData))
            {
                return true;
            }
            return false; 
        }



    }


#if UNITY_EDITOR
    public partial class UISystemSetting 
    {
        public void Reset()
        {
            InitUIWindowDataDicOnEditor();
        }
        /// <summary>
        /// UI数据初始化
        /// </summary>
        public void InitUIWindowDataDicOnEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            WindowDataDic.Clear();
            //获取所有程序集
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            Type baseType = typeof(UIWindowBase);

            foreach (Assembly assembly in asms)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (baseType.IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        IEnumerable<UIWindowDataAttribute> attributes = type.GetCustomAttributes<UIWindowDataAttribute>();

                        foreach (UIWindowDataAttribute attribute in attributes)
                        {
                           UIWindowData WindowData = new UIWindowData(attribute.ConfigKey,attribute.IsCache,attribute.Layer);


                            WindowDataDic.Add(attribute.WindowKey, WindowData);
                        }
                    }
                }
            }
        }
    }
#endif

}
