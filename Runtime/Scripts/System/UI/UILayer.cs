using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmptyFrame
{
    public class UILayer :MonoBehaviour
    {
        [SerializeField] private Image blockerImage;
        [SerializeField] private bool enableBlocker;

        private List<UIWindowBase> windowList = new List<UIWindowBase>();
        /// <summary>
        /// 添加窗口到该层级下
        /// </summary>
        public void AddWindow(UIWindowBase window)
        {
            windowList.Add(window);
            RefreshHierarchy();
        }
        /// <summary>
        /// 从该层级下移除一个窗口
        /// </summary>
        public void RemoveWindow(UIWindowBase window)
        {
            windowList.Remove(window);
            RefreshHierarchy();
        }
        private void RefreshHierarchy()
        {
            if (windowList.Count == 0)
            {
                blockerImage.raycastTarget = false;
                return;
            }
            //顶层窗口移动到层级末尾
            windowList[windowList.Count - 1].transform.SetAsLastSibling();

            //只有一个窗口
            if (windowList.Count == 1)
            {
                blockerImage.raycastTarget = false;
                return;
            }

            //将blockerImage移动到层级倒数第二位(顶层窗口下)
            blockerImage.transform.SetSiblingIndex(transform.childCount - 2);
            //开启/关闭射线拦截
            blockerImage.raycastTarget = enableBlocker;
 
        }
    }
}
