using System.Collections.Generic;
using EmptyFrame.Core;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace EmptyFrame.Network
{
    [CreateAssetMenu(menuName = "EmptyFrame/Setting/NetworkSystemSetting", fileName = "NetworkSystemSetting")]
    [HideMonoScript]
    internal class NetworkSystemSetting : FrameSettingBase
    {
        public NetworkSystemSetting() => Title = "<b>网络</b>";

        [LabelText("网络管理器"), PropertySpace(SpaceBefore = 0)]
        public GameObject NetworkManager;

        [LabelText("网络对象预制体列表"), PropertySpace()]
        public List<NetworkPrefabsList> NetworkPrefabsList = new List<NetworkPrefabsList>();
    }
}