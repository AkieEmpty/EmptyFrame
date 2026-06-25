using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EmptyFrame
{
    public static class SceneSystem
    {

        private static Dictionary<SceneProvider, ISceneLoader> loaders;
        public static void Init()
        {
            loaders = new Dictionary<SceneProvider, ISceneLoader>
            {
                 { SceneProvider.SceneManager, new BuiltInSceneLoader() },
                 { SceneProvider.Addressables, new AddressablesSceneLoader() }
            };
        }

      
    }
}
