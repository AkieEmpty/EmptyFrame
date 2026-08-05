using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame.Core
{
    internal abstract class FrameSettingBase : SerializedScriptableObject
    {
        [HideLabel, DisplayAsString(20, TextAlignment.Left, true), PropertySpace(SpaceBefore = 0,SpaceAfter =5)]
        public string Title;
    }
}
