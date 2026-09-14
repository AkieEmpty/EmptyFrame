using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 窗口预制体引用
    /// </summary>
    [Serializable]
    public class AssetReferenceUIWindow : AssetReferenceGameObject
    {
        public AssetReferenceUIWindow(string guid) : base(guid) { }
        
        public override bool ValidateAsset(UnityEngine.Object obj)
        {
            return obj is GameObject go && go.GetComponent<UIWindowBase>() != null;
        }
    }
}