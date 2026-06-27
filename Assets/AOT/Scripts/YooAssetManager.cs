using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YooAsset;

/// <summary>
/// 资源更新状态
/// </summary>
public enum AssetsUpdateMode
{
    /* 一共有五种情况：
     * 网络良好：
     *     ①：编辑器模式
     *     ②：没有新资源，直接使用本地版本
     *     有新资源：
     *         ③：玩家选择下载
     *         ④：玩家取消下载，使用本地版本
     * 网络不好：
     *     ⑤：使用本地版本
     */
    /// <summary>
    /// ① 编辑器模式（直接使用本地资源，跳过更新检查）
    /// </summary>
    EditorMode,

    /// <summary>
    /// ② 无新资源，直接使用本地版本
    /// </summary>
    NoNewAssets,

    /// <summary>
    /// ③ 有新资源，玩家选择下载
    /// </summary>
    UserConfirmedDownload,

    /// <summary>
    /// ④ 有新资源，玩家取消下载，使用本地版本
    /// </summary>
    UserCanceledDownload,

    /// <summary>
    /// ⑤ 弱网络环境，网络不好，使用本地版本
    /// </summary>
    WeakNetworkingEnv
}
//[DefaultExecutionOrder(-100)]
public class YooAssetManager : MonoBehaviour
{
    #region 数据
    private Coroutine initCoroutine;
    /// <summary>
    /// 资源系统运行模式
    /// </summary>
    [SerializeField] private EPlayMode playMode = EPlayMode.HostPlayMode;
    private static string packageVersion;
    private static DownloadDataInfo downloadDataInfo;
    [SerializeField] private string gameVersionForTest = string.Empty;
    #endregion

