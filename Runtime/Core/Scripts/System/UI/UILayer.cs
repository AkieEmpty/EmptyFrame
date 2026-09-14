using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI 层级
    /// </summary>
    [Serializable]
    internal class UILayer
    {
        [SerializeField,BoxGroup]
        [LabelText("根节点")]private RectTransform root;
        
        [SerializeField,BoxGroup]
        [LabelText("遮罩")]private Image blockerImage;
        
        [SerializeField,BoxGroup]
        [LabelText("启用遮罩")]private bool enableBlocker;

        private List<UIWindowBase> windowList;

        public Transform Root => root;
        
        public void Init()
        {
            windowList = new List<UIWindowBase>();
        }

        #region 窗口管理

        /// <summary>
        /// 添加窗口到该层级
        /// </summary>
        public void AddWindow(UIWindowBase window)
        {
            windowList.Add(window);
            Refresh();
        }

        /// <summary>
        /// 从该层级移除窗口
        /// </summary>
        public void RemoveWindow(UIWindowBase window)
        {
            windowList.Remove(window);
            Refresh();
        }

        /// <summary>
        /// 清空该层级的所有窗口
        /// </summary>
        public void ClearWindows()
        {
            windowList.Clear();
            Refresh();
        }

        #endregion

        #region 层级显示
        
        /// <summary>
        /// 刷新层级显示
        /// </summary>
        private void Refresh()
        {
            RefreshWindowOrder();
            RefreshBlocker();
        }
        /// <summary>
        /// 刷新窗口显示顺序
        /// </summary>
        private void RefreshWindowOrder()
        {
            if (windowList.Count == 0) return;

            // 顶层窗口移动到层级末尾
            UIWindowBase topWindow = windowList[windowList.Count - 1];
            topWindow.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 刷新遮罩显示
        /// </summary>
        private void RefreshBlocker()
        {
            // 仅一个窗口时关闭遮罩拦截
            if (windowList.Count <= 1)
            {
                blockerImage.raycastTarget = false;
                return;
            }

            // 遮罩位于顶层窗口下方
            blockerImage.transform.SetSiblingIndex(root.childCount - 2);
            
            // 启用遮罩时拦截对其他窗口的点击
            blockerImage.raycastTarget = enableBlocker;
        }
        
        #endregion
        

    
    }
}