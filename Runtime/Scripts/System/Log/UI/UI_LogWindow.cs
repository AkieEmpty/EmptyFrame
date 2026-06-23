using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;



namespace EmptyFrame
{  
    /// <summary>
    /// 日志系统窗口
    /// </summary>
    public partial class UI_LogWindow : MonoBehaviour
    {
        private const int VisibleCount = 15;
        private const float ItemHeight = 35f;
        private readonly static Color EnableButtonColor = new Color(0.74F, 0.74F, 0.74F, 0.74F);
        private readonly static Color DisableButtonColor = new Color(0.55F, 0.55F, 0.55F, 0.55F);

        [FoldoutGroup("按钮"),SerializeField] private Button hideButton;
        [FoldoutGroup("按钮"),SerializeField] private Button showButton;
        [FoldoutGroup("按钮"),SerializeField] private Button clearButton;
        [FoldoutGroup("按钮"),SerializeField] private Button collapseButton;
        [FoldoutGroup("按钮"),SerializeField] private Button debugButton;
        [FoldoutGroup("按钮"),SerializeField] private Button logButton;
        [FoldoutGroup("按钮"),SerializeField] private Button warningButton;
        [FoldoutGroup("按钮"),SerializeField] private Button errorButton;

        [FoldoutGroup("视图"),SerializeField] private GameObject mainPanel;
        [FoldoutGroup("视图"),SerializeField] private GameObject itemPrefab;
        [FoldoutGroup("视图"),SerializeField] private ScrollRect scrollRect;
        [FoldoutGroup("视图"),SerializeField] private RectTransform content;
        [FoldoutGroup("视图"),SerializeField] private Text detailText;

        private List<LogData> logDataList;                              //日志数据列表
        private List<LogViewData> visibleLogList;                       //可见日志数据列表
        private List<UI_LogItem> logItemList;                           
        private Dictionary<CollapseKey, LogViewData> collapseViewDic;   //折叠日志数据列表

        private LogOption filterOptions = LogOption.Collapse;           //当前按钮选项
        private LogData currentSelectLogData;
        private int startIndex;


        public void Init(List<LogData> logDataList)
        {
            this.logDataList = logDataList;
            visibleLogList = new List<LogViewData>();
            logItemList = new List<UI_LogItem>(VisibleCount);
            collapseViewDic = new Dictionary<CollapseKey, LogViewData>();

            InitMenu();
            InitVirtualList();
            RebuildLogList();
        }
    }
    public partial class UI_LogWindow
    {
        #region 菜单按钮
        private void InitMenu()
        {
            hideButton.onClick.AddListener(HideButtonOnClick);
            showButton.onClick.AddListener(ShowButtonOnClick);

            clearButton.onClick.AddListener(ClaerButtonOnClick);
            collapseButton.onClick.AddListener(CollapseButtonOnClick);

            debugButton.onClick.AddListener(DebugButtonOnClick);
            logButton.onClick.AddListener(LogButtonOnClick);
            warningButton.onClick.AddListener(WarningButtonOnClick);
            errorButton.onClick.AddListener(ErrorButtonOnClick);

            RefreshMenuButton();
        }
      
        private void RefreshMenuButton()
        {
            collapseButton.image.color = IsLogVisible(LogOption.Collapse) ? EnableButtonColor : DisableButtonColor;
            debugButton.image.color = IsLogVisible(LogOption.Debug) ? EnableButtonColor : DisableButtonColor;
            logButton.image.color = IsLogVisible(LogOption.Log) ? EnableButtonColor : DisableButtonColor;
            warningButton.image.color = IsLogVisible(LogOption.Warning) ? EnableButtonColor : DisableButtonColor;
            errorButton.image.color = IsLogVisible(LogOption.Error) ? EnableButtonColor : DisableButtonColor;
        }

        private void HideButtonOnClick()
        {
            mainPanel.SetActive(false);
            showButton.gameObject.SetActive(true);
        }
        private void ShowButtonOnClick()
        {
            mainPanel.SetActive(true);
            showButton.gameObject.SetActive(false);
        }
        private void ClaerButtonOnClick()=> ResetView();
        private void CollapseButtonOnClick() => ToggleOption(LogOption.Collapse);
        private void DebugButtonOnClick() => ToggleOption(LogOption.Debug);
        private void LogButtonOnClick() => ToggleOption(LogOption.Log);
        private void WarningButtonOnClick() => ToggleOption(LogOption.Warning);
        private void ErrorButtonOnClick() => ToggleOption(LogOption.Error);

