using System;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 窗口工厂：负责窗口配置加载、窗口实例化与回收
    /// </summary>
    internal class UIWindowFactory
    {
        private readonly GameObjectPoolModule objectPool;

        public UIWindowFactory(GameObjectPoolModule objectPool)
        {
            this.objectPool = objectPool;
        }

        #region 配置加载

        /// <summary>
        /// 同步加载窗口配置（优先使用缓存）
        /// </summary>
        public UIWindowConfigBase GetWindowConfig(UIWindowDefinition definition, UIWindowInstance instance)
        {
            if (instance.Config != null) return instance.Config;

            UIWindowConfigBase config = ResSystem.LoadAssetSync<UIWindowConfigBase>(definition.ConfigKey);
            if (config != null) instance.Config = config;
            else LogSystem.Error($"找不到窗口配置: {definition.ConfigKey}");

            return instance.Config;
        }

        /// <summary>
        /// 异步加载窗口配置（优先使用缓存）。加载完成后回调
        /// </summary>
        public void GetWindowConfigAsync(UIWindowDefinition definition, UIWindowInstance instance, Action<UIWindowConfigBase> callback)
        {
            if (instance.Config != null)
            {
                callback?.Invoke(instance.Config);
                return;
            }

            ResSystem.LoadAssetAsync<UIWindowConfigBase>(definition.ConfigKey, OnConfigLoaded);

            void OnConfigLoaded(UIWindowConfigBase config)
            {
                if (config != null) instance.Config = config;
                else LogSystem.Error($"找不到窗口配置: {definition.ConfigKey}");

                callback?.Invoke(instance.Config);
            }
        }

        #endregion

        #region 窗口实例化与回收

        /// <summary>
        /// 实例化窗口。若启用缓存则优先从对象池获取
        /// </summary>
        public UIWindowBase CreateWindow(UIWindowDefinition definition, UIWindowConfigBase config, Transform parent)
        {
            if (definition.IsCached)
            {
                UIWindowBase cachedWindow = objectPool.GetGameObject<UIWindowBase>(config.WindowPrefab.name, parent);
                if (cachedWindow != null) return cachedWindow;
            }

            UIWindowBase window = GameObject.Instantiate(config.WindowPrefab, parent).GetComponent<UIWindowBase>();
            window.gameObject.name = config.WindowPrefab.name;

            window.Init(config);
            return window;
        }

        /// <summary>
        /// 归还缓存窗口到对象池
        /// </summary>
        public void RecycleWindow(UIWindowInstance instance)
        {
            objectPool.PushGameObject(instance.Window.gameObject);
            instance.Window = null;
        }

        /// <summary>
        /// 回收窗口,启用缓存则归还对象池，否则直接销毁并释放 Config
        /// </summary>
        public void ReleaseWindow(UIWindowDefinition definition, UIWindowInstance instance)
        {
            if (definition.IsCached)
            {
                RecycleWindow(instance);
                return;
            }

            // 释放 Config 并销毁 GameObject
            if (instance.Config != null)
            {
                ResSystem.ReleaseAsset(instance.Config);
                instance.Config = null;
            }

            instance.Window.Uninit();
            GameObject.Destroy(instance.Window.gameObject);

            instance.Window = null;

        }

        #endregion
    }
}
