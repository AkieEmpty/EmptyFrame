using System.Collections.Generic;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    ///  普通类对象池数据
    /// </summary>
    internal class ObjectPoolData
    {
        public Stack<object> PoolStack { get; private set; }

        public ObjectPoolData()
        {
            PoolStack = new Stack<object>();
        }

        public void PushObj(object obj)
        {
            PoolStack.Push(obj);
        }

        public object GetObj()
        {
            if (PoolStack.Count == 0) return null;

            return PoolStack.Pop();
        }
        /// <summary>
        /// 销毁对象池
        /// </summary>
        public void Destroy()
        {
            PoolStack.Clear();

        }
    }
}
