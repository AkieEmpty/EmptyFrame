using System;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 音频系统
    /// </summary>
    public static class AudioSystem 
    {
        private static AudioModule audioModule;

        /// <summary>
        /// 主音量
        /// </summary>
        public static float MasterVolume { get => audioModule.MasterVolume; set => audioModule.MasterVolume = value; }
        /// <summary>
        /// 背景音量
        /// </summary>
        public static float BgmVolume { get => audioModule.BgmVolume; set => audioModule.BgmVolume = value; }
        /// <summary>
        /// 音效音量 
        /// </summary>
        public static float SfxVolume { get => audioModule.SfxVolume; set => audioModule.SfxVolume = value; }
        /// <summary>
        /// 静音
        /// </summary>
        public static bool IsMute { get => audioModule.IsMute; set => audioModule.IsMute = value; }

        public static void Init()
        {
            audioModule = EmptyFrameRoot.RootTransform.GetComponentInChildren<AudioModule>();
            if (audioModule == null) Debug.LogError($"[AudioSystem] ，未找到组件: {nameof(AudioModule)} 。");

            audioModule.Init();
        }


        #region OneShot
        /// <summary>
        /// 播放一次性2D音频
        /// </summary>
        public static void PlayOneShot2D(AudioClip clip, float volumeScale = 1, AudioGroup audioGroup = AudioGroup.Sfx, Action onComplete = null)
        {
            var param = AudioPlayParams.Default;
            param.VolumeScale = volumeScale;
            param.AudioGroup = audioGroup;
            param.OnComplete = onComplete;

            audioModule.PlayOneShot(clip, param);
        }
        /// <summary>
        /// 播放一次性3D音频
        /// </summary>
        public static void PlayOneShot3D(AudioClip clip, Vector3 position, float volumeScale = 1, AudioGroup audioGroup = AudioGroup.Sfx, Action onComplete = null)
        {
            var param = AudioPlayParams.Default;
            param.Is3D = true;
            param.Position = position;
            param.VolumeScale = volumeScale;
            param.AudioGroup = audioGroup;
            param.OnComplete = onComplete;
            audioModule.PlayOneShot(clip, param);
        }
        /// <summary>
        /// 播放一次性音频
        /// </summary>
        public static void PlayOneShot(AudioClip clip, AudioPlayParams param)
        {
            audioModule.PlayOneShot(clip, param);
        }
        
        #endregion

        #region Loop
        /// <summary>
        /// 播放循环音频,可以同时播放多个
        /// </summary>
        public static AudioPlayHandle PlayLoop(AudioClip clip, AudioPlayParams param)
        {
            return audioModule.PlayLoop(clip, param);
        }
        /// <summary>
        /// 播放2D循环音频
        /// </summary>
        public static AudioPlayHandle PlayLoop2D(AudioClip clip, float volumeScale = 1, AudioGroup audioGroup = AudioGroup.Bgm, float fadeInDuration = 0f)
        {
            var param = AudioPlayParams.Default;
            param.VolumeScale = volumeScale;
            param.AudioGroup = audioGroup;
            param.FadeInDuration = fadeInDuration;
            return audioModule.PlayLoop(clip, param);
        }
        /// <summary>
        /// 播放3D循环音频
        /// </summary>
        public static AudioPlayHandle PlayLoop3D(AudioClip clip, Vector3 position, float volumeScale = 1, AudioGroup audioGroup = AudioGroup.Bgm, float fadeInDuration = 0f)
        {
            var param = AudioPlayParams.Default;
            param.Is3D = true;
            param.Position = position;
            param.VolumeScale = volumeScale;
            param.AudioGroup = audioGroup;
            param.FadeInDuration = fadeInDuration;
            return audioModule.PlayLoop(clip, param);
        }
        /// <summary>
        /// 暂停指定循环音频
        /// </summary>
        public static void PauseLoop(AudioPlayHandle handle)
        {
            audioModule.PauseLoop(handle);
        }
        /// <summary>
        /// 恢复指定循环音频
        /// </summary>
        public static void UnPauseLoop(AudioPlayHandle handle)
        {
            audioModule.UnPauseLoop(handle);
        }
        /// <summary>
        /// 停止指定循环音频
        /// </summary>
        public static void StopLoop(AudioPlayHandle handle)
        {
            audioModule.StopLoop(handle);
        }
        /// <summary>
        /// 渐出后停止指定循环音频
        /// </summary>
        public static void StopLoopWithFade(AudioPlayHandle handle, float fadeDuration)
        {
            audioModule.StopLoop(handle, fadeDuration);
        }
          
        /// <summary>
        /// 停止所有循环音频
        /// </summary>
        public static void StopAllLoop()
        {
            audioModule.StopAllLoop();
        }
        // <summary>
        /// 渐出后停止所有循环音频
        /// </summary>
        public static void StopAllLoopWithFade(float fadeDuration)
        {
            audioModule.StopAllLoop(fadeDuration);
        }
       
       
        #endregion

       

    }
}
