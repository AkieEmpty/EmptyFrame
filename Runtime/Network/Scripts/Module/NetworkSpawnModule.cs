using System.Collections.Generic;
using EmptyFrame.Core;
using Unity.Netcode;
using UnityEngine;

namespace EmptyFrame.Network
{

    internal class NetworkSpawnModule
    {
        private NetworkManager networkManager;
        private NetworkObjectPoolModule poolModule;

        private Dictionary<GameObject, NetworkPrefabInstanceHandler> prefabHandlerDic;

        public NetworkSpawnModule(NetworkManager networkManager,NetworkSystemSetting setting)
        {
            this.networkManager = networkManager;

            //关闭网络对象预制体列表检查,允许运行时动态注册网络预制体
            networkManager.NetworkConfig.ForceSamePrefabs = false;

            poolModule = new NetworkObjectPoolModule();
    
            prefabHandlerDic = new Dictionary<GameObject, NetworkPrefabInstanceHandler>();

            for (int i = 0; i < setting.NetworkPrefabsList.Count; i++)
            {
                NetworkPrefabsList prefabsList = setting.NetworkPrefabsList[i];

                for (int j = 0; j < prefabsList.PrefabList.Count; j++)
                {
                    AddNetworkPrefab(prefabsList.PrefabList[j].Prefab);
                }
            }
           
        }

        public void AddNetworkPrefab(GameObject prefab)
        {
            if (!prefab.TryGetComponent<NetworkObject>(out _))
            {
                LogSystem.Warning($"预制体缺少 NetworkObject 组件: {prefab.name}");
                return;
            }

            if (prefabHandlerDic.ContainsKey(prefab))
            {
                LogSystem.Warning($"网络对象预制体已注册: {prefab.name}");
                return;
            }
            
            networkManager.AddNetworkPrefab(prefab);

            NetworkPrefabInstanceHandler handler = new NetworkPrefabInstanceHandler(prefab, poolModule);
            prefabHandlerDic.Add(prefab, handler);
            networkManager.PrefabHandler.AddHandler(prefab, handler);
        }

        public NetworkObject Spawn(ulong clientId, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!networkManager.EnsureServer())return null;

            if (!prefabHandlerDic.TryGetValue(prefab,out NetworkPrefabInstanceHandler handler))
            {
                LogSystem.Warning($"网络对象预制体未注册 :{prefab.name}");
                return null;
            }

            return handler.Instantiate(clientId,position,rotation);
        }
        public void Despawn(NetworkObject networkObject,bool isDestory = true)
        {
            if (!networkManager.EnsureServer()) return ;

            if (!networkObject.IsSpawned) return;

            networkObject.Despawn(isDestory);
        }

        public void ClearPool()
        {
            poolModule.ClearAll();
        }
    }
}
