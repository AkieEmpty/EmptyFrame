using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EmptyFrame.Core
{
    /// <summary>
    /// AudioClip 资产引用
    /// </summary>
    [Serializable]
    public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
    {
        public AssetReferenceAudioClip(string guid) : base(guid) { }
    }
}