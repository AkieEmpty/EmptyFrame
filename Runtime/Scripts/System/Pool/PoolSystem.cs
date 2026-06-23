using System;
using UnityEngine;

namespace EmptyFrame
{
    public static class PoolSystem
    {
        private static GameObjectPoolModule gameObjectPool;
        private static ObjectPoolModule objectPool;
        public static Transform RootTransform {  get; private set; }

        public static void Init()
        {
            RootTransform = new GameObject("ObjectPoolRoot").transform;
            RootTransform.SetParent(EmptyFrameRoot.RootTransform);

            gameObjectPool = new GameObjectPoolModule(RootTransform);

            objectPool = new ObjectPoolModule();

        }

        #region GameObject对象池
        /// <summary>
        /// 从对象池获取GameObject对象,若对象池中没有返回null
        /// </summary>
        public static GameObject GetGameObject(string keyName, Transform parent = null)
        {
            return gameObjectPool.GetGameObject(keyName, parent);
        }
        public static GameObject GetGameObject(GameObject gameObject,Transform parent = null)
        {
            return GetGameObject(gameObject.name,parent);
        }
        public static T GetGameObject<T>(GameObject gameObject, Transform parent = null)where T:Component
        {
            return gameObjectPool.GetGameObject<T>(gameObject.name, parent);
        }
        /// <summary>
        /// 将GameObject对象放入对象池
        /// </summary>
        public static void PushGameObject(GameObject gameObject)
        {
            gameObjectPool.PushGameObject(gameObject);
        }
        /// <summary>
        /// 清除某个GameObject在对象池中的所有数据
        /// </summary>
        public static void ClearGameObject(string keyName)
        {
            gameObjectPool.ClearGameObject(keyName);
        }

        #endregion

        #region Object对象池

        public static T GetObject<T>() where T : class
        {
            return objectPool.GetObject<T>();
        }
      
        public static object GetObject(Type type)
        {
            return objectPool.GetObject(type);
        }

        public static void PushObject(object obj)
        {
            objectPool.PushObject(obj);
        }
    

        public static void ClearObject<T>()
        {
            objectPool.ClearObject<T>();
        }
        public static void ClearObject(Type type)
        {
            objectPool.ClearObject(type);
        }
       

        #endregion

        public static void ClearAllPool()
        {
            gameObjectPool.ClearAll();

            objectPool.ClearAll();
            
        }
    }
}
