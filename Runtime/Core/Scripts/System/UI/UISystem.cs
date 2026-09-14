using System;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI 系统
    /// </summary>
    public static class UISystem
    {
        private static UIModule module;

        #region 初始化

        public static void Init()
        {
            module = EmptyFrameRoot.RootTransform.GetComponentInChildren<UIModule>();
            if (module == null)
            {
                LogSystem.Error($"未找到 {nameof(UIModule)} 组件");
                return;
            }

            UISystemSetting setting = Resources.Load<UISystemSetting>("UISystemSetting");
            if (setting == null)
            {
                LogSystem.Error("找不到 UISystemSetting 配置文件，请确保 Resources 文件夹中存在该资源");
                return;
            }

            module.Init(setting);
        }

        #endregion

        #region UI窗口操作
        /// <summary>
        /// 获取指定类型的UI窗口实例
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        /// <returns>窗口实例；不存在时返回 null</returns>
        public static T Get<T>() where T : UIWindowBase
        {
            return module.Get<T>();
        }
        /// <summary>
        /// 同步打开窗口
        /// </summary>
        public static void Open<T>() where T : UIWindowBase
        {
            module.Open<T>();
        }
        /// <summary>
        /// 异步打开窗口
        /// </summary>
        /// <param name="callback">窗口打开完成后的回调，参数为窗口实例；打开失败时为 null</param>
        public static void OpenAsync<T>(Action<T> callback) where T : UIWindowBase
        {
            module.OpenAsync(callback);
        }
        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public static void Hide<T>() where T : UIWindowBase
        {
            module.Hide<T>();
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        public static void Close<T>() where T : UIWindowBase
        {
            module.Close<T>();
        }
        /// <summary>
        /// 关闭所有窗口并清空导航栈
        /// </summary>
        public static void CloseAll()
        {
            module.CloseAll();
        }
        /// <summary>
        /// 打开导航窗口：隐藏当前栈顶窗口，打开新窗口并入栈
        /// </summary>
        public static void Push<T>() where T : UIWindowBase
        {
            module.Push<T>();
        }
        /// <summary>
        /// 异步打开导航窗口：隐藏当前栈顶窗口，打开新窗口并入栈
        /// </summary>
        public static void PushAsync<T>(Action<T> callback) where T : UIWindowBase
        {
            module.PushAsync(callback);
        }
        /// <summary>
        /// 返回上一个导航窗口
        /// </summary>
        public static void Pop()
        {
            module.Pop();
        }
        #endregion
    }
}
