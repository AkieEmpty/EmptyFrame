using System;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI 模块（MonoBehaviour 桥接层）
    /// </summary>
    internal class UIModule : MonoBehaviour
    {
        private UIWindowFactory windowFactory;
        private UIWindowService windowService;

        #region 初始化

        public void Init(UISystemSetting setting, GameObjectPoolModule objectPool)
        {
            windowFactory = new UIWindowFactory(objectPool);
            UILayerManager layerManager = new UILayerManager(transform.GetComponentsInChildren<UILayer>());
            windowService = new UIWindowService(setting.WindowDefinitionDic, windowFactory, layerManager);
        }

        #endregion

        #region 独立窗口操作

        public T GetWindow<T>() where T : UIWindowBase
        {
            return windowService.Get<T>();
        }

        public void ShowWindow<T>() where T : UIWindowBase
        {
            windowService.Open(typeof(T).Name);
        }

        public void ShowWindowAsync<T>(Action<T> callback) where T : UIWindowBase
        {
            windowService.OpenAsync<T>(typeof(T).Name, callback);
        }

        public void HideWindow<T>() where T : UIWindowBase
        {
            windowService.Hide(typeof(T).Name);
        }

        public void CloseWindow<T>() where T : UIWindowBase
        {
            windowService.Close(typeof(T).Name);
        }

        public void CloseAllWindow()
        {
            windowService.CloseAll();
        }

        #endregion

        #region 导航窗口

        public void PushWindow<T>() where T : UIWindowBase
        {
            windowService.Push<T>();
        }

        public void PushWindowAsync<T>(Action<T> callback) where T : UIWindowBase
        {
            windowService.PushAsync<T>(callback);
        }

        public void PopWindow()
        {
            windowService.Pop();
        }
        #endregion
    }
}
