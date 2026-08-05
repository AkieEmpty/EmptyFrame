using System;
using UnityEngine;

namespace EmptyFrame.Core
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
        /// <param name="useHierarchy">是否启用层级管理</param>
        public static GameObject GetGameObject(string keyName, Transform parent = null, bool useHierarchy = true)
        {
            return gameObjectPool.GetGameObject(keyName, parent, useHierarchy);
        }
        public static GameObject GetGameObject(GameObject gameObject, Transform parent = null, bool useHierarchy = true)
        {
            return GetGameObject(gameObject.name, parent, useHierarchy);
        }
        public static T GetGameObject<T>(GameObject gameObject, Transform parent = null, bool useHierarchy = true)where T:Component
        {
            return gameObjectPool.GetGameObject<T>(gameObject.name, parent, useHierarchy);
        }
        /// <summary>
        /// 将GameObject对象放入对象池
        /// </summary>
        /// <param name="useHierarchy">是否启用层级管理</param>
        public static void PushGameObject(GameObject gameObject, bool useHierarchy = true)
        {
            gameObjectPool.PushGameObject(gameObject, useHierarchy);
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

        /// <summary>
        /// 从对象池获取Object对象,若对象池中没有返回null
        /// </summary>
        public static T GetObject<T>() where T : class
        {
            return objectPool.GetObject<T>();
        }

        /// <summary>
        /// 从对象池获取Object对象,若对象池中没有返回null
        /// </summary>
        public static object GetObject(Type type)
        {
            return objectPool.GetObject(type);
        }

        /// <summary>
        /// 将Object对象放入对象池
        /// </summary>
        public static void PushObject(object obj)
        {
            objectPool.PushObject(obj);
        }

        /// <summary>
        /// 清除某个类型在对象池中的所有数据
        /// </summary>
        public static void ClearObject<T>()
        {
            objectPool.ClearObject<T>();
        }
        /// <summary>
        /// 清除某个类型在对象池中的所有数据
        /// </summary>
        public static void ClearObject(Type type)
        {
            objectPool.ClearObject(type);
        }

        #endregion

        /// <summary>
        /// 清除所有对象池数据（GameObject和Object）
        /// </summary>
        public static void ClearAllPool()
        {
            gameObjectPool.ClearAll();

            objectPool.ClearAll();

        }
    }
}
