using TMPro;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 本地化文本组件(TextMeshProUGUI)
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizationUGUIText : LocalizationBehaviourBase<TextMeshProUGUI>
    {
        protected override void Refresh()
        {
            component.text = LocalizationSystem.GetContent(localizationKey);
        }
    }
}