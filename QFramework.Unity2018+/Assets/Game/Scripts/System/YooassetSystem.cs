using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using YooAsset;

namespace Game
{
    public class YooassetSystem : AbstractSystem
    {
        private readonly string _packageName = "DefaultPackage";
        private EPlayMode _playMode = EPlayMode.EditorSimulateMode;

        protected override void OnInit()
        {
            ResKit.Init();
            // UIKit.Root.SetResolution(1920,1080,0);
            //将Yoo加载加入UI对象池
            UIKit.Config.PanelLoaderPool = new YooAssetsPanelLoaderPool();
            //讲Yoo加入对象池
            AudioKit.Config.AudioLoaderPool = new YooAssetsAudioLoaderPool();
            //将Yooasset加入到资源加载工厂
            ResFactory.AddResCreator<YooAssetsResCreator>();
            ResFactory.AddResCreator<YooAssetsSceneResCreator>();

            InitSync();
        }

        /// <summary>
        /// 同步初始化资源系统
        /// </summary>
        private void InitSync()
        {
            // 初始化资源系统
            YooAssets.Initialize();

            var gamePackage = YooAssets.TryGetPackage(_packageName) ?? YooAssets.CreatePackage(_packageName);

            YooAssets.SetDefaultPackage(gamePackage);
        }

        /// <summary>
        /// 异步初始化资源系统
        /// </summary>
        public async UniTask InitAsync()
        {
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(_packageName);

            LogKit.I($"YooAssets InitAsync with PlayMode : {_playMode.ToString()}");

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (_playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(_packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (_playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (_playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (_playMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                var createParameters = new WebPlayModeParameters();
			    string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
#else
                var createParameters = new WebPlayModeParameters();
                createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
#endif
            }

            await initializationOperation;

            // 如果初始化失败弹出提示界面
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"{initializationOperation.Error}");
            }
            else
            {
                await RequestPackageVersionAsync();
            }
        }

        /// <summary>
        /// 请求资源包版本
        /// </summary>
        private async UniTask RequestPackageVersionAsync()
        {
            var package = YooAssets.GetPackage(_packageName);
            var operation = package.RequestPackageVersionAsync();
            await operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
            }
            else
            {
                Debug.Log($"Request package version : {operation.PackageVersion}");
                await UpdateManifest(operation.PackageVersion);
            }
        }

        /// <summary>
        /// 更新资源包清单
        /// </summary>
        private async UniTask UpdateManifest(string packageVersion)
        {
            var package = YooAssets.GetPackage(_packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            await operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                return;
            }
            else
            {
                await CreateDownloader();
            }
        }

        /// <summary>
        /// 创建资源下载器并处理下载
        /// </summary>
        private async UniTask CreateDownloader()
        {
            var package = YooAssets.GetPackage(_packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("Not found any download files !");
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
            }

            downloader.DownloadErrorCallback = data => { Debug.Log($"Download error : {data.PackageName} {data.FileName} {data.ErrorInfo}"); };

            downloader.DownloadUpdateCallback = data => { Debug.Log($"Download update : {data.PackageName} {data.TotalDownloadCount} {data.TotalDownloadBytes} {data.CurrentDownloadCount} {data.CurrentDownloadBytes}"); };

            downloader.DownloadFinishCallback = data => { Debug.Log($"Download finish : {data.PackageName} {data.Succeed}"); };

            downloader.BeginDownload();

            await downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
            {
                return;
            }

            await ClearUnusedCacheFilesAsync();
        }

        /// <summary>
        /// 清理未使用的缓存文件
        /// </summary>
        private async UniTask ClearUnusedCacheFilesAsync()
        {
            var package = YooAssets.GetPackage(_packageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            await operation;
        }

        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string hostServerIP = "http://127.0.0.1";
            string appVersion = "v1.0";

#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
                return $"{hostServerIP}/CDN/WebGL/{appVersion}";
            else
                return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
            if (Application.platform == RuntimePlatform.Android)
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
                return $"{hostServerIP}/CDN/WebGL/{appVersion}";
            else
                return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }

            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }
    }
}