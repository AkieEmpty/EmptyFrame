using UnityEngine;

namespace EmptyFrame.Core
{

    public partial class EmptyFrameRoot : MonoBehaviour
    {
        private static EmptyFrameRoot instance;
        public static EmptyFrameRoot Instance { get => instance; }
        public static Transform RootTransform { get; private set; }

        private void Awake()
        {
            if(instance!=null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            RootTransform = transform; 
            InitSystem();
        }


        private void InitSystem()
        {
            LogSystem.Init();
            MonoSystem.Init();
            PoolSystem.Init();
            EventSystem.Init();
            SaveSystem.Init();
            ResSystem.Init();
            AudioSystem.Init();
            LocalizationSystem.Init();
            UISystem.Init();
            SceneSystem.Init();
        }
    }

#if UNITY_EDITOR

    public partial class EmptyFrameRoot : MonoBehaviour
    {

    }

   

#endif
}
