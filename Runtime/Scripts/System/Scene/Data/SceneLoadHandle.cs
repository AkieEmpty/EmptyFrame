using System;
using System.Xml;

namespace EmptyFrame
{
    /// <summary>
    /// 场景异步加载操作句柄
    /// </summary>
    public class SceneLoadHandle
    {
        public string SceneKey { get; internal set; }
        /// <summary>
        /// 是否允许激活
        /// </summary>
        internal bool IsActivationAllowed { get; set; }       
        public SceneLoadState State { get; internal set; }
        public float Progress { get; internal set; }
        /// <summary>
        /// 当加载进度发生变更时触发
        /// </summary>
        public event Action<float> OnProgressChanged;
        /// <summary>
        /// 当场景开始加载时触发
        /// </summary>
        public event Action OnLoading;
        /// <summary>
        /// 当场景等待激活时触发
        /// </summary>
        public event Action OnAwaiting;
        /// <summary>
        /// 当场景开始激活时触发
        /// </summary>
        public event Action OnActivating;
        /// <summary>
        /// 当场景激活完成时触发
        /// </summary>
        public event Action OnCompleted;

        public SceneLoadHandle(LoadSceneParms parms)
        {
            SceneKey = parms.SceneKey;
            IsActivationAllowed = parms.ActivateOnLoad;
        }

        internal void ChangedState(SceneLoadState newState)
        {
            if (State == newState) return;

            State = newState;

            switch (newState)
            {
                case SceneLoadState.Loading:
                    OnLoading?.Invoke();
                    break;
                case SceneLoadState.Awaiting:
                    OnAwaiting?.Invoke();
                    break;
                case SceneLoadState.Activating:
                    OnActivating?.Invoke();
                    break;
                case SceneLoadState.Completed:
                    Complete();
                    break;
            }
        }

        internal void UpdateProgress(float progress)
        {
            Progress = progress;
            OnProgressChanged?.Invoke(progress);
        }

        internal void Complete()
        {
            OnCompleted?.Invoke();
            Clear();
        }
        /// <summary>
        /// 允许激活场景
        /// </summary>
        public void AllowActivation()
        {
            if (State == SceneLoadState.Awaiting)
            {
                IsActivationAllowed = true;
            }
        }

        private void Clear()
        {
            OnProgressChanged = null;

            OnLoading = null;
            OnAwaiting = null;
            OnActivating = null;
            OnCompleted = null;
        }

       
    }
}
