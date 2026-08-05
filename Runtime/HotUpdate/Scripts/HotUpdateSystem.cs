using System;
using EmptyFrame.Core;
using UnityEngine;

namespace EmptyFrame.HotUpdate
{
    /// <summary>
    /// 热更新系统
    /// </summary>
    public static class HotUpdateSystem
    {
        private static HotUpdateModule module;

        public static void Init()
        {
            HotUpdateSystemSetting setting = Resources.Load<HotUpdateSystemSetting>("HotUpdateSystemSetting");
            if (setting == null)
            {
                LogSystem.Error("未找到热更新配置文件，请通过 CreateAssetMenu 创建 EmptyFrame.HotUpdateSystemSetting.asset 并放入 Resources 目录");
                return;
            }

            module = new HotUpdateModule();
            module.Init(setting);
        }

        /// <summary>
        /// 启动热更新流程
        /// </summary>
        /// <param name="onComplete">热更新成功回调</param>
        /// <param name="onProgress">下载进度回判</param>
        /// <param name="onFailure">热更新失败回调</param>
        public static void LaunchHotUpdate(Action onComplete, Action<HotUpdateProgress> onProgress = null, Action onFailure = null)
        {
            module.Execute(onComplete, onFailure, onProgress);
        }

      
    }
}
