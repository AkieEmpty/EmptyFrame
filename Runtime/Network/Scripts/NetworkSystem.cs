using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
namespace EmptyFrame.Network
{
    /// <summary>
    /// ����ϵͳ
    /// </summary>
    public static class NetworkSystem
    {
        private static UnityTransport transport;
        private static NetworkSystemSetting setting;
        private static NetworkManager networkManager;
        
        public static bool IsServer => networkManager.IsServer;
        public static bool IsClient => networkManager.IsClient;
        public static bool IsHost => networkManager.IsHost;
        public static bool IsConnectedClient => networkManager.IsConnectedClient;

        public static void Init()
        {
            NetworkVariableSerializtion.Init();

            setting = Resources.Load<NetworkSystemSetting>("NetworkSystemSetting");
            if (setting == null)
            {
                Debug.LogError("找不到 NetworkSystemSetting 配置文件，请确保 Resources 文件夹中存在该资源");
                return;
            }
            if (setting.NetworkManager == null)
            {
                Debug.LogError("NetworkSystemSetting.NetworkManager 未赋值");
                return;
            }
            GameObject go = GameObject.Instantiate(setting.NetworkManager);
            go.name = nameof(NetworkManager);

            GameObject.DontDestroyOnLoad(go);

            networkManager = go.GetComponent<NetworkManager>();
            transport = go.GetComponent<UnityTransport>();

            connectionModule = new NetworkConnectionModule(networkManager);
            spawnModule = new NetworkSpawnModule(networkManager,setting);
            messageModule = new NetworkMessageModule(networkManager);
        }


        #region ��������
        private static NetworkConnectionModule connectionModule;

        public static event Action<ulong> OnClientConnected
        {
            add => networkManager.OnClientConnectedCallback += value;
            remove => networkManager.OnClientConnectedCallback -= value;
        }
        public static event Action<ulong> OnClientDisconnect
        {
            add => networkManager.OnClientDisconnectCallback += value;
            remove => networkManager.OnClientDisconnectCallback -= value;
        }

        public static bool StartClient()
        {
            bool result = connectionModule.StartClient();

            if (result) messageModule.OnNetworkStart();

            return result;
        }
        public static bool StartServer()
        {
            bool result = connectionModule.StartServer();

            if (result) messageModule.OnNetworkStart();

            return result;
        }
        public static bool StartHost()
        {
            bool result = connectionModule.StartHost();

            if (result) messageModule.OnNetworkStart();

            return result;
        }
        public static void Stop(bool discardMessageQueue = false)
        {
            connectionModule.Stop(discardMessageQueue);

            messageModule.OnNetworkStop();

            spawnModule.ClearPool();
        }
        public static void DisconnectClient(ulong clientId)
        {
            connectionModule.DisconnectClient(clientId);
        }

        #endregion

        #region �������
        private static NetworkSpawnModule spawnModule;
        public static NetworkObject SpawnObject(ulong clientId, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return spawnModule.Spawn(clientId, prefab, position, rotation);
        }
        public static void DespawnObject(NetworkObject networkObject)
        {
            spawnModule.Despawn(networkObject);
        }

        public static void RegisterNetworkPrefab(GameObject prefab)
        {
            spawnModule.AddNetworkPrefab(prefab);
        }
        #endregion

        #region ������Ϣ
        private static NetworkMessageModule messageModule;
        public static void RegisterMessage<T>(Action<ulong, T> action) where T : struct, INetworkSerializable
        {
            messageModule.Register<T>(action);
        }

        public static void UnregisterMessage<T>(Action<ulong, T> action) where T : struct, INetworkSerializable
        {
            messageModule.Unregister<T>(action);
        }

        public static void SendMessageToClient<T>(ulong clientId, T message) where T : struct, INetworkSerializable
        {
            messageModule.SendToClient<T>(clientId,message);
        }

        public static void SendMessageToServer<T>(T message) where T : struct, INetworkSerializable
        {
            messageModule.SendToServer<T>(message);
        }

        public static void SendMessageToAllClient<T>(T message) where T : struct, INetworkSerializable
        {
            messageModule.SendToAllClient<T>(message);
        }
        #endregion
    }
}