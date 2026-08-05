using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI窗口服务
    /// </summary>
    internal class UIWindowService
    {
        private readonly UIWindowFactory windowFactory;
        private readonly UILayerManager layerManager;

        private readonly Dictionary<string, UIWindowDefinition> definitionDic;
        private readonly Dictionary<string, UIWindowInstance> instancesDic = new Dictionary<string, UIWindowInstance>();
        private readonly Stack<string> navigationStack = new Stack<string>();     
        
        // 导航锁，防止导航期间栈被二次操作
        private bool isNavigating;

        public UIWindowService(Dictionary<string, UIWindowDefinition> definitions, UIWindowFactory windowFactory, UILayerManager layerManager)
        {
            this.definitionDic = definitions;
            this.windowFactory = windowFactory;
            this.layerManager = layerManager;
        }

        #region 独立窗口

        /// <summary>
        /// 获取当前已打开的窗口实例
        /// </summary>
        public T Get<T>() where T : UIWindowBase
        {
            return GetInstance(typeof(T).Name)?.Window as T;
        }

       
        /// <summary>
        /// 同步显示窗口
        /// </summary>
        public void Open(string windowKey)
        {
            if (!ValidateShow(windowKey)) return;

            definitionDic.TryGetValue(windowKey, out var def);
            instancesDic.TryGetValue(windowKey, out var inst);

            if (inst == null || inst.Window == null)
            {
                // 首次创建（inst==null）才调用 OnCreate；缓存窗口从池取出（inst!=null, Window==null）跳过
                bool isFirstCreation = inst == null;
                if (inst == null) inst = CreateInstance(windowKey);

                UIWindowBase window = CreateWindow(def, inst);
                if (window == null)//如果为null则创建失败，需要移除运行时数据,防止影响后续逻辑
                {
                    RemoveInstance(windowKey);
                    return;
                }

                inst.Window = window;
                if (isFirstCreation) window.OnCreate();
            }

            TransitionToShowing(inst);
            ShowWindow(def, inst, null);
        }

        /// <summary>
        /// 异步显示窗口
        /// </summary>
        public void OpenAsync<T>(string windowKey, Action<T> callback) where T : UIWindowBase
        {
            if (!ValidateShow(windowKey))
            {
                callback?.Invoke(null);
                return;
            }

            definitionDic.TryGetValue(windowKey, out var def);
            instancesDic.TryGetValue(windowKey, out var inst);

            // 首次创建（inst==null）才调用 OnCreate
            bool isFirstCreate = inst == null;
            if (inst == null) inst = CreateInstance(windowKey);

            TransitionToShowing(inst);

            if (inst.Window == null)
            {
                MonoSystem.StartCoroutine(DoOpenAsync(def, inst, windowKey, isFirstCreate, callback));
            }
            else
            {
                ShowWindow(def, inst, () =>
                {
                    callback?.Invoke(inst.Window as T);
                });
            }
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public void Hide(string windowKey)
        {
            if (!ValidateHide(windowKey)) return;

            definitionDic.TryGetValue(windowKey, out var def);
            instancesDic.TryGetValue(windowKey, out var inst);

            HideWindow(def, inst, null);
        }

        /// <summary>
        /// 关闭窗口（先隐藏再销毁/回收）
        /// </summary>
        public void Close(string windowKey)
        {
            if (!ValidateClose(windowKey)) return;

            definitionDic.TryGetValue(windowKey, out var def);
            instancesDic.TryGetValue(windowKey, out var inst);

            if (inst.State == UIWindowState.Shown)
            {
                HideWindow(def, inst, () =>
                {
                    OnCloseComplete(def, inst, windowKey);
                });
            }
            else
            {
                OnCloseComplete(def, inst, windowKey);
            }
        }

        private void OnCloseComplete(UIWindowDefinition def, UIWindowInstance inst, string windowKey)
        {
            if (def.IsCached)
            {
                windowFactory.RecycleWindow(inst);
            }
            else
            {
                ReleaseWindow(def, inst);
                RemoveInstance(windowKey);
            }
        }

        /// <summary>
        /// 关闭所有窗口并清空导航栈
        /// </summary>
        public void CloseAll()
        {
            List<string> activeKeys = GetActiveWindowKeys();

            foreach (string windowKey in activeKeys)
            {
                instancesDic.TryGetValue(windowKey, out var inst);
                if (inst == null) continue;

                definitionDic.TryGetValue(windowKey, out var def);

                if (inst.State == UIWindowState.Shown)
                {
                    HideWindowImmediate(def, inst);
                }

                ReleaseWindow(def, inst);
            }

            // 清空所有
            foreach (var pair in instancesDic)
            {
                if (pair.Value.Config != null)
                {
                    ResSystem.ReleaseAsset(pair.Value.Config);
                    pair.Value.Config = null;
                }
            }

            instancesDic.Clear();
            ClearNavigation();
        }

        #endregion

        #region 导航窗口

        /// <summary>
        /// 打开导航窗口：隐藏当前栈顶窗口，显示新窗口并入栈。
        /// </summary>
        public void Push<T>() where T : UIWindowBase
        {
            string windowKey = typeof(T).Name;

            if (isNavigating) return;

            // 隐藏栈顶窗口
            if (navigationStack.Count > 0)
            {
                string prevKey = navigationStack.Peek();
                if (GetInstance(prevKey) != null) Hide(prevKey);   
            }

            // 显示新窗口
            Open(windowKey);

            // 入栈
            navigationStack.Push(windowKey);
        }

        /// <summary>
        /// 异步打开导航窗口：先隐藏栈顶，再加载新窗口。
        /// </summary>
        public void PushAsync<T>(Action<T> callback) where T : UIWindowBase
        {
            string windowKey = typeof(T).Name;

            if (isNavigating)
            {
                callback?.Invoke(null);
                return;
            }

            isNavigating = true;

            //上一个窗口
            string prevKey = navigationStack.Count > 0 ? navigationStack.Peek() : null;

            // 隐藏栈顶
            if (prevKey != null && GetInstance(prevKey) != null)
            {
                Hide(prevKey);
            }

            // 异步打开新窗口
            OpenAsync<T>(windowKey, OnOpened);


            void OnOpened(T window)
            {
                //打开完成
                if (window != null)
                {
                    navigationStack.Push(windowKey);
                    isNavigating = false;
                    callback?.Invoke(window);
                }
                // 失败：恢复前一个窗口
                else
                {                    
                    if (prevKey != null && GetInstance(prevKey) != null)
                    {
                        Open(prevKey);
                    }
                    isNavigating = false;
                    callback?.Invoke(null);
                }
            }
        }

        /// <summary>
        /// 导航窗口返回：关闭栈顶窗口，恢复上一个窗口
        /// </summary>
        public void Pop()
        {
            if (isNavigating) return;
            if (navigationStack.Count == 0) return;

            string topKey = navigationStack.Pop();

            // 关闭出栈的窗口
            if (GetInstance(topKey) != null) Close(topKey);

            // 恢复新栈顶窗口
            if (navigationStack.Count > 0)
            {
                string prevKey = navigationStack.Peek();
                if (GetInstance(prevKey) != null) Open(prevKey);
            }
        }

        /// <summary>
        /// 清空导航栈
        /// </summary>
        private void ClearNavigation()
        {
            navigationStack.Clear();
            isNavigating = false;
        }

        #endregion

        #region 内部 - 查询

        private UIWindowInstance GetInstance(string windowKey)
        {
            instancesDic.TryGetValue(windowKey, out var inst);
            return inst;
        }

        private UIWindowInstance CreateInstance(string windowKey)
        {
            var inst = new UIWindowInstance();
            instancesDic[windowKey] = inst;
            return inst;
        }

        private void RemoveInstance(string windowKey)
        {
            instancesDic.Remove(windowKey);
        }

        private List<string> GetActiveWindowKeys()
        {
            var result = new List<string>();
            foreach (var pair in instancesDic)
            {
                if (pair.Value.Window != null)
                    result.Add(pair.Key);
            }
            return result;
        }

        #endregion

        #region 内部 - 校验

        private bool ValidateShow(string windowKey)
        {
            if (!definitionDic.TryGetValue(windowKey, out var def))
            {
                LogSystem.Warning($"找不到的窗口数据 {windowKey}");
                return false;
            }
            if (instancesDic.TryGetValue(windowKey, out var inst)
                && !inst.StateMachine.CanTransition(UIWindowState.Showing))
            {
                LogSystem.Warning($"窗口重复打开 {windowKey}（当前状态: {inst.State}）");
                return false;
            }
            return true;
        }

        private bool ValidateHide(string windowKey)
        {
            if (!instancesDic.TryGetValue(windowKey, out var inst))
            {
                LogSystem.Warning($"找不到的窗口数据 {windowKey}");
                return false;
            }
            if (!inst.StateMachine.CanTransition(UIWindowState.Hiding))
            {
                LogSystem.Warning($"窗口无法隐藏（当前状态: {inst.State}）{windowKey}");
                return false;
            }
            return true;
        }

        private bool ValidateClose(string windowKey)
        {
            if (!instancesDic.TryGetValue(windowKey, out var inst) || inst.Window == null)
            {
                LogSystem.Warning($"窗口数据不存在 {windowKey}");
                return false;
            }
            if (!inst.StateMachine.CanTransition(UIWindowState.Hiding) && !inst.StateMachine.CanTransition(UIWindowState.None))
            {
                LogSystem.Warning($"窗口无法关闭（当前状态: {inst.State}）{windowKey}");
                return false;
            }
            return true;
        }

        #endregion

        #region 内部 - 窗口创建

        private UIWindowBase CreateWindow(UIWindowDefinition def, UIWindowInstance inst)
        {
            UIWindowConfigBase config = windowFactory.GetWindowConfig(def, inst);
            if (config == null) return null;

            Transform parent = layerManager.GetParent(def);
            return windowFactory.CreateWindow(def, config, parent);
        }

        private IEnumerator DoOpenAsync<T>(UIWindowDefinition def, UIWindowInstance inst, string windowKey, bool isFirstCreate, Action<T> callback) where T : UIWindowBase
        {
            UIWindowConfigBase config = null;
            bool loadDone = false;

            windowFactory.GetWindowConfigAsync(def, inst, (loaded) =>
            {
                config = loaded;
                loadDone = true;
            });

            while (!loadDone) yield return null;

            // 加载失败，回滚状态
            if (config == null)
            {
                inst.StateMachine.ChangedState(UIWindowState.None);
                RemoveInstance(windowKey);
                callback?.Invoke(null);
                yield break;
            }

            Transform parent = layerManager.GetParent(def);
            UIWindowBase window = windowFactory.CreateWindow(def, config, parent);

            // 实例化失败，回滚状态
            if (window == null)
            {
                inst.StateMachine.ChangedState(UIWindowState.None);
                RemoveInstance(windowKey);
                callback?.Invoke(null);
                yield break;
            }

            inst.Window = window;
            if (isFirstCreate) window.OnCreate();

            ShowWindow(def, inst, () =>
            {
                callback?.Invoke(window as T);
            });

        }

        #endregion

        #region 内部 - 生命周期步骤

        private void TransitionToShowing(UIWindowInstance inst)
        {
            inst.StateMachine.ChangedState(UIWindowState.Showing);
        }

        private void ShowWindow(UIWindowDefinition def, UIWindowInstance inst, Action onComplete)
        {
            layerManager.Add(def, inst);

            inst.Window.gameObject.SetActive(true);

            inst.Window.OnShowing(() =>
            {
                inst.Window.OnShow();
                inst.StateMachine.ChangedState(UIWindowState.Shown);
                onComplete?.Invoke();
            });
        }

        private void HideWindow(UIWindowDefinition def, UIWindowInstance inst,Action onComplete)
        {
            inst.StateMachine.ChangedState(UIWindowState.Hiding);

            inst.Window.OnHide();
            inst.Window.OnHiding(() =>
            {
                inst.Window.gameObject.SetActive(false);
                inst.StateMachine.ChangedState(UIWindowState.Hidden);

                layerManager.Remove(def, inst);
                onComplete?.Invoke();  
            });
        }

        /// <summary>
        /// 立即隐藏窗口:跳过过渡阶段直接隐藏
        /// </summary>
        private void HideWindowImmediate(UIWindowDefinition def, UIWindowInstance inst)
        {
            inst.StateMachine.ChangedState(UIWindowState.Hiding);
            inst.Window.OnHide();
            inst.Window.gameObject.SetActive(false);
            inst.StateMachine.ChangedState(UIWindowState.Hidden);
            layerManager.Remove(def, inst);
        }

        private void ReleaseWindow(UIWindowDefinition def, UIWindowInstance inst)
        {
            inst.Window.OnRelease();
            windowFactory.ReleaseWindow(def, inst);
            inst.StateMachine.ChangedState(UIWindowState.None);
        }

        #endregion
    }
}