        #endregion

        #region 日志选项
        /// <summary>
        /// 切换指定选项状态
        /// </summary>
        private void ToggleOption(LogOption option)
        {
            filterOptions ^= option;

            RefreshMenuButton();

            RebuildLogList();
        }
        #endregion

        #region 日志列表
        private void InitVirtualList()
        {
            //默认原生尺寸的两倍
            float width = scrollRect.viewport.rect.width * 2f;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);

            for (int i = 0; i < VisibleCount; i++)
            {
                UI_LogItem item = Instantiate(itemPrefab, content).GetComponent<UI_LogItem>();
                //item.SetPosition(i,ItemHeight);
                item.SetWidth(width);
                item.name = itemPrefab.name;
                item.OnClick += OnItemClick;
                logItemList.Add(item);
            }

            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
        private void RefreshView()
        {
            for (int i = 0; i < logItemList.Count; i++)
            {
                int dataIndex = startIndex + i;

                UI_LogItem item = logItemList[i];

                if (dataIndex >= visibleLogList.Count)
                {
                    item.gameObject.SetActive(false);
                    continue;
                }

                item.gameObject.SetActive(true);

                LogViewData data = visibleLogList[dataIndex];

                item.Init( data.LogData, data.RepeatCount,dataIndex % 2 == 0,data.ShowCount);

                //检查item是否为当前选中的item
                bool isSelected = CheckIsSelected(data.LogData);
                item.SetSelect(isSelected);
                item.SetPosition(dataIndex, ItemHeight);
            }
        }
        public void ResetView()
        {
            logDataList.Clear();
            visibleLogList.Clear();
            collapseViewDic.Clear();

            currentSelectLogData = null;
            startIndex = 0;

            content.anchoredPosition = Vector2.zero;
            RefreshContentHeight();

            foreach (var item in logItemList)
            {
                item.gameObject.SetActive(false);
            }
            ClearLogDetail();
        }
        private void RefreshContentHeight()
        {
            float height = visibleLogList.Count * ItemHeight; content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        private void RebuildLogList()
        {
            startIndex = 0;
            content.anchoredPosition = Vector2.zero;

            //如果折叠模式已激活
            if (IsLogVisible(LogOption.Collapse))
            {
                BuildCollapseVisibleList();
            }
            else
            {
                BuildDefaultVisibleList();
            }

            RefreshContentHeight();
            RefreshView();
             
        }
        private void BuildDefaultVisibleList()
        {
            visibleLogList.Clear();

            for (int i = 0;i < logDataList.Count;i++)
            {
                LogData data = logDataList[i];

                if (!CanDisplayLog(data)) continue;
                //将日志视图数据添加到虚拟列表中
                visibleLogList.Add(new LogViewData()
                {
                    LogData = data,
                    RepeatCount = 1,
                    ShowCount = false
                });
            }

           
        }
        private void BuildCollapseVisibleList()
        {
            collapseViewDic.Clear();
            visibleLogList.Clear();

            //统计折叠信息
            for (int i = 0; i < logDataList.Count; i++)
            {
                LogData data = logDataList[i];

                if (!CanDisplayLog(data)) continue;

                CollapseKey key = new CollapseKey(data);

                if (collapseViewDic.TryGetValue(key, out var viewData))
                {
                    viewData.RepeatCount++;
                    viewData.LogData = data;
                }
                else
                {
                    viewData = new LogViewData()
                    {
                        LogData = data,
                        RepeatCount = 1,
                        ShowCount = true
                    };

                    collapseViewDic.Add(key, viewData);
                    visibleLogList.Add(viewData);
                }
            }

        }
        public void AddLog(LogData data)
        {
            if (!CanDisplayLog(data)) return;

            if (IsLogVisible(LogOption.Collapse))
            {
                AddCollapseLog(data);
            }
            else
            {
                AddDefaultLog(data);
            }
        }
        private void AddDefaultLog(LogData data)
        {
            visibleLogList.Add(new LogViewData()
            {
                LogData = data,
                RepeatCount = 1,
                ShowCount = false
            });

            RefreshContentHeight();

            RefreshView();
        }
        private void AddCollapseLog(LogData data)
        {
            CollapseKey key = new CollapseKey(data);

            //已有相同日志,直接修改数据
            if (collapseViewDic.TryGetValue(key, out var viewData))
            {
                viewData.LogData = data;
                viewData.RepeatCount++;

                RefreshView();
                return;
            }
            //否则，创建新数据
            LogViewData newViewData = new LogViewData()
            {
                LogData = data,
                RepeatCount = 1,
                ShowCount = true
            };

            visibleLogList.Add(newViewData);

            collapseViewDic.Add(key, newViewData);

            RefreshContentHeight();

            RefreshView();
        }
        private void OnItemClick(UI_LogItem item)
        {
            currentSelectLogData = item.LogData;

            RefreshView();

            ShowLogDetail(item.LogData);
        }
        private void OnScrollValueChanged(Vector2 arg0)
        {
            float offsetY = content.anchoredPosition.y;

            int newStartIndex = Mathf.FloorToInt(offsetY / ItemHeight);

            if (newStartIndex < 0) newStartIndex = 0;
            if (newStartIndex == startIndex) return;

            startIndex = newStartIndex;

            RefreshView();
        }
        #endregion

        #region 日志详情
        private void ShowLogDetail(LogData data)
        {
            string content = $"[{data.Type}] {data.Message}";

            if (!string.IsNullOrEmpty(data.StackTrace))
            {
                content += "\n" + data.StackTrace;
            }

            detailText.text = content;
        }

        private void ClearLogDetail()
        {
            detailText.text = string.Empty;
        }
        #endregion

        #region 辅助功能
        /// <summary>
        /// 日志是否允许显示
        /// </summary>
        private bool IsLogVisible(LogOption option)
        {
            return (filterOptions & option) == 0;
        }
        /// <summary>
        /// 当前日志是否可以显示
        /// </summary>
        private bool CanDisplayLog(LogData data)
        {
            switch (data.Type)
            {
                case LogType.Debug:
                    return IsLogVisible(LogOption.Debug);
                case LogType.Log:
                    return IsLogVisible(LogOption.Log);
                case LogType.Warning:
                    return IsLogVisible(LogOption.Warning);
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return IsLogVisible(LogOption.Error);
                default:
                    return true;
            }
        }
        /// <summary>
        /// 检查当前日志数据是否属于被选中项
        /// </summary>
        private bool CheckIsSelected(LogData data)
        {
            if (currentSelectLogData == null) return false;

            if (IsLogVisible(LogOption.Collapse))
            {
                return new CollapseKey(data).Equals(new CollapseKey(currentSelectLogData));
            }

            return data == currentSelectLogData;
        }
        #endregion
    }
    public partial class UI_LogWindow 
    {
        /// <summary>
        /// 日志选项
        /// </summary>
        [Flags]
        private enum LogOption
        {
            None = 0,
            Debug = 1 << 0,
            Log = 1 << 1,
            Warning = 1 << 2,
            Error = 1 << 3,
            Collapse = 1 << 4,
        }
        /// <summary>
        /// 折叠日志的key，
        /// 使用结构体代替字符串拼接Key，
        /// 避免频繁创建字符串产生GC。
        /// </summary>
        private readonly struct CollapseKey : IEquatable<CollapseKey>
        {
            public readonly LogType Type;
            public readonly string Message;
            public readonly string StackTrace;
            private readonly int hashCode;
            public CollapseKey(LogData data)
            {
                Type = data.Type;
                Message = data.Message;
                StackTrace = data.StackTrace;

                hashCode = HashCode.Combine( Type,Message, StackTrace);
            }
            public bool Equals(CollapseKey other)
            {
                return Type == other.Type
                    && Message == other.Message
                    && StackTrace == other.StackTrace;
            }
            public override bool Equals(object obj)
            {
                return obj is CollapseKey other && Equals(other);
            }
            public override int GetHashCode()
            {
                return hashCode;
            }
        }
        /// <summary>
        /// 日志视图数据
        /// </summary>
        private class LogViewData
        {
            public LogData LogData;
            public int RepeatCount;
            public bool ShowCount;
        }
    }
}