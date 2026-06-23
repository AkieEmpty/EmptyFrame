using UnityEngine;

namespace EmptyFrame
{

    public partial class EmptyFrameRoot : MonoBehaviour
    {
        [SerializeField] private EmptyFrameSetting setting;

        private static EmptyFrameRoot instance;
        public static EmptyFrameRoot Instance { get => instance; }
        public static EmptyFrameSetting Setting { get => instance.setting; }
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
            PoolSystem.Init();
            EventSystem.Init();
            SaveSystem.Init();
            ResSystem.Init();
            AudioSystem.Init();
            LocalizationSystem.Init();
            UISystem.Init();
        }
    }

#if UNITY_EDITOR

    public partial class EmptyFrameRoot : MonoBehaviour
    {

    }

   

#endif
}
