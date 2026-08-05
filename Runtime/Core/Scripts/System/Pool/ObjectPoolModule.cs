using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 普通类对象池
    /// </summary>
    internal class ObjectPoolModule
    {
        private Dictionary<Type, ObjectPoolData> poolDic = new Dictionary<Type, ObjectPoolData>();

        #region 获取
        public object GetObject(Type type)
        {
            if (poolDic.TryGetValue(type, out ObjectPoolData poolData))
            {
                return poolData.GetObj();
            }
            return null;
        }

        public T GetObject<T>() where T : class
        {
            return GetObject(typeof(T)) as T; 
        }

        #endregion

        #region 放入
        public void PushObject(object obj)
        {
            if (obj == null) return;
            Type type = obj.GetType();
            if (!poolDic.TryGetValue(type, out ObjectPoolData poolData))
            {
                poolData = CreateObjectPoolData(type);
            }
            poolData.PushObj(obj);
        }

        #endregion

        #region 清理
        public void ClearObject(Type type)
        {
            if (poolDic.TryGetValue(type, out ObjectPoolData objectPoolData))
            {
                objectPoolData.Destroy();
                poolDic.Remove(type);
            }
        }
        public void ClearObject<T>()
        {
            ClearObject(typeof(T));
        }
        
        public void ClearAll()
        {
            foreach (var poolData in poolDic.Values)
            {
                poolData.Destroy();
            }
            poolDic.Clear();
        }

        #endregion
        private ObjectPoolData CreateObjectPoolData(Type type)
        {
            ObjectPoolData poolData = new ObjectPoolData();
            poolDic.Add(type, poolData);
            return poolData;
        }
    }
}
