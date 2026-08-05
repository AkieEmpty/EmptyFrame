using System;
using EmptyFrame.Core;
using Unity.Netcode;

namespace EmptyFrame.Network
{
    internal class NetworkConnectionModule
    {
        private NetworkManager networkManager;

        public NetworkConnectionModule(NetworkManager networkManager) 
        {
            this.networkManager = networkManager;
        }

        #region 连接、断开
        public bool StartClient()
        {
            if (!networkManager.EnsureIdle()) return false;

            bool result = networkManager.StartClient();

            if (!result) LogSystem.Warning("客户端连接失败");
            else LogSystem.Log("客户端连接成功");

            return result;
        }
        public bool StartServer()
        {
            if (!networkManager.EnsureIdle()) return false;

            bool result = networkManager.StartServer();

            if (!result) LogSystem.Warning("服务端连接失败");
            else LogSystem.Log("服务端连接成功");

            return result;
        }
        public bool StartHost()
        {
            if (!networkManager.EnsureIdle()) return false;

            bool result = networkManager.StartHost();

            if (!result) LogSystem.Warning("主机连接失败");
            else LogSystem.Log("主机连接成功");

            return result;
        }
        public void Stop(bool discardMessageQueue = false)
        {
            networkManager.Shutdown(discardMessageQueue);
            LogSystem.Log("网络已断开");
        }
        public void DisconnectClient(ulong clientId)
        {
            if (!networkManager.EnsureServer()) return;

            networkManager.DisconnectClient(clientId);
            LogSystem.Log($"已断开客户端: {clientId}");
        }
        #endregion

        #region 回调注册、取消
        public void RegisterClientConnected(Action<ulong> action)
        {
            networkManager.OnClientConnectedCallback += action;
        }
        public void RegisterClientDisconnect(Action<ulong> action)
        {
            networkManager.OnClientDisconnectCallback += action;
        }
        public void UnregisterClientConnected(Action<ulong> action)
        {
            networkManager.OnClientConnectedCallback -= action;
        }
        public void UnregisterClientDisconnect(Action<ulong> action)
        {
            networkManager.OnClientDisconnectCallback -= action;
        }
        #endregion
    }
}
