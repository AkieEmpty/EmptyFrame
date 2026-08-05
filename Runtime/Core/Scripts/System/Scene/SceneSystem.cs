using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace EmptyFrame.Core
{
    public static class SceneSystem
    {
        private static Dictionary<SceneProvider, ISceneLoader> loaders;
        public static void Init()
        {
            loaders = new Dictionary<SceneProvider, ISceneLoader>
            {
                 { SceneProvider.BuiltIn, new BuiltInSceneLoader() },
                 { SceneProvider.Addressables, new AddressablesSceneLoader() }
            };
        }
        public static SceneHandle LoadSceneSync(string sceneKey, SceneProvider provider = SceneProvider.BuiltIn, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return LoadSceneSync(new LoadSceneOptions()
            {
                SceneKey = sceneKey,
                Provider = provider,
                Mode = mode
            });

        }
        public static SceneHandle LoadSceneSync(LoadSceneOptions options)
        {
            ISceneLoader loader = GetSceneLoader(options.Provider);
            if (loader == null) return default;

            return loader.LoadSceneSync(options);
        }
        public static LoadSceneOperation LoadSceneAsync(string sceneKey, SceneProvider provider = SceneProvider.BuiltIn, bool autoActivate = true, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return LoadSceneAsync(new LoadSceneOptions()
            {
                SceneKey = sceneKey,
                Provider = provider,
                AutoActivate = autoActivate,
                Mode = mode,
            });
        }
        public static LoadSceneOperation LoadSceneAsync(LoadSceneOptions options)
        {
            ISceneLoader loader = GetSceneLoader(options.Provider);
            if (loader == null) return null;

            return loader.LoadSceneAsync(options);
        }

        public static void UnloadScene(SceneHandle sceneHandle)
        {
            ISceneLoader loader = GetSceneLoader(sceneHandle.Provider);
            if (loader == null) return;

            loader.UnloadScene(sceneHandle);
        }


        private static ISceneLoader GetSceneLoader(SceneProvider provider)
        {
            if(loaders.TryGetValue(provider,out ISceneLoader loader))
            {
                return loader;
            }
            LogSystem.Warning($"未找到场景加载器：{provider}");
            return null;
        }
    }
}
