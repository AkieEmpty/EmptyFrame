using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 单例模式基类(Mono)
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;

        public static  T Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError($"引用为空：尚未初始化 {typeof(T).Name} ！");
                }
                return instance;
            }
        }


        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}
