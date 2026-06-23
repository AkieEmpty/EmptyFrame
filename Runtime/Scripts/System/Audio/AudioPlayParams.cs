using System;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 音频播放参数
    /// </summary>
    public struct AudioPlayParams
    {
        public AudioGroup AudioGroup;
        public float VolumeScale;
        public float Pitch;
        public bool Is3D;
        public Vector3 Position;
        public float FadeInDuration;
        public Action OnComplete;

        public static AudioPlayParams Default => new()
        {
            AudioGroup = AudioGroup.Sfx,
            VolumeScale = 1f,
            Pitch = 1f,
            Is3D = false,
            Position = Vector3.zero,
            FadeInDuration = 0f,
            OnComplete = null

        };
    }
}
