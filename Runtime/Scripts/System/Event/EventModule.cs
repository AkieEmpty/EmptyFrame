using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace EmptyFrame
{
    public partial class EventModule
    {
        private interface IEventInfo 
        {
            void Clear();
        }
        /// <summary>
        /// 添加无参数事件监听
        /// </summary>
        private class EventInfo : IEventInfo
        {
            public Action Action;
            public void Clear() => Action = null;
        }
        /// <summary>
        /// 添加一个参数事件监听
        /// </summary>
        private class EventInfo<T> : IEventInfo  
        {
            public Action<T> Action;
            public void Clear() => Action = null;
        }
        /// <summary>
        /// 添加二个参数事件监听
        /// </summary>
        private class EventInfo<T1,T2> : IEventInfo
        {
            public Action<T1, T2> Action;
            public void Clear() => Action = null;
        }
        /// <summary>
        /// 添加三个参数事件监听
        /// </summary>
        private class EventInfo<T1,T2,T3> : IEventInfo
        {
            public Action<T1, T2, T3> Action;
            public void Clear() => Action = null;
        }
        //.........
    }

    public partial class EventModule
    {
        private ObjectPoolModule objectPoolModule;

        private Dictionary<string, IEventInfo> eventDic;

        public void Init()
        {
            objectPoolModule = new ObjectPoolModule();
            eventDic = new Dictionary<string, IEventInfo>();
        }

        #region 添加事件
        /// <summary>
        /// 添加无参数事件监听
        /// </summary>
        public void AddEventListener(string eventName, Action action)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo).Action += action;
            }
            else
            {
                EventInfo eventInfo = objectPoolModule.GetObject<EventInfo>() ?? new EventInfo();
                eventInfo.Action += action;
                eventDic.Add(eventName, eventInfo);
            }
        }
        /// <summary>
        /// 添加一个参数事件监听
        /// </summary>
        public void AddEventListener<T>(string eventName, Action<T> action)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T>).Action += action;
            }
            else
            {
                EventInfo<T> eventInfo = objectPoolModule.GetObject<EventInfo<T>>() ?? new EventInfo<T>();
                eventInfo.Action += action;
                eventDic.Add(eventName, eventInfo);
            }
        }
        /// <summary>
        /// 添加两个参数事件监听
        /// </summary>
        public void AddEventListener<T1,T2>(string eventName, Action<T1,T2> action)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T1, T2>).Action += action;
            }
            else
            {
                EventInfo<T1, T2> eventInfo = objectPoolModule.GetObject<EventInfo<T1, T2>>() ?? new EventInfo<T1, T2>();
                eventInfo.Action += action;
                eventDic.Add(eventName, eventInfo);
            }
        }
        /// <summary>
        /// 添加三个参数事件监听
        /// </summary>
        public void AddEventListener<T1,T2,T3>(string eventName, Action<T1,T2,T3> action)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T1, T2, T3>).Action += action;
            }
            else
            {
                EventInfo<T1, T2, T3> eventInfo = objectPoolModule.GetObject<EventInfo<T1, T2, T3>>() ?? new EventInfo<T1, T2, T3>();
                eventInfo.Action += action;
                eventDic.Add(eventName, eventInfo);
            }
        }
        #endregion

        #region 移除事件
        /// <summary>
        /// 移除无参数事件监听
        /// </summary>
        public void RemoveEventListener(string eventName, Action action)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo).Action -= action;
            }
        }
        /// <summary>
        /// 移除一个参数事件监听
        /// </summary>
        public void RemoveEventListener<T>(string eventName, Action<T> action)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T>).Action -= action;
            }
        }
        /// <summary>
        /// 移除二个参数事件监听
        /// </summary>
        public void RemoveEventListener<T1,T2>(string eventName, Action<T1, T2> action)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T1, T2>).Action -= action;
            }
        }
        /// <summary>
        /// 移除三个参数事件监听
        /// </summary>
        public void RemoveEventListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T1, T2, T3>).Action -= action;
            }
        }
        #endregion

        #region 触发事件
        /// <summary>
        /// 触发无参数事件
        /// </summary>
        public void TriggerEvent(string eventName)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo).Action?.Invoke();
            }
        }
        /// <summary>
        /// 触发一个参数事件
        /// </summary>
        public void TriggerEvent<T>(string eventName,T arg)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T>).Action?.Invoke(arg);
            }
        }
        /// <summary>
        /// 触发两个参数事件
        /// </summary>
        public void TriggerEvent<T1,T2>(string eventName, T1 arg1,T2 arg2)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T1, T2>).Action?.Invoke(arg1,arg2);
            }
        }
        /// <summary>
        /// 触发三个参数事件
        /// </summary>
        public void TriggerEvent<T1, T2 , T3>(string eventName, T1 arg1, T2 arg2,T3 arg3)
        {
            if (eventDic.TryGetValue(eventName, out IEventInfo info))
            {
                (info as EventInfo<T1, T2, T3>).Action?.Invoke(arg1, arg2, arg3);
            }
        }
        #endregion

        #region 清理事件
        /// <summary>
        /// 清理事件
        /// </summary>
        public void ClearEvent(string eventName)
        {
            if (eventDic.Remove(eventName, out IEventInfo info))
            {
                info.Clear();
                objectPoolModule.PushObject(info);
            }
        }
       
        #endregion
    }
}
