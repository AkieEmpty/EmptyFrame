using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame.Core
{
    [CreateAssetMenu(menuName = "EmptyFrame/Setting/AudioSystemSetting", fileName = "AudioSystemSetting")]
    [HideMonoScript]

    internal class AudioSystemSetting : FrameSettingBase
    {
        public AudioSystemSetting() => Title = "<b>音频</b>";

        [LabelText("全局音量"), PropertySpace(SpaceBefore = 0)]
        [Range(0, 1)]
        public float MasterVolume = 1f;

        [LabelText("背景音量"), PropertySpace()]
        [Range(0, 1)]
        public float BgmVolume = 1f;

        [LabelText("音效音量"), PropertySpace()]
        [Range(0, 1)]
        public float SfxVolume = 1f;

        [LabelText("全局静音"), PropertySpace()]
        public bool IsMute = false;
    }
}
