using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// UI窗口配置基类
    /// </summary>
    public abstract class UIWindowConfigBase : SerializedScriptableObject
    {
        [Title("窗口预制体")]
        public GameObject WindowPrefab;
    }
   
}
