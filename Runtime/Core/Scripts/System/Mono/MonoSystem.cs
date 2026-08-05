using System;
using System.Collections;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// MonoBehaviour系统,代理执行Update、协程
    /// </summary>
    public static class MonoSystem 
    {
        private static MonoModule module;

        public static void Init()
        {
            module = EmptyFrameRoot.RootTransform.GetComponentInChildren<MonoModule>();
            module.Init();
        }

        #region 生命周期
        /// <summary>
        /// 添加Update监听
        /// </summary>
        public static void AddUpdateListener(Action action) => module.AddUpdateListener(action);
        /// <summary>
        /// 添加LateUpdate监听
        /// </summary>
        public static void AddLateUpdateListener(Action action) => module.AddLateUpdateListener(action);
        /// <summary>
        /// 添加FixedUpdate监听
        /// </summary>
        public static void AddFixedUpdateListener(Action action) => module.AddFixedUpdateListener(action);
        /// <summary>
        /// 移除Update监听
        /// </summary>
        public static void RemoveUpdateListener(Action action) => module.RemoveUpdateListener(action);
        /// <summary>
        /// 移除LateUpdate监听
        /// </summary>
        public static void RemoveLateUpdateListener(Action action) => module.RemoveLateUpdateListener(action);
        /// <summary>
        /// 移除FixedUpdate监听
        /// </summary>
        public static void RemoveFixedUpdateListener(Action action) => module.RemoveFixedUpdateListener(action);
        #endregion

        #region 协程
        /// <summary>
        /// 启动一个协程
        /// </summary>
        public static Coroutine StartCoroutine(IEnumerator enumerator) => module.StartRoutine(enumerator);
        /// <summary>
        /// 关闭一个协程
        /// </summary>
        public static void StopCoroutine(IEnumerator enumerator) => module.StopRoutine(enumerator);
        /// <summary>
        /// 关闭一个协程
        /// </summary>
        public static void StopCoroutine(Coroutine routine) => module.StopRoutine(routine);
        /// <summary>
        /// 关闭所有协程
        /// </summary>
        public static void StopAllCoroutine() => module.StopAllRoutine();
        #endregion



    }
}
