using System.Collections.Generic;
using EmptyFrame.Core;
using Unity.Netcode;

namespace EmptyFrame.Network
{
    /// <summary>
    /// 网络对象池
    /// </summary>
    internal class NetworkObjectPoolModule
    {
        private Dictionary<string, NetworkObjectPoolData> poolDic = new Dictionary<string, NetworkObjectPoolData>();

        public NetworkObject GetNetworkObject(string keyName)
        {
            if (poolDic.TryGetValue(keyName, out var poolData))
            {
                return poolData.Pop();
            }
            return null;
        }

        public void PushNetworkObject(NetworkObject networkObject)
        {
            if (!poolDic.TryGetValue(networkObject.name, out var poolData))
            {
                poolData = CreateNetworkObjectPoolData(networkObject.name);
            }
            poolData.Push(networkObject);
        }

        public void ClearNetworkObject(string keyName)
        {
            if (poolDic.TryGetValue(keyName, out var poolData))
            {
                poolData.Clear();
                poolDic.Remove(keyName);
            }
        }

        public void ClearAll()
        {
            foreach (var poolData in poolDic.Values)
            {
                poolData.Clear();
            }
            poolDic.Clear();
        }

        private NetworkObjectPoolData CreateNetworkObjectPoolData(string keyName)
        {
            NetworkObjectPoolData poolData = new NetworkObjectPoolData();

            poolDic.Add(keyName, poolData);

            return poolData;
        }
    }
}
