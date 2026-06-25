using System;
using UnityEngine.SceneManagement;

namespace EmptyFrame
{
    public struct LoadSceneParms
    {
        public string SceneKey;
        public SceneProvider LoadProvider;
        public LoadSceneMode LoadMode;
        public bool ActivateOnLoad;   
    }
}
