using System;
using System.Collections;
using UnityEngine;

namespace EmptyFrame.Core
{
    internal class MonoModule : MonoBehaviour
    {
        private Action onUpdate;
        private Action onLateUpdate;
        private Action onFixedUpdate;

        public void Init() { }


        private void Update() => onUpdate?.Invoke();
        private void LateUpdate() => onLateUpdate?.Invoke();
        private void FixedUpdate() => onFixedUpdate?.Invoke();

        #region 生命周期
        /// <summary>
        /// 添加Update监听
        /// </summary>
        public  void AddUpdateListener(Action action) => onUpdate += action;
        /// <summary>
        /// 添加LateUpdate监听
        /// </summary>
        public  void AddLateUpdateListener(Action action) => onLateUpdate += action;
        /// <summary>
        /// 添加FixedUpdate监听
        /// </summary>
        public  void AddFixedUpdateListener(Action action) => onFixedUpdate += action;
        /// <summary>
        /// 移除Update监听
        /// </summary>
        public  void RemoveUpdateListener(Action action) => onUpdate -= action;
        /// <summary>
        /// 移除LateUpdate监听
        /// </summary>
        public  void RemoveLateUpdateListener(Action action) => onLateUpdate -= action;
        /// <summary>
        /// 移除FixedUpdate监听
        /// </summary>
        public  void RemoveFixedUpdateListener(Action action) => onFixedUpdate -= action;
        #endregion

        #region 协程
        /// <summary>
        /// 启动一个协程
        /// </summary>
        public  Coroutine StartRoutine(IEnumerator routine) => StartCoroutine(routine);
        /// <summary>
        /// 关闭一个协程
        /// </summary>
        public  void StopRoutine(IEnumerator routine) => StopCoroutine(routine);
        /// <summary>
        /// 关闭一个协程
        /// </summary>
        public  void StopRoutine(Coroutine routine) => StopCoroutine(routine);
        /// <summary>
        /// 关闭所有协程
        /// </summary>
        public  void StopAllRoutine() => StopAllCoroutines();
        #endregion
    }
}
