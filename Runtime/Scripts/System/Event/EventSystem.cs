using System;

namespace EmptyFrame
{
    /// <summary>
    /// 事件系统
    /// </summary>
    public static class EventSystem
    {
        public static EventModule eventModule;

        public static void Init() 
        {
            eventModule = new EventModule();
            eventModule.Init();
        }

        #region 添加事件
        
        /// <summary>
        /// 添加无参数事件监听
        /// </summary>
        public static void AddEventListener(string eventName, Action action)
        {
            eventModule.AddEventListener(eventName, action);
        }
        /// <summary>
        /// 添加类型参数事件监听
        /// </summary>
        public static void AddEventListener<T>(Action<T> action)
        {
            eventModule.AddEventListener(typeof(T).Name, action);
        }
        /// <summary>
        /// 添加一个参数事件监听
        /// </summary>
        public static void AddEventListener<T>(string eventName, Action<T> action)
        {
            eventModule.AddEventListener<T>(eventName, action);
        }
        /// <summary>
        /// 添加两个参数事件监听
        /// </summary>
        public static void AddEventListener<T1, T2>(string eventName, Action<T1, T2> action)
        {
            eventModule.AddEventListener(eventName, action);
        }
        /// <summary>
        /// 添加三个参数事件监听
        /// </summary>
        public static void AddEventListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            eventModule.AddEventListener(eventName, action);
        }
        #endregion

        #region 移除事件
        /// <summary>
        /// 移除无参数事件监听
        /// </summary>
        public static void RemoveEventListener(string eventName, Action action)
        {
            eventModule.RemoveEventListener(eventName, action);
        }
        /// <summary>
        /// 移除类型参数事件监听
        /// </summary>
        public static void RemoveEventListener<T>(Action<T> action)
        {
            eventModule.RemoveEventListener(typeof(T).Name, action);
        }
        /// <summary>
        /// 移除一个参数事件监听
        /// </summary>
        public static void RemoveEventListener<T>(string eventName, Action<T> action)
        {
            eventModule.RemoveEventListener(eventName, action);
        }
        /// <summary>
        /// 移除二个参数事件监听
        /// </summary>
        public static void RemoveEventListener<T1, T2>(string eventName, Action<T1, T2> action)
        {
            eventModule.RemoveEventListener(eventName, action);
        }
        /// <summary>
        /// 移除三个参数事件监听
        /// </summary>
        public static void RemoveEventListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            eventModule.RemoveEventListener(eventName, action);
        }
        #endregion

        #region 触发事件
        /// <summary>
        /// 触发无参数事件
        /// </summary>
        public static void TriggerEvent(string eventName)
        {
            eventModule.TriggerEvent(eventName);
        }
        /// <summary>
        /// 触发类型参数事件
        /// </summary>
        public static void TriggerEvent<T>( T arg)
        {
            eventModule.TriggerEvent(typeof(T).Name, arg);
        }
        /// <summary>
        /// 触发一个参数事件
        /// </summary>
        public static void TriggerEvent<T>(string eventName, T arg)
        {
            eventModule.TriggerEvent(eventName,arg);
        }
        /// <summary>
        /// 触发两个参数事件
        /// </summary>
        public static void TriggerEvent<T1, T2>(string eventName, T1 arg1, T2 arg2)
        {
            eventModule.TriggerEvent(eventName, arg1,arg2);
        }
        /// <summary>
        /// 触发三个参数事件
        /// </summary>
        public static void TriggerEvent<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3)
        {
            eventModule.TriggerEvent(eventName, arg1, arg2,arg3);
        }
        #endregion

        #region 清理事件
        /// <summary>
        /// 清理参数事件
        /// </summary>
        public static void ClearEvent(string eventName)
        {
            eventModule.ClearEvent(eventName);
        }
        /// <summary>
        /// 清理类型参数事件
        /// </summary>
        public static void ClearEvent<T>()
        {
            eventModule.ClearEvent(typeof(T).Name);
        }
        #endregion
    }
}
