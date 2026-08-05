using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI 层级组件，管理同层级窗口的排序和遮罩。
    /// </summary>
    internal class UILayer : MonoBehaviour
    {
        [SerializeField] private Image blockerImage;
        [SerializeField] private bool enableBlocker;

        private readonly List<UIWindowBase> windowList = new List<UIWindowBase>();

        #region 窗口管理

        /// <summary>
        /// 添加窗口到该层级并刷新排序
        /// </summary>
        public void AddWindow(UIWindowBase window)
        {
            windowList.Add(window);
            RefreshHierarchy();
        }

        /// <summary>
        /// 从该层级移除窗口并刷新排序
        /// </summary>
        public void RemoveWindow(UIWindowBase window)
        {
            windowList.Remove(window);
            RefreshHierarchy();
        }

        #endregion

        #region 内部逻辑

        private void RefreshHierarchy()
        {
            RefreshWindowOrder();
            RefreshBlocker();
        }

        private void RefreshWindowOrder()
        {
            if (windowList.Count == 0) return;

            // 顶层窗口移动到层级末尾
            UIWindowBase topWindow = windowList[windowList.Count - 1];
            topWindow.transform.SetAsLastSibling();
        }

        private void RefreshBlocker()
        {
            // 仅一个窗口时不需要遮罩
            if (windowList.Count <= 1)
            {
                blockerImage.raycastTarget = false;
                return;
            }

            // 遮罩放在顶层窗口下方，拦截对底层窗口的点击
            blockerImage.transform.SetSiblingIndex(transform.childCount - 2);
            blockerImage.raycastTarget = enableBlocker;
        }

        #endregion
    }
}
