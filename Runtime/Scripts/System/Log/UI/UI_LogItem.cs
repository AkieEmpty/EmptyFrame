using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EmptyFrame
{
    public class UI_LogItem : MonoBehaviour,IPointerClickHandler
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Text logText;
        [SerializeField] private Image bgImage;
        [SerializeField] private GameObject countRoot;
        [SerializeField] private Text countText;

        private readonly static Color oddItemColor = new Color(0.22F, 0.22F, 0.22F, 0.22F);
        private readonly static Color evenItemColor = new Color(0, 0, 0, 0);
        private readonly static Color selectItemColor = new Color(0.28F, 0.55F, 0.86F, 0.71F);

        public LogData LogData { get; private set; }
        private Color bgColor;

        public event Action<UI_LogItem> OnClick;

        public void Init(LogData data,int count,bool isEven,bool showCount)
        {
            bgColor = isEven ? evenItemColor : oddItemColor;
            bgImage.color = bgColor;

            RefreshLog(data);

            countRoot.SetActive(showCount);
            if (showCount) RefreshCount(count);
        }
        public void SetPosition(int dataIndex, float itemHeight)
        {
            rectTransform.anchoredPosition = new Vector2(0, -dataIndex * itemHeight);
        }
        public void SetWidth(float width)
        {
            rectTransform.sizeDelta = new Vector2(width, rectTransform.rect.height);
        }
        public void SetSelect(bool isSelect)
        {
            bgImage.color = isSelect ? selectItemColor : bgColor;
        }
        public void RefreshLog(LogData data)
        {
            LogData = data;

            string time = $"[{data.Time:HH:mm:ss}]";
            string typeStr = $"[{data.Type}]";
            logText.text = $"{time}{typeStr} {data.Message}";
            logText.color = data.Color;
        }
        public void RefreshCount(int count)
        {
            countText.text = count.ToString();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }
    }
}
