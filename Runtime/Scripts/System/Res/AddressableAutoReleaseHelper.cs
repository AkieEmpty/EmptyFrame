using System;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// Addressables 实例资源自动释放组件
    /// </summary>
    public class AddressableAutoReleaseHelper : MonoBehaviour
    {
        private Action<GameObject> onDestroyAction;

        public void Init(Action<GameObject> action)
        {
            this.onDestroyAction = action;
        }

        private void OnDestroy()
        {
            if (onDestroyAction != null)
            {
                onDestroyAction.Invoke(this.gameObject);
                onDestroyAction = null;
            }
        }
    }
}