    #region 监听事件
    public event Action<string> Failed;
    public event Action<string> Finished;
    #endregion
    void Start()
    {
        initCoroutine = StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        SetPlayMode();
        SetLocalGameVersion();

        InitYooAsset();
        yield return InitResourcePackage();
        yield return RequestPackageVersion();
        yield return UpdatePackageManifest();
        yield return DownloadHotUpdateAssets();

        yield return CacheDlls();
        LoadMetadataForAOTAssemblies();
        LoadHotUpdateDll();

        Finished?.Invoke(packageVersion);
    }
    private void OccurredError(string errorMessage)
    {
        Debug.LogError(errorMessage);
        StopCoroutine(initCoroutine);
        Failed?.Invoke(errorMessage);
    }
    #region 0. 设置 PlayMode 和 本地的GameVersion
    private void SetPlayMode()
    {
        //#if !UNITY_EDITOR
        //        playMode = EPlayMode.HostPlayMode;
        //#endif
        if (Application.isEditor is false)
        {
            playMode = EPlayMode.HostPlayMode;
        }
    }
    private void SetLocalGameVersion()
    {
        string localVersion = PlayerPrefs.GetString(YooAssetConstants.GameVersion);
        if (string.IsNullOrEmpty(localVersion))
        {
            PlayerPrefs.SetString(YooAssetConstants.GameVersion, YooAssetConstants.DefaultVersion);
            //PlayerPrefs.Save();
        }

        if (string.IsNullOrEmpty(gameVersionForTest) is false)
        {
            PlayerPrefs.SetString(YooAssetConstants.GameVersion, gameVersionForTest);
            //PlayerPrefs.Save();
        }
    }
    #endregion
    #region 1.初始化YooAsset
    private ResourcePackage package;
    private void InitYooAsset()
    {
        Debug.Log("================== 1.初始化YooAsset =================");
        YooAssets.Initialize();
        package =
            YooAssets.TryGetPackage(YooAssetConstants.PackageName) ?? YooAssets.CreatePackage(YooAssetConstants.PackageName);
        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);
    }
    #endregion
    #region 2. 初始化包：根据不同的运行模式，创建不同的初始化参数，调用package.InitializeAsync()函数进行初始化。
    private IEnumerator InitResourcePackage()
    {
        Debug.Log("================== 2.根据不同平台初始化资源包 =================");
        InitializationOperation initOperation;
        Debug.LogWarning($"当前PlayMode = {playMode}");
        switch (playMode)
        {
            case EPlayMode.EditorSimulateMode:
                initOperation = CreateOperationOnEditorSimulateMode(package);
                break;
            case EPlayMode.HostPlayMode:
                initOperation = CreateOperationOnHostPlayMode(package);
                break;
            default:
                OccurredError($"不支持的运行模式：{playMode}");
                yield break;
        }
        //yield return xxx = 等待 xxx 执行完成后，再继续往下走
        // 让当前协程暂停执行，直到 initOperation 这个异步操作完成。
        yield return initOperation;

        if (initOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("资源包初始化成功！");
        }
        else
        {
            OccurredError($"资源包初始化失败：{initOperation.Error}");
            yield break;
        }
    }
    private InitializationOperation CreateOperationOnEditorSimulateMode(ResourcePackage package)
    {
        PackageInvokeBuildResult buildResult = EditorSimulateModeHelper.SimulateBuild(YooAssetConstants.PackageName);
        string packageRoot = buildResult.PackageRootDirectory;
        FileSystemParameters fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

        EditorSimulateModeParameters createParameters = new()
        {
            EditorFileSystemParameters = fileSystemParams
        };

        return package.InitializeAsync(createParameters);
    }
    private InitializationOperation CreateOperationOnHostPlayMode(ResourcePackage package)
    {
        IRemoteServices remoteServices = new RemoteServices(
            YooAssetConstants.DefaultHostServerURL, YooAssetConstants.FallBackHostServerURL);

        // 创建内置文件系统的参数
        FileSystemParameters buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
        // 注意：设置参数COPY_BUILDIN_PACKAGE_MANIFEST，可以初始化的时候拷贝内置清单到沙盒目录
        buildinFileSystemParams.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST, true);

        // 创建缓存文件系统的参数
        FileSystemParameters cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
        // 注意：设置参数INSTALL_CLEAR_MODE，可以解决覆盖安装的时候将拷贝的内置清单文件清理的问题。
        cacheFileSystemParams.AddParameter(FileSystemParametersDefine.INSTALL_CLEAR_MODE, EOverwriteInstallClearMode.None);

        HostPlayModeParameters playModeParameters = new()
        {
            BuildinFileSystemParameters = buildinFileSystemParams,
            CacheFileSystemParameters = cacheFileSystemParams
        };
        return package.InitializeAsync(playModeParameters);
    }
    #endregion

    #region 3. 请求包版本
    private AssetsUpdateMode assetsUpdateMode;
    private IEnumerator RequestPackageVersion()
    {
        Debug.Log("================== 3.请求包版本 =================");
        // 先获取服务器中最新的资源版本
        RequestPackageVersionOperation operation = package.RequestPackageVersionAsync();
        yield return operation;

        string localVersion = PlayerPrefs.GetString(YooAssetConstants.GameVersion);
        string serverVersion = operation.PackageVersion;
        Debug.LogWarning($"本地包版本：{localVersion}");
        Debug.LogWarning($"服务器包版本：{serverVersion}");
        // 如果获取远端资源版本成功，说明当前网络连接通畅，可以走正常更新流程。
        if (operation.Status == EOperationStatus.Succeed)
        {
            // EditorSimulateMode 是个例外
            if (playMode is EPlayMode.EditorSimulateMode)
            {
                assetsUpdateMode = AssetsUpdateMode.EditorMode;
                packageVersion = operation.PackageVersion;
                Debug.Log($"使用 EditorSimulateMode，包版本：{packageVersion}");
                yield break;
            }

            // 本地版本号与服务器版本号一致（没有新资源需要下载，即本地版本已经是最新版本）
            if (localVersion == serverVersion)
            {
                assetsUpdateMode = AssetsUpdateMode.NoNewAssets;
                packageVersion = operation.PackageVersion;
                Debug.Log($"本地已经是服务器端版本 {packageVersion}，直接开始游戏！");
                yield break;
            }
            else
            {
                // 本地版本号 不等于 服务器版本号，表示有新资源
                // 玩家选择是否下载新资源
                yield return WaitingForDownloadDecision(serverVersion);
            }

        }
        // 弱联网环境（网络有问题）：使用本地版本
        else
        {
            Debug.Log($"当前处于 弱联网环境（网络有问题），正在尝试请求本地的包版本...");
            assetsUpdateMode = AssetsUpdateMode.WeakNetworkingEnv;
            TryUseLocalVersion(localVersion);
        }
    }
    /// <summary>
    /// 玩家决定是否下载
    /// </summary>
    public event Action MakeDownloadDecision;
    private bool? downloadDecision;
    public void ConfirmDownload()
    {
        downloadDecision = true;
    }

    public void CancelDownload()
    {
        downloadDecision = false;
    }
    private IEnumerator WaitingForDownloadDecision(string serverVersion)
    {
        Debug.Log("================== 等待玩家选择是否下载新资源 =================");
        MakeDownloadDecision?.Invoke();

        while (downloadDecision.HasValue is false)
        {
            yield return null;
        }
        // 玩家点击了 确定（下载）  按钮：使用服务器上的新版本
        if (downloadDecision.Value is true)
        {
            assetsUpdateMode = AssetsUpdateMode.UserConfirmedDownload;
            Debug.Log("玩家 确定开始下载");
            packageVersion = serverVersion;
        }
        // 玩家点击了 下次再说 按钮：使用本地版本
        else
        {
            assetsUpdateMode = AssetsUpdateMode.UserCanceledDownload;
            Debug.Log("玩家 取消本次下载，正在尝试请求本地的包版本...");
            string localVersion = PlayerPrefs.GetString(YooAssetConstants.GameVersion);
            TryUseLocalVersion(localVersion);
        }
    }
    /// <summary>
    /// 弱联网环境 或 玩家取消下载 的时候，尝试使用本地的包版本
    /// </summary>
    /// <returns></returns>
    private void TryUseLocalVersion(string localVersion)
    {
        if (string.IsNullOrEmpty(localVersion))
        {
            OccurredError($"请求本地的包版本失败！没有找到本地的包版本信息！");
        }
        else
        {
            Debug.Log($"请求本地的包版本成功！本地的包版本号：{localVersion}");
            packageVersion = localVersion;
        }
    }
    #endregion

    #region 4.更新包清单
    private IEnumerator UpdatePackageManifest()
    {
        Debug.Log("================== 4.更新包清单 =================");
        UpdatePackageManifestOperation operation = package.UpdatePackageManifestAsync(packageVersion);
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            OccurredError($"更新包清单失败，错误信息：{operation.Error}");
            yield break;
        }
    }
    #endregion

    #region 5.下载热更新资源
    private IEnumerator DownloadHotUpdateAssets()
    {
        Debug.Log("================== 5.下载热更新资源 =================");
        ResourceDownloaderOperation downloader =
            package.CreateResourceDownloader(YooAssetConstants.DownloadingMaxNum, YooAssetConstants.FailedTryAgain);

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log(assetsUpdateMode);
            Debug.Log("没有需要下载的资源！");
            yield break;
        }
        Debug.LogWarning($"需要下载的文件数量：{downloader.TotalDownloadCount}");
        // 如果是加载本地包 且 downloader.TotalDownloadCount > 0，那么就是有问题的
        if (assetsUpdateMode == AssetsUpdateMode.WeakNetworkingEnv ||
            assetsUpdateMode == AssetsUpdateMode.UserCanceledDownload)
        {
            OccurredError("本地资源内容不完整！");
            yield break;
        }
        //需要下载的文件总数
        int totalDownloadCount = downloader.TotalDownloadCount;
        //需要下载的文件总大小
        double totalDownloadMB = BytesToMB(downloader.TotalDownloadBytes);
        Debug.Log($"需要下载的资源文件总数：{totalDownloadCount}，总大小：{totalDownloadMB:F2} MB");
        downloadDataInfo.SetTotalData(totalDownloadMB, totalDownloadCount);
        downloadDataInfo.SetCurrentData(0, 0);

        //注册回调方法
        downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction; //当开始下载某个文件
        downloader.DownloadUpdateCallback = OnDownloadUpdateFunction; //当下载进度发生变化
        downloader.DownloadErrorCallback = OnDownloadErrorFunction; //当下载器发生错误
        downloader.DownloadFinishCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）

        //开始下载
        downloader.BeginDownload();
        yield return downloader;

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            Debug.Log($"热更新资源下载成功！" +
                $"下载文件总数：{totalDownloadCount}，" +
                $"下载文件总大小：{totalDownloadMB:F2} MB");

            // 注意：下载完成之后再保存本地版本
            PlayerPrefs.SetString(YooAssetConstants.GameVersion, packageVersion);
            yield break;
        }
        else
        {
            OccurredError($"热更新资源下载失败，错误信息：{downloader.Error}");
            yield break;
        }
    }
    #region Callback
    /// <summary>
    /// 开始下载
    /// </summary>
    private void OnDownloadFileBeginFunction(DownloadFileData downloadFileData)
    {
        double fileSizeMB = BytesToMB(downloadFileData.FileSize);
        // Debug.Log($"开始下载：文件名：{downloadFileData.FileName}，文件大小：{fileSizeMB:F2} MB");
    }

    public event Action<DownloadDataInfo> DownloadProgressChanged;
    /// <summary>
    /// 更新中
    /// </summary>
    private void OnDownloadUpdateFunction(DownloadUpdateData downloadUpdateData)
    {
        double totalMB = BytesToMB(downloadUpdateData.TotalDownloadBytes);
        double currentMB = BytesToMB(downloadUpdateData.CurrentDownloadBytes);
        // Debug.Log($"下载中......文件总数：{downloadUpdateData.TotalDownloadCount}，" +
        //     $"总大小：{totalMB:F2} MB，" +
        //     $"已下载文件数：{downloadUpdateData.CurrentDownloadCount}，" +
        //     $"已下载大小：{currentMB:F2} MB");
        downloadDataInfo.SetCurrentData(currentMB, downloadUpdateData.CurrentDownloadCount);
        DownloadProgressChanged?.Invoke(downloadDataInfo);
    }

    /// <summary>
    /// 下载出错
    /// </summary>
    /// <param name="errorData"></param>
    private void OnDownloadErrorFunction(DownloadErrorData errorData)
    {
        Debug.LogError($"下载出错，包名：{errorData.PackageName}，文件名：{errorData.FileName}，" +
            $"错误信息：{errorData.ErrorInfo}");
    }

    /// <summary>
    /// 下载完成
    /// </summary>
    private void OnDownloadFinishFunction(DownloaderFinishData downloaderFinishData)
    {
        Debug.Log($"下载{(downloaderFinishData.Succeed ? "成功" : "失败")}，包名：{downloaderFinishData.PackageName}");
    }
    private double BytesToMB(long bytes)
    {
        return bytes / 1024d / 1024d;
    }
    #endregion
    #endregion

    #region 6.缓存dll，判断是否已经加载 热更新dll 和 补充AOTdll
    /// <summary>
    /// 补充元数据dll的列表，
    /// 通过RuntimeApi.LoadMetadataForAOTAssembly()函数来补充AOT泛型的原始元数据
    /// </summary>
    private List<string> AOTMetaAssemblyFiles { get; } =
        new()
        {
            "mscorlib.dll", "System.dll", "System.Core.dll",
            "DOTween.dll", "Newtonsoft.Json.dll", "UnityEngine.CoreModule.dll",
            "YooAsset.dll",
            "Unity.InputSystem.dll",
        };

    private readonly Dictionary<string, TextAsset> assetDict = new();
    private IEnumerator CacheDlls()
    {
        Debug.Log("================== 6.加载 热更新dll 和 补充AOTdll，然后进行缓存 =================");
        var assets = new List<string> { YooAssetConstants.HotUpdateDllName }.Concat(AOTMetaAssemblyFiles);
        ResourcePackage package = YooAssets.GetPackage(YooAssetConstants.PackageName);
        foreach (string asset in assets)
        {
            AssetHandle handle = package.LoadAssetAsync<TextAsset>(asset);
            yield return handle;
            TextAsset textAsset = handle.AssetObject as TextAsset;
            assetDict[asset] = textAsset;
            Debug.Log($"dll:{asset} 加载{(textAsset != null ? "成功" : "失败")}！");
        }
    }
    #endregion

    #region HybridCLR相关的代码，补充AOTdll元数据和加载热更新dll
    #region 7.补充元数据
    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private void LoadMetadataForAOTAssemblies()
    {
        Debug.Log("================== 7.补充元数据 =================");
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (string aotDllName in AOTMetaAssemblyFiles)
        {
            byte[] dllBytes = GetDllBytes(aotDllName);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log(
                $"LoadMetadataForAOTAssembly: {aotDllName}, " +
                $"result: {errorCode}.");
        }
    }

    private byte[] GetDllBytes(string dllName)
    {
        if (assetDict.ContainsKey(dllName))
        {
            return assetDict[dllName].bytes;
        }

        return Array.Empty<byte>();
    }

    #endregion
    #region 8.加载热更新dll
    private void LoadHotUpdateDll()
    {
        Debug.Log("================== 8.加载热更新dll =================");
        byte[] dllBytes = GetDllBytes(YooAssetConstants.HotUpdateDllName);
        if (dllBytes != null && dllBytes.Length > 0)
        {
            // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
            Assembly hotUpdateAssembly = Assembly.Load(dllBytes);
#else
            // Editor下无需加载，直接查找获得HotUpdate程序集
            Assembly hotUpdateAssembly = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "HotUpdate");
#endif
            Debug.Log($"{YooAssetConstants.HotUpdateDllName} 加载成功，程序集名称为 {hotUpdateAssembly.FullName}");
        }
        else
        {
            Debug.LogError($"{YooAssetConstants.HotUpdateDllName} 加载失败！");
        }
    }
    #endregion

    #endregion


}
public struct DownloadDataInfo
{
    private double totalDownloadSize;
    private double currentDownloadSize;
    private int totalDownloadCount;
    private int currentDownloadCount;

    public double TotalDownloadSize { get => totalDownloadSize; }
    public double CurrentDownloadSize { get => currentDownloadSize; }
    public int TotalDownloadCount { get => totalDownloadCount; }
    public int CurrentDownloadCount { get => currentDownloadCount; }

    public void SetTotalData(double totalDownloadSize, int totalDownloadCount)
    {
        this.totalDownloadSize = totalDownloadSize;
        this.totalDownloadCount = totalDownloadCount;
    }
    public void SetCurrentData(double currentDownloadSize, int currentDownloadCount)
    {
        this.currentDownloadSize = currentDownloadSize;
        this.currentDownloadCount = currentDownloadCount;
    }
}