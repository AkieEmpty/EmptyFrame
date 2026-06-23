using System;
using System.Collections;
using UnityEngine;

namespace EmptyFrame
{
    /// <summary>
    /// MonoBehaviour系统,代理执行Update、协程
    /// </summary>
    public class MonoSystem : MonoBehaviour
    {
        private static MonoSystem instance;

        private Action onUpdate;
        private Action onLateUpdate;
        private Action onFixedUpdate;

        public static void Init()
        {
            instance = EmptyFrameRoot.RootTransform.GetComponentInChildren<MonoSystem>();
        }
        /// <summary>
        /// 添加Update监听
        /// </summary>
        public static void AddUpdateListener(Action action) => instance.onUpdate += action;
        /// <summary>
        /// 添加LateUpdate监听
        /// </summary>
        public static void AddLateUpdateListener(Action action) => instance.onLateUpdate += action;
        /// <summary>
        /// 添加FixedUpdate监听
        /// </summary>
        public static void AddFixedUpdateListener(Action action) => instance.onFixedUpdate += action;
        /// <summary>
        /// 移除Update监听
        /// </summary>
        public static void RemoveUpdateListener(Action action) => instance.onUpdate -= action;
        /// <summary>
        /// 移除LateUpdate监听
        /// </summary>
        public static void RemoveLateUpdateListener(Action action) => instance.onLateUpdate -= action;
        /// <summary>
        /// 移除FixedUpdate监听
        /// </summary>
        public static void RemoveFixedUpdateListener(Action action) => instance.onFixedUpdate -= action;
        /// <summary>
        /// 启动一个协程
        /// </summary>
        public static Coroutine StartRoutine(IEnumerator routine)
        {
           return instance.StartCoroutine(routine);
        }
        /// <summary>
        /// 关闭一个协程
        /// </summary>
        public static void StopRoutine(IEnumerator routine) => instance.StopCoroutine(routine);
        /// <summary>
        /// 关闭所有协程
        /// </summary>
        public static void StopAllRoutine() => instance.StopAllCoroutines();

        private void Update() => onUpdate?.Invoke();
        private void LateUpdate() => onLateUpdate?.Invoke();
        private void FixedUpdate() => onFixedUpdate?.Invoke();

    }
}
