using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmptyFrame.Core
{
    /// <summary>
    /// GameObject对象池数据
    /// </summary>
    internal class GameObjectPoolData
    {
        public Transform RootTransform { get; private set; }
        public Stack<GameObject> PoolStack { get; private set; }


        public GameObjectPoolData()
        {
            PoolStack = new Stack<GameObject>();
        }

        /// <summary>
        /// 初始化分类节点
        /// </summary>
        public void Init(string assetPath, Transform poolRootTransform)
        {
            //创建该类物体的专属父节点
            GameObject go = new GameObject(assetPath);
            go.transform.SetParent(poolRootTransform);
            RootTransform = go.transform;
        }

        /// <summary>
        /// 将对象放进对象池
        /// </summary>
        /// <param name="useHierarchy">是否启用层级管理</param>
        public void PushObj(GameObject gameObject, bool useHierarchy = true)
        {
            gameObject.SetActive(false);
            if (useHierarchy)
            {
                gameObject.transform.SetParent(RootTransform);
            }
            PoolStack.Push(gameObject);
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="useHierarchy">是否启用层级管理</param>
        public GameObject GetObj(Transform parent = null, bool useHierarchy = true)
        {

            while (PoolStack.Count > 0)
            {
                GameObject gameObject = PoolStack.Pop();

                if (gameObject == null) continue;//跳过因意外情况销毁的实例

                gameObject.SetActive(true);
                if (useHierarchy)
                {
                    gameObject.transform.SetParent(parent);

                    if (parent == null) // 如果未指定父节点，将其放回当前活动场景，避免留在DontDestroyOnLoad等特殊场景
                    {
                        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
                    }
                }

                return gameObject;

            }

            return null;
        }

        /// <summary>
        /// 销毁对象池
        /// </summary>
        public void Destroy()
        {
            if (RootTransform != null)
            {
                GameObject.Destroy(RootTransform.gameObject);
            }

            PoolStack.Clear();
            RootTransform = null;
        }
    }
}
