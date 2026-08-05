using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// GameObject对象池
    /// </summary>
    internal class GameObjectPoolModule
    {
        private Transform poolRootTransform;

        private Dictionary<string, GameObjectPoolData> poolDic = new Dictionary<string, GameObjectPoolData>();

        public GameObjectPoolModule(Transform rootTransform)
        {
            this.poolRootTransform = rootTransform;
        }

        #region 获取
        /// <summary>
        /// 获取对象
        /// </summary>
        public GameObject GetGameObject(string keyName, Transform parent = null, bool useHierarchy = true)
        {
            if(poolDic.TryGetValue(keyName,out GameObjectPoolData poolData))
            {
                return poolData.GetObj(parent, useHierarchy);
            }

            return null;
        }
        public T GetGameObject<T>(string keyName, Transform parent = null, bool useHierarchy = true) where T : Component
        {
            GameObject gameObject = GetGameObject(keyName, parent, useHierarchy);
            if(gameObject==null) return null;
            return gameObject.GetComponent<T>();
        }


        #endregion

        #region 放入
        /// <summary>
        /// 放入对象
        /// </summary>
        public void PushGameObject(GameObject gameObject, bool useHierarchy = true)
        {
            if (!poolDic.TryGetValue(gameObject.name, out GameObjectPoolData poolData))
            {
                poolData = CreateGameObjectPoolData(gameObject.name);
            }

           poolData.PushObj(gameObject, useHierarchy);
        }


        #endregion

        #region 清理
        public void ClearGameObject(string keyName)
        {
            if (poolDic.TryGetValue(keyName, out GameObjectPoolData poolData))
            {
                poolData.Destroy();
                poolDic.Remove(keyName);
            }
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


        private GameObjectPoolData CreateGameObjectPoolData(string keyName)
        {
            GameObjectPoolData poolData = new GameObjectPoolData();

            poolData.Init(keyName, poolRootTransform);
            poolDic.Add(keyName, poolData);

            return poolData;
        }
    }
}
