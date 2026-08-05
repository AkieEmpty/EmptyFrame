using Unity.Netcode;
using UnityEngine;

namespace EmptyFrame.Network
{
    internal class NetworkPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        private NetworkObjectPoolModule poolModule;
        private GameObject prefab;

        public NetworkPrefabInstanceHandler(GameObject prefab, NetworkObjectPoolModule poolModule)
        {
            this.prefab = prefab;
            this.poolModule = poolModule;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            NetworkObject networkObject = poolModule.GetNetworkObject(prefab.name);

            if(networkObject == null)
            {
                networkObject = GameObject.Instantiate(prefab, position, rotation).GetComponent<NetworkObject>();
                networkObject.gameObject.name = prefab.name;
            }

            networkObject.transform.position = position;
            networkObject.transform.rotation = rotation;

            return networkObject;
        }

        public void Destroy(NetworkObject networkObject)
        {
            poolModule.PushNetworkObject(networkObject);
        }
    }
}
