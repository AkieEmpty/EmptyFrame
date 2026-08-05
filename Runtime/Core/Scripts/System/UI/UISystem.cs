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
                Debug.LogError($"未找到 {nameof(UIModule)} 组件");
                return;
            }

            var setting = Resources.Load<UISystemSetting>("UISystemSetting");
            if (setting == null)
            {
                Debug.LogError("找不到 UISystemSetting 配置文件，请确保 Resources 文件夹中存在该资源");
                return;
            }
            var objectPool = new GameObjectPoolModule(PoolSystem.RootTransform);

            module.Init(setting, objectPool);
        }

        #endregion

        #region 独立窗口
        /// <summary>
        /// 获取当前已打开的窗口实例
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        /// <returns>窗口实例；未打开则返回 null</returns>
        public static T Get<T>() where T : UIWindowBase
        {
            return module.GetWindow<T>();
        }
        /// <summary>
        /// 同步显示窗口
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        public static void Show<T>() where T : UIWindowBase
        {
            module.ShowWindow<T>();
        }
        /// <summary>
        /// 异步显示窗口
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        /// <param name="callback">窗口显示完成后的回调，参数为窗口实例（失败时为 null）</param>
        public static void ShowAsync<T>(Action<T> callback) where T : UIWindowBase
        {
            module.ShowWindowAsync<T>(callback);
        }
        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        public static void Hide<T>() where T : UIWindowBase
        {
            module.HideWindow<T>();
        }
        /// <summary>
        /// 关闭窗口（先隐藏再销毁）
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        public static void Close<T>() where T : UIWindowBase
        {
            module.CloseWindow<T>();
        }
        /// <summary>
        /// 关闭所有已打开的窗口
        /// </summary>
        public static void CloseAll()
        {
            module.CloseAllWindow();
        }
        #endregion

        #region 导航窗口
        /// <summary>
        /// 打开窗口：隐藏当前栈顶窗口，显示新窗口并入栈
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        public static void Push<T>() where T : UIWindowBase
        {
            module.PushWindow<T>();
        }
        /// <summary>
        /// 异步打开窗口：隐藏当前栈顶窗口，异步显示新窗口并入栈
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        /// <param name="callback">窗口显示完成后的回调，参数为窗口实例（失败时为 null）</param>
        public static void PushAsync<T>(Action<T> callback) where T : UIWindowBase
        {
            module.PushWindowAsync<T>(callback);
        }
        /// <summary>
        /// 返回窗口：关闭栈顶窗口，恢复上一个窗口
        /// </summary>
        public static void Pop()
        {
            module.PopWindow();
        }
        #endregion
    }
}
