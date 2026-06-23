using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EmptyFrame
{
    public partial class UISystem : MonoBehaviour
    {
        private UISystemSetting setting;

        private GameObjectPoolModule gameObjectPoolModule;
        private List<UILayer> layerList;

        private static UISystem instance;

        public static void Init()
        {
            instance = EmptyFrameRoot.RootTransform.GetComponentInChildren<UISystem>();
            if (instance == null) Debug.LogError("[AudioSystem] Init 失败，未找到 AudioSystem 组件");

            instance.setting = EmptyFrameRoot.Setting.UISystemSetting; 

            instance.gameObjectPoolModule = new GameObjectPoolModule(PoolSystem.RootTransform);
            instance.InitUILayer();
        }

        private void InitUILayer()
        {
            UILayer[] layers = transform.GetComponentsInChildren<UILayer>();
            layerList = new List<UILayer>(layers.Length);
            for (int i = 0; i < layers.Length; i++)
                layerList.Add(layers[i]);
        }

        private UIWindowData GetWindowData(string windowKey)
        {
            setting.WindowDataDic.TryGetValue(windowKey, out UIWindowData windowData);
            return windowData;
        }

        /// <summary>
        /// 加载窗口配置
        /// </summary>
        private UIWindowConfigBase LoadWindowConfig(UIWindowData windowData)
        {
            if (windowData.Config != null) return windowData.Config;

            UIWindowConfigBase config = ResSystem.LoadAssetSync<UIWindowConfigBase>(windowData.ConfigKey);
            if (config != null)
                windowData.Config = config;
            else
                Debug.LogError($"[UISystem] 找不到 Key 为 {windowData.ConfigKey} 的窗口配置。");

            return windowData.Config;
        }

        private UILayer GetUILayer(int layerIndex) => layerList[layerIndex];

        private static T InstantiateWindow<T>(GameObject prefab, Transform parent) where T : UIWindowBase
        {
            T window = Instantiate(prefab, parent).GetComponent<T>();
            window.gameObject.name = prefab.name;
            return window;
        }
    }

    public partial class UISystem : MonoBehaviour
    {
        private static bool ValidateSystem()
        {
            if (instance != null && instance.setting != null) return true;
            Debug.LogError("[UISystem] UISystem 未初始化。");
            return false;
        }
        private static bool ValidateShowRequest(UIWindowData data, string windowKey)
        {
            if (data == null)
            {
                Debug.LogError($"[UISystem] 找不到 Key 为 {windowKey} 的窗口数据。");
                return false;
            }
            if (data.Window != null)
            {
                Debug.LogWarning($"[UISystem] 窗口 {windowKey} 重复打开。");
                return false;
            }
            return true;
        }
        private static bool ValidateCloseRequest(UIWindowData data, string windowKey)
        {
            if (data == null)
            {
                Debug.LogError($"[UISystem] 找不到 Key 为 {windowKey} 的窗口数据。");
                return false;
            }
            if (data.Window == null)
            {
                Debug.LogWarning($"[UISystem] 窗口 {windowKey} 重复关闭。");
                return false;
            }
            return true;
        }
   
        private T GetOrInstantiateWindow<T>(UIWindowData data, UIWindowConfigBase config, UILayer layer)  where T : UIWindowBase
        {
            if (data.IsCache)
            {
                T cachWindow = gameObjectPoolModule.GetGameObject<T>(config.WindowPrefab.name, layer.transform);
                if (cachWindow != null) return cachWindow;
            }

            T window = InstantiateWindow<T>(config.WindowPrefab, layer.transform);
            window.Init(config);
            return window;
        }


        /// <summary>
        /// 激活窗口
        /// </summary>
        private static void ActivateWindow(UIWindowData windowData, UIWindowBase window, UILayer layer)
        {
            windowData.Window = window;
            layer.AddWindow(window);
            window.OnShow();
        }

        /// <summary>
        /// 释放窗口
        /// </summary>
        private static void ReleaseWindow(UIWindowData data)
        {
            data.Window.OnClose();

            UILayer layer = instance.GetUILayer(data.Layer);
            layer.RemoveWindow(data.Window);

            if (data.IsCache)
            {
                instance.gameObjectPoolModule.PushGameObject(data.Window.gameObject);
            }
            else
            {
                data.Window.Uninit();
                Destroy(data.Window.gameObject);
            }

            data.Window = null;
        }
    }


    public partial class UISystem : MonoBehaviour
    {
        /// <summary>
        /// 获取当前已打开的窗口实例，未打开则返回 null
        /// </summary>
        public static T GetWindow<T>() where T : UIWindowBase
        {
            if (instance == null || instance.setting == null) return null;
            UIWindowData windowData = instance.GetWindowData(typeof(T).Name);
            return windowData?.Window as T;
        }

        /// <summary>
        /// 打开一个 UI 窗口
        /// </summary>
        public static void ShowWindow<T>() where T : UIWindowBase
        {
            if (!ValidateSystem()) return;

            string windowKey = typeof(T).Name;
            UIWindowData windowData = instance.GetWindowData(windowKey);

            if (!ValidateShowRequest(windowData, windowKey)) return;

            UIWindowConfigBase config = instance.LoadWindowConfig(windowData);
            if (config == null) return;

            UILayer layer = instance.GetUILayer(windowData.Layer);
            T window = instance.GetOrInstantiateWindow<T>(windowData, config, layer);

            ActivateWindow(windowData, window, layer);
        }

        /// <summary>
        /// 关闭一个 UI 窗口
        /// </summary>
        public static void CloseWindow<T>() where T : UIWindowBase
        {
            if (!ValidateSystem()) return;

            string windowKey = typeof(T).Name;
            UIWindowData windowData = instance.GetWindowData(windowKey);

            if (!ValidateCloseRequest(windowData, windowKey)) return;

            ReleaseWindow(windowData);
        }

        /// <summary>
        /// 关闭所有 UI 窗口
        /// </summary>
        public static void CloseAllWindow()
        {
            if (!ValidateSystem()) return;

            foreach (var item in instance.setting.WindowDataDic)
            {
                if (item.Value.Window != null)
                    ReleaseWindow(item.Value);
            }
        }
    }


#if UNITY_EDITOR
    public partial class UISystem : MonoBehaviour
    {
        private const string EmptyFrameSettingKey = "EmptyFrameSetting";

        [InitializeOnLoadMethod]
        public static void InitForEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            string[] guids = AssetDatabase.FindAssets(EmptyFrameSettingKey);
            if (guids.Length <= 0)
            {
                Debug.LogWarning($"[UISystem] 找不到 {nameof(EmptyFrameSetting)} 配置文件。");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var setting = AssetDatabase.LoadAssetAtPath<EmptyFrameSetting>(path);
            setting.UISystemSetting.InitUIWindowDataDicOnEditor();
        }
    }
#endif
}
