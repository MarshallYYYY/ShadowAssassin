// 这个脚本可以提取出来，挂载到YooAssetScene的其他物体上，
// 比如YooAssetManager，然后使用查找的方式获取UI，这样就可以将下载界面做成可热更的。
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YooAsset;

public class YooAssetSceneUIManager : MonoBehaviour
{
    // 这个属性告诉Unity：在加载任何场景之前，先执行这个方法
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // private static void LoadYooAssetScene()
    // {
    //     // 这里的 "YooAssetScene" 替换成你真正的首个场景名
    //     // 注意：这个场景必须在 Build Settings 的场景列表中
    //     SceneManager.LoadScene("YooAssetScene");
    // }
    private YooAssetManager yooAssetManager;
    [SerializeField] private Text titleText;
    private Tween titleTextTween;
    private void Awake()
    {
        yooAssetManager = GameObject.Find(nameof(YooAssetManager)).GetComponent<YooAssetManager>();
        titleTextTween = titleText.DOFade(0.3f, 2f).SetLoops(-1, LoopType.Yoyo);
        InitDownloadWindow();
        InitFailedWindow();
    }
    private void OnEnable()
    {
        yooAssetManager.MakeDownloadDecision += OnMakeDownloadDecision;
        yooAssetManager.DownloadProgressChanged += OnDownloadProgressChanged;
        yooAssetManager.Failed += OnFailed;
        yooAssetManager.Finished += OnFinished;
    }
    private void OnDisable()
    {
        yooAssetManager.MakeDownloadDecision -= OnMakeDownloadDecision;
        yooAssetManager.DownloadProgressChanged -= OnDownloadProgressChanged;
        yooAssetManager.Failed -= OnFailed;
        yooAssetManager.Finished -= OnFinished;
    }

    private void OnDestroy()
    {
        // if (titleText != null)
        // {
        //     DOTween.Kill(titleText);
        // }

        titleTextTween?.Kill();
    }

    #region YooAssetManage中的（监听）事件
    private void OnMakeDownloadDecision()
    {
        Debug.Log("玩家做决定");
        downloadWindow.SetActive(true);
    }

    private void OnDownloadProgressChanged(DownloadDataInfo info)
    {
        if (slider.gameObject.activeSelf is false)
        {
            slider.gameObject.SetActive(true);
        }
        // 2026-6-15 01:27:24：就很奇怪，如果Progress是在当前类由 Current/Total 计算得来，那么Slider的Handle就会不显示，
        // 测试了好几个方案，都不行。。。
        // 2026-6-15 01:35:58：经过与DeepSeek对话，发现是 NaN 的问题。
        // 仔细看了一下Console，发现progress在一开始确实出现了两次NaN，然后才变为0。
        float progress = float.Parse((info.CurrentDownloadSize / info.TotalDownloadSize).ToString("F2"));
        // 防止 NaN/Infinity
        if (float.IsNaN(progress) || float.IsInfinity(progress))
            progress = 0f;
        slider.value = progress;
        percentageProgressText.text = (progress * 100).ToString("F0") + "%";
        sizeProgressText.text = $"{info.CurrentDownloadSize:F2}MB / {info.TotalDownloadSize:F2}MB";
    }

    private void OnFailed(string errorMessage)
    {
        downloadWindow.SetActive(false);
        errorMessageText.text = errorMessage;
        failedWindow.SetActive(true);
    }

    private void OnFinished()
    {
        downloadWindow.SetActive(false);
        StartCoroutine(EnterLoadMetadataScene());
    }
    private IEnumerator EnterLoadMetadataScene()
    {
        Debug.Log("================== Enter LoadMetadataScene =================");
        ResourcePackage package = YooAssets.GetPackage(YooAssetConstants.PackageName);

        SceneHandle handle = package.LoadSceneAsync(YooAssetConstants.LoadMetadataLocation);
        yield return handle.Task;
        Debug.Log("场景名称：" + handle.SceneName);
    }
    #endregion
    #region Download Window
    [Header("DownloadWindow")]
    [SerializeField] private GameObject downloadWindow;
    #region DownloadConfirm
    [Header("DownloadConfirm")]
    [SerializeField] private GameObject downloadConfirm;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    #endregion
    #region Slider
    [Header("Slider")]
    [SerializeField] private Slider slider;
    [SerializeField] private Text percentageProgressText;
    [SerializeField] private Text sizeProgressText;
    #endregion
    private void InitDownloadWindow()
    {
        downloadWindow.SetActive(false);
        downloadConfirm.SetActive(true);
        slider.gameObject.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }
    private void OnConfirmButtonClicked()
    {
        //isPlayerClicked = true;
        //isClickedConfirm = true;
        yooAssetManager.ConfirmDownload();
        downloadConfirm.SetActive(false);
        slider.gameObject.SetActive(true);
    }
    private void OnCancelButtonClicked()
    {
        //isPlayerClicked = true;
        //isClickedConfirm = false;
        yooAssetManager.CancelDownload();
        downloadConfirm.SetActive(false);
        // 下面这个也可不加
        slider.gameObject.SetActive(false);
    }
    #endregion

    #region FailedWindow
    [Header("FailedWindow")]
    [SerializeField] private GameObject failedWindow;
    [SerializeField] private Text errorMessageText;
    [SerializeField] private Button quitGameButton;
    private void InitFailedWindow()
    {
        failedWindow.SetActive(false);

        quitGameButton.onClick.AddListener(OnQuitGameButtonClicked);
    }
    private void OnQuitGameButtonClicked()
    {
#if UNITY_EDITOR
        // 编辑器模式下：停止播放
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 打包后：退出应用程序
        Application.Quit();
#endif
    }
    #endregion
}
