using UnityEngine.SceneManagement;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 加载场景选项
    /// </summary>
    public struct LoadSceneOptions
    {
        public string SceneKey;
        public SceneProvider Provider;
        public LoadSceneMode Mode;
        public bool AutoActivate;   
    }
}
