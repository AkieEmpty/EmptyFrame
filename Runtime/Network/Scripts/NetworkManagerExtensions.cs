using EmptyFrame.Core;
using Unity.Netcode;

namespace EmptyFrame.Network
{
    internal static class NetworkManagerExtensions
    {
        /// <summary>
        /// 确保当前为服务端
        /// </summary>
        public static bool EnsureServer(this NetworkManager networkManager)
        {
            if (!networkManager.IsServer)
            {
                LogSystem.Warning("客户端无权调用此服务端专用方法");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 确保当前为客户端
        /// </summary>
        public static bool EnsureClient(this NetworkManager networkManager)
        {
            if (!networkManager.IsClient)
            {
                LogSystem.Warning("服务端无权调用此客户端专用方法");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 确保网络处于空闲状态
        /// </summary>
        public static bool EnsureIdle(this NetworkManager networkManager)
        {
            if (networkManager.IsListening)
            {
                LogSystem.Warning("当前网络已启动");
                return false;
            }
            return true;
        }
    }
}
