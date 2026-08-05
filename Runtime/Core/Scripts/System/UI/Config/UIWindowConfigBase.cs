using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI 窗口配置
    /// </summary>
    [CreateAssetMenu(menuName = "EmptyFrame/UI/UIWindowConfig", fileName = "UIWindowConfig")]
    public class UIWindowConfigBase : SerializedScriptableObject
    {
        [Title("窗口预制体")]
        public GameObject WindowPrefab;
    }
}
