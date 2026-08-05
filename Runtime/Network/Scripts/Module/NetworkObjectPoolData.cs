using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace EmptyFrame.Network
{
    /// <summary>
    /// 网络对象池数据
    /// </summary>
    internal class NetworkObjectPoolData
    {
        private Stack<NetworkObject> poolStack;

        public NetworkObjectPoolData()
        {
            poolStack = new Stack<NetworkObject>();
        }

        public void Push(NetworkObject networkObject)
        {
            networkObject.gameObject.SetActive(false);
            poolStack.Push(networkObject);
        }

        public NetworkObject Pop()
        {
            while (poolStack.Count > 0)
            {
                NetworkObject networkObject = poolStack.Pop();
                if (networkObject == null) continue;

                networkObject.gameObject.SetActive(true);
                return networkObject;
            }
            return null;
        }

        public void Clear()
        {
            while (poolStack.Count > 0)
            {
                NetworkObject networkObject = poolStack.Pop();
                if (networkObject != null)
                    GameObject.Destroy(networkObject.gameObject);
            }
        }
    }
}
