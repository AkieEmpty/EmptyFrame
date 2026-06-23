using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// 本地化组件基类
    /// </summary>
    public abstract class LocalizationBehaviourBase<T>:MonoBehaviour where T : Component
    { 
        [SerializeField] protected string localizationKey;

        protected T component;

        protected virtual void Awake()
        {
            component = GetComponent<T>();
            if(component==null) Debug.LogError($"{GetType().Name} 未能找到组件: {typeof(T).Name}");
        }
       
        protected virtual void OnEnable()
        {
            LocalizationSystem.LanguageChanged += Refresh;

            Refresh();
        }

        protected virtual void OnDisable()
        {
            LocalizationSystem.LanguageChanged -= Refresh;
        }

        protected abstract void Refresh();
        /// <summary>
        /// 设置本地化Key
        /// </summary>
        /// <param name="localizationKey"></param>
        public void SetLocalizationKey(string localizationKey)
        {
            this.localizationKey = localizationKey;

            if(gameObject.activeInHierarchy) Refresh();
        }
    }
}
