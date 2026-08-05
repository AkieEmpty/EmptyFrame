using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace EmptyFrame.Network
{
    internal partial class NetworkMessageModule
    {
        private CustomMessagingManager messagingManager;
        private NetworkManager networkManager;

        private Dictionary<uint, IMessageHandler> handlerDic = new Dictionary<uint, IMessageHandler>();

        public NetworkMessageModule(NetworkManager networkManager)
        {
            this.networkManager = networkManager;
           

        }

        public void OnNetworkStart()
        {
            messagingManager = networkManager.CustomMessagingManager;
            messagingManager.OnUnnamedMessage += OnMessageReceived;
        }
        public void OnNetworkStop()
        {
            if (messagingManager == null) return;

            messagingManager.OnUnnamedMessage -= OnMessageReceived;
            messagingManager = null;
            
        }
        #region 消息注册、注销
        public void Register<T>(Action<ulong, T> action) where T : struct, INetworkSerializable
        {
            uint hash = GetTypeHash<T>();

            if(handlerDic.TryGetValue(hash,out IMessageHandler handler))
            {
                (handler as MessageHandler<T>).OnMessage += action;
            }
            else
            {
                MessageHandler<T> messageHandler = new MessageHandler<T>();
                messageHandler.OnMessage += action;
                handlerDic.Add(hash, messageHandler);
            }
        }
        public void Unregister<T>(Action<ulong, T> action) where T : struct, INetworkSerializable
        {
            uint hash = GetTypeHash<T>();

            if (handlerDic.TryGetValue(hash, out IMessageHandler handler))
            {
                (handler as MessageHandler<T>).OnMessage -= action;
            }
        }
        #endregion

        #region 消息发送
        public void SendToServer<T>(T message) where T : struct, INetworkSerializable
        {
            if (!networkManager.EnsureClient()) return;

            using (FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp, 10240))
            {
                writer.WriteValueSafe(GetTypeHash<T>());
                writer.WriteValueSafe(message);
                messagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer);
            }
        }
        public void SendToClient<T>(ulong clientId, T message) where T : struct, INetworkSerializable
        {
            if (!networkManager.EnsureServer()) return;

            using (FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp, 10240))
            {
                writer.WriteValueSafe(GetTypeHash<T>());
                writer.WriteValueSafe(message);
                messagingManager.SendUnnamedMessage(clientId, writer);
            }
        }
        public void SendToAllClient<T>(T message) where T : struct, INetworkSerializable
        {
            if (!networkManager.EnsureServer()) return;

            using (FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp, 10240))
            {
                writer.WriteValueSafe(GetTypeHash<T>());
                writer.WriteValueSafe(message);
                messagingManager.SendUnnamedMessageToAll(writer);
            }
        }
        #endregion

        #region 消息接收
        private void OnMessageReceived(ulong clientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out uint hash);

            if (handlerDic.TryGetValue(hash, out IMessageHandler handler))
            {
                handler.Handle(clientId, reader);
            }
        }
        #endregion

        #region 工具
        private uint GetTypeHash<T>()
        {
            return GetTypeHash(typeof(T).FullName);
        }
        private uint GetTypeHash(string typeName)
        {
            // FNV-1a 32位 hash
            uint hash = 2166136261u;
            for (int i = 0; i < typeName.Length; i++)
            {
                hash ^= (byte)typeName[i];
                hash *= 16777619u;
            }
            return hash;
        }
        #endregion
    }

    internal partial class NetworkMessageModule
    {
        private interface IMessageHandler
        {
            void Handle(ulong clientId, FastBufferReader reader);
        }

        private class MessageHandler<T> : IMessageHandler where T : struct, INetworkSerializable
        {
            public event Action<ulong, T> OnMessage;

            
            public void Handle(ulong clientId, FastBufferReader reader)
            {
                reader.ReadValueSafe(out T msg);
                OnMessage?.Invoke(clientId, msg);
            }
        }
    }
}
