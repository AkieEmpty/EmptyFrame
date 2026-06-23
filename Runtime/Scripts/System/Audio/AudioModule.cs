using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 音频模块
    /// </summary>
    public partial class AudioModule : MonoBehaviour
    {
        /// <summary>
        /// 音频定位器
        /// </summary>
        private class AudioLocator
        {
            public AudioSource AudioSource;
            public float VolumeScale;
            public AudioGroup Group;

            public float FadeMultiplier = 1f;//过渡进度
            public Coroutine FadeCoroutine;
            public void Init(AudioSource audioSource, float volumeScale, AudioGroup group)
            {
                AudioSource = audioSource;
                VolumeScale = volumeScale;
                Group = group;
                FadeMultiplier = 1f;
                FadeCoroutine = null;
            }
        }

        private Transform audioSourceRoot;
        private GameObjectPoolModule gameObjectPool;
        private ObjectPoolModule objectPool;
        public void Init()
        {
            gameObjectPool = new GameObjectPoolModule(PoolSystem.RootTransform);
            objectPool = new ObjectPoolModule();

            audioSourceRoot = transform;
        }

       
    }
    public partial class AudioModule : MonoBehaviour
    {
        [Title("播放设置", titleAlignment: TitleAlignments.Centered)]
        
        [VerticalGroup("Volumes"), OnValueChanged(nameof(OnMasterVolumeChanged))]
        [SerializeField, Range(0, 1), LabelText("全局音量")] 
        private float masterVolume;

        [VerticalGroup("Volumes") ,OnValueChanged(nameof(OnBgmVolumeChanged))]
        [SerializeField, Range(0, 1), LabelText("背景音量")] 
        private float bgmVolume;

        [VerticalGroup("Volumes"), OnValueChanged(nameof(OnSfxVolumeChanged))]
        [SerializeField, Range(0, 1), LabelText("音效音量")] 
        private float sfxVolume;

        [Space(8), OnValueChanged(nameof(RefreshMuteState))]
        [SerializeField, ToggleLeft, LabelText("全局静音")]
        private bool isMute;

        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(masterVolume, value)) return;
                masterVolume = value;

                RefreshVolumesByGroup(AudioGroup.Bgm);
                RefreshVolumesByGroup(AudioGroup.Sfx);
            }
        }
        public float BgmVolume
        {
            get => bgmVolume;
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(bgmVolume, value)) return;
                bgmVolume = value;
                RefreshVolumesByGroup(AudioGroup.Bgm);
            }
        }
        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(sfxVolume, value)) return;
                sfxVolume = value;
                RefreshVolumesByGroup(AudioGroup.Sfx);
            }
        }
        public bool IsMute
        {
            get=> isMute;
            set
            {
                if (isMute == value) return;
                isMute = value;
                RefreshMuteState();
            }
        }
  

        #region 值更新回调
        private void OnMasterVolumeChanged()
        {
            RefreshVolumesByGroup(AudioGroup.Bgm);
            RefreshVolumesByGroup(AudioGroup.Sfx);
        }
        private void OnBgmVolumeChanged() => RefreshVolumesByGroup(AudioGroup.Bgm);
        private void OnSfxVolumeChanged() => RefreshVolumesByGroup(AudioGroup.Sfx);
        private void RefreshVolumesByGroup(AudioGroup group)
        {
            foreach (var locator in loopAudioSourceDic.Values)
            {
                if (locator.Group == group)
                {
                    locator.AudioSource.volume = CalculateVolume(locator.VolumeScale, locator.Group, locator.FadeMultiplier);
                }
            }

            foreach (var locator in oneShotAudioSourceList)
            {
                if (locator.Group == group)
                {
                    locator.AudioSource.volume = CalculateVolume(locator.VolumeScale, locator.Group, locator.FadeMultiplier);
                }
            }
        }
        private void RefreshMuteState()
        {
            foreach (var locator in loopAudioSourceDic.Values)
            {
                locator.AudioSource.mute = isMute;
            }

            foreach (var locator in oneShotAudioSourceList)
            {
                locator.AudioSource.mute = isMute;
            }
        }
        #endregion

        #region 音量过渡
        /// <summary>
        /// 音量渐入
        /// </summary>
        private IEnumerator DoVolumeFadeIn(AudioLocator locator, float duration)
        {
            float timer = 0F;

            while (timer<duration)
            {
                timer += Time.deltaTime;
                //计算进度
                locator.FadeMultiplier = Mathf.Clamp01(timer/duration);

                if (locator.AudioSource != null)
                {
                    locator.AudioSource.volume = CalculateVolume(locator.VolumeScale, locator.Group, locator.FadeMultiplier);
                }
                yield return null;
            }

            locator.FadeMultiplier = 1f;
            if (locator.AudioSource != null)
            {
                locator.AudioSource.volume = CalculateVolume(locator.VolumeScale, locator.Group, locator.FadeMultiplier);
            }

            locator.FadeCoroutine = null;
        }
        /// <summary>
        /// 音量渐出
        /// </summary>
        private IEnumerator DoVolumeFadeOut(AudioLocator locator, float duration)
        {
            //获取当前起始进度
            float startMultiplier = locator.FadeMultiplier;
            float timer = 0;

            while (timer<duration)
            {
                timer += Time.deltaTime;
                //计算进度
                locator.FadeMultiplier = Mathf.Lerp(startMultiplier,0F, Mathf.Clamp01(timer / duration));

                if (locator.AudioSource != null)
                {
                    locator.AudioSource.volume = CalculateVolume(locator.VolumeScale, locator.Group, locator.FadeMultiplier);
                }
                yield return null;
            }

            locator.FadeMultiplier = 0F;
            if (locator.AudioSource != null)
            {
                locator.AudioSource.volume = CalculateVolume(locator.VolumeScale, locator.Group, locator.FadeMultiplier);
            }

            locator.FadeCoroutine = null;
            RecycleAudioSource(locator);
            
        }
        /// <summary>
        /// 关闭音量过渡
        /// </summary>
        private void CancelVolumeFade(AudioLocator locator)
        {
            if (locator.FadeCoroutine == null) return;
            StopCoroutine(locator.FadeCoroutine);
            locator.FadeCoroutine = null;
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 计算音量
        /// </summary>
        private float CalculateVolume(float volumeScale, AudioGroup group, float fadeMultiplier = 1f)
        {
            //实际音量 = 主音量 * 分组音量 * 音量缩放 * 音量过渡进度 
            return masterVolume * GetGroupVolume(group) * volumeScale * fadeMultiplier;
        }
        /// <summary>
        /// 获取分组音量
        /// </summary>
        private float GetGroupVolume(AudioGroup group)
        {
            return group == AudioGroup.Sfx ? sfxVolume : bgmVolume;
        }
        #endregion
    }
    public partial class AudioModule : MonoBehaviour
    {
        private int handleId = 1;

        private Dictionary<int, AudioLocator> loopAudioSourceDic = new Dictionary<int, AudioLocator>();
        private List<AudioLocator> oneShotAudioSourceList = new List<AudioLocator>();

        #region OneShot
        /// <summary>
        /// 播放一次性音频
        /// </summary>
        public void PlayOneShot(AudioClip audioClip, AudioPlayParams param)
        {
            if (audioClip == null) return;

            AudioSource audioSource = GetAudioSource();
            audioSource.loop = false;
            ApplyParamsToSource(audioSource,audioClip, param);

            AudioLocator locator = GetAudioLocator();
            locator.Init(audioSource, param.VolumeScale, param.AudioGroup);
            oneShotAudioSourceList.Add(locator);

            audioSource.Play();
            
            StartCoroutine(RecycleOneShot(locator, param.OnComplete));
        }
      
        #endregion

        #region Loop
        /// <summary>
        /// 播放循环
        /// </summary>
        public AudioPlayHandle PlayLoop(AudioClip audioClip, AudioPlayParams param)
        {
            if (audioClip == null) return default;

            AudioSource audioSource = GetAudioSource();

            audioSource.loop = true;

            ApplyParamsToSource(audioSource, audioClip,param);

            int id = handleId++;
            AudioLocator locator = GetAudioLocator();
            locator.Init(audioSource, param.VolumeScale, param.AudioGroup);
            loopAudioSourceDic.Add(id,locator);

            //需要渐入
            if (param.FadeInDuration > 0f)
            {
                locator.FadeMultiplier = 0f;
                audioSource.volume = 0f;
                audioSource.Play();
                locator.FadeCoroutine = StartCoroutine(DoVolumeFadeIn(locator, param.FadeInDuration));
            }
            else
            {
                locator.FadeMultiplier = 1f;
                audioSource.Play();
            }

            return new AudioPlayHandle(id, param.AudioGroup);
        }
        /// <summary>
        /// 暂停指定循环音频
        /// </summary>
        public void PauseLoop(AudioPlayHandle handle)
        {
            if (loopAudioSourceDic.TryGetValue(handle.ID, out AudioLocator locator))
            {
                locator.AudioSource.Pause();
            }
        }
        /// <summary>
        /// 取消暂停指定循环音频
        /// </summary>
        public void UnPauseLoop(AudioPlayHandle handle)
        {
            if (loopAudioSourceDic.TryGetValue(handle.ID, out AudioLocator locator))
            {
                locator.AudioSource.UnPause();
            }
        }
        /// <summary>
        /// 停止指定循环音频
        /// </summary>
        public void StopLoop(AudioPlayHandle handle, float fadeDuration = 0f)
        {
            if (!loopAudioSourceDic.Remove(handle.ID, out var locator)) return;

            //关闭协程
            CancelVolumeFade(locator);

            //需要渐出
            if (fadeDuration > 0f)
            {
                locator.FadeCoroutine = StartCoroutine(DoVolumeFadeOut(locator, fadeDuration));
            }
            //常规回收
            else RecycleAudioSource(locator);
        }
        /// <summary>
        /// 停止所有循环音频
        /// </summary>
        public void StopAllLoop(float fadeDuration = 0f)
        {
            foreach (var locator in loopAudioSourceDic.Values)
            {
                //关闭协程
                CancelVolumeFade(locator);
                //需要渐出
                if (fadeDuration > 0f)
                {
                    locator.FadeCoroutine = StartCoroutine(DoVolumeFadeOut(locator, fadeDuration));
                }
                //常规回收
                else RecycleAudioSource(locator);
            }
            loopAudioSourceDic.Clear();
        }
        #endregion

        #region 回收
        /// <summary>
        /// 回收一次性音频
        /// </summary>
        private IEnumerator RecycleOneShot(AudioLocator locator, Action OnComplete)
        {
            yield return null;

            while (locator.AudioSource != null && locator.AudioSource.isPlaying)
            {
                yield return null;
            }

            OnComplete?.Invoke();

            oneShotAudioSourceList.Remove(locator);

            RecycleAudioSource(locator);
        }
        /// <summary>
        /// 回收音频播放器
        /// </summary>
        private void RecycleAudioSource(AudioLocator locator)
        {
            if (locator == null|| locator.AudioSource == null) return;

            locator.AudioSource.Stop();
            locator.AudioSource.clip = null;
            gameObjectPool.PushGameObject(locator.AudioSource.gameObject);
            locator.AudioSource = null;

            objectPool.PushObject(locator);
        }

        #endregion

        #region 辅助方法
        /// <summary>
        /// 将参数应用到AudioSource中
        /// </summary>
        private void ApplyParamsToSource(AudioSource audioSource, AudioClip clip, AudioPlayParams param)
        {
            audioSource.clip = clip;
            audioSource.volume = CalculateVolume(param.VolumeScale, param.AudioGroup);
            audioSource.pitch = param.Pitch;
            audioSource.spatialBlend = param.Is3D ? 1f : 0f;
            audioSource.mute = isMute;

            if (param.Is3D) audioSource.transform.position = param.Position;
        }
        /// <summary>
        /// 获取音频定位器
        /// </summary>
        private AudioLocator GetAudioLocator()
        {
            var locator = objectPool.GetObject<AudioLocator>();
            if (locator == null) locator = new AudioLocator();
            return locator;
        }
        /// <summary>
        /// 获取音频播放器
        /// </summary>
        private AudioSource GetAudioSource()
        {
            AudioSource audioSource = gameObjectPool.GetGameObject<AudioSource>("AudioPlayer", audioSourceRoot);

            if (audioSource == null)
            {
                GameObject go = new GameObject("AudioPlayer");
                go.transform.SetParent(audioSourceRoot);
                audioSource = go.AddComponent<AudioSource>();
            }

            return audioSource;
        }
        #endregion
    }

}
