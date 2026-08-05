using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using EmptyFrame.Core;
using HybridCLR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EmptyFrame.HotUpdate
{
    internal class HotUpdateModule
    {
        private HotUpdateSystemSetting setting;
        private readonly HashSet<string> loadedDllNames = new HashSet<string>();

        private string persistentDataPath => $"{Application.persistentDataPath}/com.unity.addressables";
        private string catalogPath => $"{persistentDataPath}/catalog_0.1.json";

        public void Init(HotUpdateSystemSetting setting)
        {
            this.setting = setting;
            // Addressables 诊断系统在处理"即时完成"操作时有 NRE bug
            Addressables.ResourceManager.ClearDiagnosticCallbacks();
        }



        public async void Execute(Action onComplete, Action onFailure, Action<HotUpdateProgress> onProgress = null)
        {
            await ExecuteAsync(onComplete, onFailure, onProgress);
        }
        /// <summary>
        /// 执行热更新
        /// </summary>
        private async Task ExecuteAsync(Action onComplete, Action onFailure, Action<HotUpdateProgress> onProgress = null)
        {
            try
            {
                LogSystem.Debug($"开始热更新");

                // 下载资源
                await UpdateAssetsAsync(onProgress);
                // 加载AOT程序集元数据
                await LoadMetadataForAOTAssemblyAsync();
                // 加载热更程序集
                await LoadHotUpdateAssemblyAsync();
                // 重载目录
                await ReloadCatalogAsync();

                LogSystem.Debug($"热更新完成");
                onComplete?.Invoke();
            }
#if UNITY_EDITOR
            // 编辑器停止播放，静默退出
            catch (OperationCanceledException) { }
#endif
            catch
            {
                onFailure?.Invoke();
            }
        }

        /// <summary>
        /// 更新资源
        /// </summary>
        private async Task UpdateAssetsAsync(Action<HotUpdateProgress> onProgress)
        {
            List<string> catalogList = await CheckForCatalogUpdatesAsync();
            if (catalogList == null) return;

            List<object> resKeyList = await UpdateCatalogsAsync(catalogList);

            long totalDownloadSize = await GetDownloadSizeAsync(resKeyList);
            if (totalDownloadSize <= 0) return;

            await DownloadDependenciesAsync(resKeyList, totalDownloadSize, onProgress);
        }

        /// <summary>
        /// 检查目录更新
        /// </summary>
        private async Task<List<string>> CheckForCatalogUpdatesAsync()
        {
            LogSystem.Debug("开始检查目录更新");

            AsyncOperationHandle<List<string>> handle = Addressables.CheckForCatalogUpdates(false);

            try
            {
                await handle.Task;

                List<string> result = new List<string>(handle.Result);
                if (result.Count > 0)
                {
                    LogSystem.Debug($"检查目录更新成功，发现 {result.Count} 个可更新目录");
                    return result;
                }
                else
                {
                    LogSystem.Debug("检查目录更新成功: 无需更新");
                    return null;
                }
            }
            catch
            {
                throw ReportHandleException("检查目录更新失败", handle);
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 更新目录
        /// </summary>
        private async Task<List<object>> UpdateCatalogsAsync(List<string> catalogList)
        {
            LogSystem.Debug("开始更新目录");

            AsyncOperationHandle<List<IResourceLocator>> handle = Addressables.UpdateCatalogs(catalogList, false);

            try
            {
                await handle.Task;

                List<IResourceLocator> locators = handle.Result;
                List<object> result = new List<object>();
                for (int i = 0; i < locators.Count; i++)
                {
                    result.AddRange(locators[i].Keys);
                }
                LogSystem.Debug($"更新目录成功，共 {result.Count} 个资源条目");
                return result;
            }
            catch
            {
                throw ReportHandleException("更新目录失败", handle);
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 获取下载量
        /// </summary>
        private async Task<long> GetDownloadSizeAsync(IEnumerable<object> resKeyList)
        {
            LogSystem.Log("获取下载量");

            AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(resKeyList);

            try
            {
                await handle.Task;

                long totalDownloadSize = handle.Result;
                if (totalDownloadSize > 0)
                {
                    LogSystem.Debug($"获取下载量成功: 总计 {totalDownloadSize} bytes");
                }
                else
                {
                    LogSystem.Debug("获取下载量成功: 无需下载");
                }
                return totalDownloadSize;
            }
            catch
            {
                throw ReportHandleException("获取下载量失败", handle);
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 下载依赖资源
        /// </summary>
        private async Task DownloadDependenciesAsync(IEnumerable<object> resKeyList, long totalDownloadSize, Action<HotUpdateProgress> onProgress)
        {
            LogSystem.Debug("开始下载资源");

            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(resKeyList, Addressables.MergeMode.Union);

            try
            {
                while (!handle.IsDone)
                {
                    if (handle.Status == AsyncOperationStatus.Failed) break;

                    DownloadStatus status = handle.GetDownloadStatus();
                    long downloadedBytes = (long)(totalDownloadSize * status.Percent);
                    onProgress?.Invoke(new HotUpdateProgress
                    {
                        TotalBytes = totalDownloadSize,
                        DownloadedBytes = downloadedBytes,
                        Percent = status.Percent
                    });
                    LogSystem.Debug($"下载进度 : {downloadedBytes} / {totalDownloadSize} bytes");

                    await Task.Yield();
#if UNITY_EDITOR
                    // 停止播放模式时取消异步，防止句柄失效后继续访问
                    Application.exitCancellationToken.ThrowIfCancellationRequested();
#endif
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw ReportHandleException("下载资源失败", handle);
                }

                onProgress?.Invoke(new HotUpdateProgress
                {
                    TotalBytes = totalDownloadSize,
                    DownloadedBytes = totalDownloadSize,
                    Percent = 1
                });
                LogSystem.Debug("下载资源成功");
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }

        /// <summary>
        /// 加载AOT程序集元数据
        /// </summary>
        private async Task LoadMetadataForAOTAssemblyAsync()
        {
            LogSystem.Debug("开始加载AOT程序集元数据");

            for (int i = 0; i < setting.AotAssemblyNames.Count; i++)
            {
                string assemblyName = setting.AotAssemblyNames[i];
                AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>($"{assemblyName}.dll");

                try
                {
                    await handle.Task;

                    var err = RuntimeApi.LoadMetadataForAOTAssembly(handle.Result.bytes, HomologousImageMode.SuperSet);
                    LogSystem.Debug($"已加载AOT程序集元数据: {handle.Result.name}, ret:{err}");
                }
                catch
                {
                    throw ReportHandleException($"加载AOT程序集数据失败: {assemblyName}", handle);
                }
                finally
                {
                    if (handle.IsValid()) Addressables.Release(handle);
                }
            }

            LogSystem.Debug("加载AOT程序集元数据成功");
        }

        /// <summary>
        /// 加载热更程序集
        /// </summary>
        private async Task LoadHotUpdateAssemblyAsync()
        {
            LogSystem.Debug("开始加载热更程序集");

            for (int i = 0; i < setting.HotUpdateAssemblyNames.Count; i++)
            {
                string assemblyName = setting.HotUpdateAssemblyNames[i];
                AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>($"{assemblyName}.dll");

                try
                {
                    await handle.Task;

                    TextAsset textAsset = handle.Result;
                    if (!loadedDllNames.Contains(textAsset.name))
                    {
                        Assembly.Load(textAsset.bytes);
                        loadedDllNames.Add(textAsset.name);
                        LogSystem.Debug($"已加载热更程序集: {handle.Result.name}");
                    }
                }
                catch
                {
                    throw ReportHandleException($"加载热更程序集数据失败: {assemblyName}", handle);
                }
                finally
                {
                    if (handle.IsValid()) Addressables.Release(handle);
                }
            }

            LogSystem.Debug("加载热更程序集成功");
        }

        /// <summary>
        /// 重新加载 Addressables 目录。
        /// 热更新程序集加载后，需要重新加载目录，否则 Addressables 可能无法正确识别热更新程序集中的类型，并将其识别为 System.Object。
        /// </summary>
        private async Task ReloadCatalogAsync()
        {
            LogSystem.Debug("开始重新加载目录");

            AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(catalogPath);

            try
            {
                await handle.Task;
                LogSystem.Debug("重新加载目录成功");
            }
            catch
            {
                throw ReportHandleException("重新加载目录失败", handle);
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }

        private static Exception ReportHandleException(string prefix, AsyncOperationHandle handle)
        {
            var ex = handle.OperationException ?? new Exception("未知异常");
            LogSystem.Error($"{prefix}: {ex.Message}");
            return ex;
        }
    }
}
