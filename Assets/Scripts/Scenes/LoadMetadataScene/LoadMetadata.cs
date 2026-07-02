using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HybridCLR;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class LoadMetadata : MonoBehaviour
{
    [SerializeField] private Text titleText;
    // 持有引用
    private Tween titleTextTween;
    void Awake()
    {
        titleTextTween = titleText.DOFade(0.3f, 2f).SetLoops(-1, LoopType.Yoyo);
        StartCoroutine(Init());
    }
    private IEnumerator Init()
    {
        InitStartGameWindow();

        yield return CacheDlls();
        LoadMetadataForAOTAssemblies();

        // 等待补充元数据结束后，再启用开始游戏窗口：显示“按任意键开始游戏”和右下角的版本号
        EnableStartGameWindow();
    }
    private void Update()
    {
        if (startGameWindow.activeSelf)
        {
            if (Input.anyKeyDown)
            {
                StartGame();
            }
        }
    }
    private void OnDestroy()
    {
        /*
        // 如果不加此逻辑，从当前场景跳转到其他场景时，DOTween会警告⚠️。
        if (titleText != null)
        {
            // DOTween.Kill(titleText) 是通过目标对象去查找 tween，不如直接持有 tween 引用精确 kill：
            DOTween.Kill(titleText);
        }
        */

        // 精确 kill，不靠目标查找
        titleTextTween?.Kill();
    }
    #region 1. 缓存dll，判断是否已经加载 补充AOTdll
    /// <summary>
    /// 补充元数据dll的列表，
    /// 通过RuntimeApi.LoadMetadataForAOTAssembly()函数来补充AOT泛型的原始元数据
    /// </summary>
    private List<string> AOTMetaAssemblyFiles { get; } =
        new()
        {
            "mscorlib.dll", "System.dll", "System.Core.dll",
            "DOTween.dll", "Newtonsoft.Json.dll", "UnityEngine.CoreModule.dll",
            "YooAsset.dll", "Unity.InputSystem.dll", "Cinemachine.dll",
            // "UnityEngine.PhysicsModule.dll",
        };

    private readonly Dictionary<string, TextAsset> assetDict = new();
    private IEnumerator CacheDlls()
    {
        Debug.Log("================== 加载AOTdll，然后进行缓存 =================");
        ResourcePackage package = YooAssets.GetPackage(YooAssetConstants.PackageName);
        foreach (string asset in AOTMetaAssemblyFiles)
        {
            AssetHandle handle = package.LoadAssetAsync<TextAsset>(asset);
            yield return handle;
            TextAsset textAsset = handle.AssetObject as TextAsset;
            assetDict[asset] = textAsset;
            Debug.Log($"dll:{asset} 加载{(textAsset != null ? "成功" : "失败")}！");
        }
    }
    #endregion

    #region 2. 补充元数据
    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private void LoadMetadataForAOTAssemblies()
    {
        Debug.Log("================== 补充元数据 =================");
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

    #region StartGame Window
    [Header("StartGame")]
    [SerializeField] private GameObject startGameWindow;
    [SerializeField] private Text tipText;
    [SerializeField] private Text versionText;
    private void InitStartGameWindow()
    {
        startGameWindow.SetActive(false);
        tipText.gameObject.SetActive(true);
        versionText.gameObject.SetActive(true);
    }
    private void EnableStartGameWindow()
    {
        versionText.text = YooAssets.GetPackage(YooAssetConstants.PackageName).GetPackageVersion();
        startGameWindow.SetActive(true);
    }
    #region 3. 开始游戏
    private void StartGame()
    {
        Debug.Log("================== 开始游戏 =================");
        StartCoroutine(LoadScene());
    }
    private IEnumerator LoadScene()
    {
        var package = YooAssets.GetPackage(YooAssetConstants.PackageName);
        SceneHandle handle = package.LoadSceneAsync(SceneLoadConstants.StartScene);
        yield return handle.Task;
        Debug.Log("场景名称：" + handle.SceneName);
    }
    #endregion
    #endregion
}