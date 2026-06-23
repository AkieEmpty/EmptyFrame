using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 本地化配置
    /// </summary>
    [CreateAssetMenu(menuName = "Unity/Localiation/LocalizationConfig", fileName = "LocalizationConfig")]
    public class LocalizationConfig: SerializedScriptableObject
    {
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public Dictionary<string, Dictionary<LanguageType, string>> LocalizationConfigDic = new Dictionary<string, Dictionary<LanguageType, string>>(); 

       
    }

   
}
