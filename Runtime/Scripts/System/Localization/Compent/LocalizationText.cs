using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmptyFrame
{
    /// <summary>
    /// 本地化文本组件
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizationText : LocalizationBehaviourBase<Text>
    {

        protected override void Refresh()
        {
            component.text = LocalizationSystem.GetContent(localizationKey);
        }
    }
}
