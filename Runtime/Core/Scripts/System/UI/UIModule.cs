using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// UI 模块
    /// </summary>
    internal class UIModule : MonoBehaviour
    {
        [SerializeField, LabelText("层级列表")]
        private UILayer[] layers;
        
        private Dictionary<string, UIWindowDefinition> definitionDic;
        private readonly Dictionary<string, UIWindowInstance> instancesDic = new Dictionary<string, UIWindowInstance>();
        private readonly Stack<string> navigationStack = new Stack<string>();     
        
        // 导航锁，防止导航期间栈被二次操作
        private bool isNavigating;
        
        public void Init(UISystemSetting setting)
        {
            for (int i = 0; i < layers.Length; i++) layers[i].Init();

            definitionDic = setting.WindowDefinitionDic;
        }
        
        #region UI窗口操作

        /// <summary>
        /// 获取指定类型的UI窗口
        /// </summary>
        public T Get<T>() where T : UIWindowBase
        {
            return GetInstance(typeof(T).Name)?.Window as T;
        }

        /// <summary>
        /// 同步打开UI窗口
        /// </summary>
        public bool Open(string windowKey)
        {
            UIWindowDefinition definition = GetDefinition(windowKey);
            
            if (definition == null)
            {
                LogSystem.Warning($"找不到的UI窗口数据 {windowKey}");
                return false;
            }

            UIWindowInstance instance = GetInstance(windowKey);
            
            // 如果UI窗口已存在，检查是否可以显示
            if (instance != null)
            {
                if (!instance.StateMachine.CanTransition(UIWindowState.Showing))
                {
                    LogSystem.Warning($"UI窗口重复打开 {windowKey} ,当前状态: {instance.State}");
                    return false;
                }
            }
            // UI窗口实例不存在，创建运行实例
            else
            {
                instance = CreateInstance(windowKey);

                UIWindowBase window = CreateWindowSync(definition);
                
                // 窗口创建失败，移除对应实例
                if (window == null)
                {
                    RemoveInstance(windowKey);
                    return false; 
                }

                instance.Window = window;
                window.OnCreate();
            }
            
            ShowWindow(definition, instance, null);
            return true;
        }

        public bool Open<T>() where T : UIWindowBase => Open(typeof(T).Name);

        /// <summary>
        /// 异步打开UI窗口
        /// </summary>
        public void OpenAsync<T>(string windowKey, Action<T> callback) where T : UIWindowBase
        {
            UIWindowDefinition definition = GetDefinition(windowKey);
            
            if (definition == null)
            {
                LogSystem.Warning($"找不到的窗口数据 {windowKey}");
                callback?.Invoke(null);
                return;
            }

            UIWindowInstance instance = GetInstance(windowKey);
            
            // UI窗口实例已存在，检查是否可以重新显示
            if (instance != null)
            {
                if (!instance.StateMachine.CanTransition(UIWindowState.Showing))
                {
                    LogSystem.Warning($"窗口重复打开 {windowKey} ,当前状态: {instance.State}");
                    callback?.Invoke(null);
                    return;
                }
            }
            // UI窗口实例不存在，创建运行实例
            else
            {
                instance = CreateInstance(windowKey);
            }
            
            // 已有窗口实例，直接复用
            if (instance.Window != null)
            {
                ShowWindow(definition, instance, () => { callback?.Invoke(instance.Window as T); });
                return;
            }
            
            // 如果没有缓存，异步创建
            CreateWindowAsync(definition, GetLayer(definition.Layer).Root, window =>
            {
                // 窗口创建失败，移除对应实例
                if (window == null)
                {
                    instance.StateMachine.ChangedState(UIWindowState.None);
                    RemoveInstance(windowKey);
                    callback?.Invoke(null);
                    return;
                }

                instance.Window = window;
                window.OnCreate();

                ShowWindow(definition, instance, () => { callback?.Invoke(instance.Window as T); });
            });
        }
        public void OpenAsync<T>(Action<T> callback) where T : UIWindowBase => OpenAsync(typeof(T).Name, callback);

        /// <summary>
        /// 隐藏UI窗口
        /// </summary>
        public bool Hide(string windowKey)
        {
            UIWindowInstance instance = GetInstance(windowKey);
            
            if (instance == null)
            {
                LogSystem.Warning($"找不到的窗口数据 {windowKey}");
                return false; 
            }

            if (!instance.StateMachine.CanTransition(UIWindowState.Hiding))
            {
                LogSystem.Warning($"窗口无法隐藏 , 当前状态: {instance.State}{windowKey}");
                return false;
            }
            
            UIWindowDefinition definition = GetDefinition(windowKey);

            if (definition == null)
            {
                LogSystem.Warning($"找不到的窗口数据 {windowKey}");
                return false;
            }

            HideWindow(definition, instance, null);
            return true;
        }

        public bool Hide<T>() where T : UIWindowBase => Hide(typeof(T).Name);

        /// <summary>
        /// 关闭UI窗口
        /// </summary>
        public void Close(string windowKey)
        {
            UIWindowInstance instance = GetInstance(windowKey);

            if (instance == null || instance.Window == null)
            {
                LogSystem.Warning($"窗口数据不存在 {windowKey}");
                return;
            }

            if (!instance.StateMachine.CanTransition(UIWindowState.Hiding) && !instance.StateMachine.CanTransition(UIWindowState.None))
            {
                LogSystem.Warning($"窗口无法关闭（当前状态: {instance.State}）{windowKey}");
                return;
            }

            UIWindowDefinition definition = GetDefinition(windowKey);

            if (definition == null)
            {
                LogSystem.Warning($"找不到的窗口数据 {windowKey}");
                return;
            }

            if (instance.State == UIWindowState.Shown)
            {
                HideWindow(definition, instance, () => { CompleteClose(definition, instance, windowKey); });
                return;
            }

            CompleteClose(definition, instance, windowKey);
        }
        
        public void Close<T>() where T : UIWindowBase => Close(typeof(T).Name);
        
        /// <summary>
        /// 强制关闭并清理所有窗口
        /// </summary>
        public void CloseAll()
        {
            foreach (KeyValuePair<string, UIWindowInstance> item in instancesDic)
            {
                UIWindowInstance instance = item.Value;

                if (instance == null || instance.Window == null)continue;

                instance.Window.OnHide();
                ReleaseWindow(instance);
                instance.StateMachine.Reset();
            }

            instancesDic.Clear();
            
            ClearNavigation();

            ClearLayers();
        }

         /// <summary>
        /// 打开导航窗口：隐藏当前栈顶窗口，打开新窗口并入栈
        /// </summary>
        public void Push<T>() where T : UIWindowBase
        {
            string windowKey = typeof(T).Name;

            if (isNavigating) return;
            
            // 保存当前栈顶，打开失败时恢复
            string prevKey = navigationStack.Count > 0 ? navigationStack.Peek() : null;
            
            // 隐藏当前栈顶窗口
            if (prevKey != null && GetInstance(prevKey) != null)
            {
                // 若隐藏失败，停止导航
                if (!Hide(prevKey)) return;
            }

            // 打开新窗口
            if (Open(windowKey))
            {
                // 新窗口入栈
                navigationStack.Push(windowKey);
            }
            // 打开失败：恢复前一个窗口
            else if (prevKey != null && GetInstance(prevKey) != null)
            {
                Open(prevKey);
            }
        }

        /// <summary>
        /// 异步打开导航窗口，打开失败时恢复之前的窗口
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

            // 保存当前栈顶，打开失败时恢复
            string prevKey = navigationStack.Count > 0 ? navigationStack.Peek() : null;

            // 隐藏当前栈顶窗口
            if (prevKey != null && GetInstance(prevKey) != null)
            {
                // 若隐藏失败
                if (!Hide(prevKey))
                {
                    isNavigating = false;
                    callback?.Invoke(null);
                    return;
                }
            }

            // 异步打开新窗口
            OpenAsync<T>(windowKey, OnOpened);
            
            return;
            
            void OnOpened(T window)
            {
                // 窗口打开完成
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
        /// 返回上一个导航窗口
        /// </summary>
        public void Pop()
        {
            if (isNavigating) return;
            if (navigationStack.Count == 0) return;

            string topKey = navigationStack.Pop();

            // 关闭出栈的窗口
            if (GetInstance(topKey) != null) Close(topKey);

            if (navigationStack.Count <= 0) return;
            
            // 恢复新的栈顶窗口
            string prevKey = navigationStack.Peek();
            if (GetInstance(prevKey) != null) Open(prevKey);
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

        #region 查询
        
        /// <summary>
        /// 获取UI窗口运行实例
        /// </summary>
        private UIWindowInstance GetInstance(string windowKey)
        {
            instancesDic.TryGetValue(windowKey, out UIWindowInstance instance);
            return instance;
        }
        
        /// <summary>
        /// 获取UI窗口定义
        /// </summary>
        private UIWindowDefinition GetDefinition(string windowKey)
        {
            definitionDic.TryGetValue(windowKey, out UIWindowDefinition definition);
            return definition;
        }
        
        #endregion

        #region 运行时数据
        
        /// <summary>
        /// 创建UI窗口运行实例
        /// </summary>
        private UIWindowInstance CreateInstance(string windowKey)
        {
            UIWindowInstance instance = new UIWindowInstance();
            instancesDic[windowKey] = instance;
            return instance;
        }
        
        /// <summary>
        /// 移除UI窗口运行实例
        /// </summary>
        private void RemoveInstance(string windowKey)
        {
            instancesDic.Remove(windowKey);
        }
        
        #endregion

        #region UI窗口资源
        
        /// <summary>
        /// 同步创建UI窗口
        /// </summary>
        private UIWindowBase CreateWindowSync(UIWindowDefinition definition)
        {
            if (!ValidatePrefabRef(definition)) return null;

            Transform parent = GetLayer(definition.Layer).Root;

            UIWindowBase window = ResSystem.InstantiateSync<UIWindowBase>( definition.PrefabRef, parent);

            if (window != null) window.gameObject.name = definition.WindowKey;

            return window;
        }
        
        /// <summary>
        /// 异步创建UI窗口
        /// </summary>
        private void CreateWindowAsync(UIWindowDefinition definition, Transform parent, Action<UIWindowBase> callback)
        {
            if (!ValidatePrefabRef(definition))
            {
                callback?.Invoke(null);
                return;
            }

            ResSystem.InstantiateAsync<UIWindowBase>(definition.PrefabRef, parent, window =>
            {
                if (window != null) window.gameObject.name = definition.WindowKey;
                
                callback?.Invoke(window);
            });
        }
        
        /// <summary>
        /// 释放UI窗口
        /// </summary>
        private void ReleaseWindow(UIWindowInstance instance)
        {
            if (instance.Window == null) return;

            instance.Window.OnRelease();
            GameObject.Destroy(instance.Window.gameObject);
            instance.Window = null;
        }
        
        /// <summary>
        /// 验证预制体引用是否有效
        /// </summary>
        private bool ValidatePrefabRef(UIWindowDefinition definition)
        {
            if (definition.PrefabRef != null && definition.PrefabRef.RuntimeKeyIsValid()) return true;
            LogSystem.Error($"窗口定义缺少有效的预制体引用: {definition.WindowKey}");
            return false;
        }
        
        #endregion
        
        #region 窗口流程
        
        /// <summary>
        /// 显示窗口
        /// </summary>
        private void ShowWindow(UIWindowDefinition definition, UIWindowInstance instance, Action onComplete)
        {
            instance.StateMachine.ChangedState(UIWindowState.Showing);
            
            // 添加UI窗口到对应层级
            AddWindowToLayer(definition.Layer, instance.Window);
            
            UIWindowBase window = instance.Window;
            window.gameObject.SetActive(true);

            window.OnShowing(() =>
            {
                if (window == null) return;
                
                window.OnShow();
                instance.StateMachine.ChangedState(UIWindowState.Shown);
                onComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// 隐藏窗口
        /// </summary>
        private void HideWindow(UIWindowDefinition definition, UIWindowInstance instance, Action onComplete)
        {
            instance.StateMachine.ChangedState(UIWindowState.Hiding);
            
            UIWindowBase window = instance.Window;
            window.OnHide();
            window.OnHiding(() =>
            {
                if (window == null)return;

                window.gameObject.SetActive(false);
                instance.StateMachine.ChangedState(UIWindowState.Hidden);
                
                // 从对应层级移除UI窗口
                RemoveWindowFromLayer(definition.Layer, instance.Window);
                onComplete?.Invoke();
            });
        }

    
        /// <summary>
        /// 完成窗口关闭
        /// </summary>
        private void CompleteClose(UIWindowDefinition definition, UIWindowInstance instance, string windowKey)
        {
            // 缓存窗口：保留实例，隐藏后完成关闭
            if (definition.IsCached) return;

            instance.StateMachine.ChangedState(UIWindowState.None);
            ReleaseWindow(instance);
            RemoveInstance(windowKey);
        }
        
        #endregion

        #region  UI层级

        /// <summary>
        /// 获取UI层级节点
        /// </summary>
        private UILayer GetLayer(int layerIndex)
        {
            if (layerIndex >= 0 && layerIndex < layers.Length) return layers[layerIndex];
            LogSystem.Error($"UI层级索引无效：{layerIndex}");
            return null;
        }
        
        /// <summary>
        /// 添加UI窗口到指定层级
        /// </summary>
        private void AddWindowToLayer(int layerIndex, UIWindowBase window)
        {
            if (window == null)
            {
                LogSystem.Warning($"添加UI窗口失败：窗口实例为空，层级索引: {layerIndex}");
                return;
            }
            
            UILayer layer = GetLayer(layerIndex);
            if (layer == null) return;

            layer.AddWindow(window);
        }
        
        
        /// <summary>
        /// 从指定层级移除UI窗口
        /// </summary>
        private void RemoveWindowFromLayer(int layerIndex, UIWindowBase window)
        {
            if (window == null)
            {
                LogSystem.Warning($"移除UI窗口失败：窗口实例为空，层级索引: {layerIndex}");
                return;
            }

            UILayer layer = GetLayer(layerIndex);
            if (layer == null) return;

            layer.RemoveWindow(window);
        }
        
        
        /// <summary>
        /// 清理所有层级下的所有窗口
        /// </summary>
        private void ClearLayers()
        {
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].ClearWindows();
            }
        }
        
        #endregion
       
    }
}
