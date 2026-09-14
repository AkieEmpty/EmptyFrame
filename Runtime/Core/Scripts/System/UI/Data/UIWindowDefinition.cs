using System;
using Sirenix.OdinInspector;
using UnityEngine.AddressableAssets;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI窗口定义
    /// </summary>
    [Serializable]
    internal class UIWindowDefinition
    {
        /// <summary>
        /// 窗口唯一标识
        /// </summary>
        [LabelText("窗口标识")]public string WindowKey;

        /// <summary>
        /// 窗口预制体引用
        /// </summary>
        [LabelText("预制体引用")]public AssetReferenceUIWindow PrefabRef;

        /// <summary>
        /// 是否缓存窗口
        /// </summary>
        [LabelText("是否缓存窗口")]public bool IsCached;

        /// <summary>
        /// 窗口层级
        /// </summary>
        [LabelText("窗口层级")]public int Layer;
        
    }
}
