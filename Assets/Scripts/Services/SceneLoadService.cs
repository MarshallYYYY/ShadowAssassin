using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class SceneLoadService : BaseService<SceneLoadService>
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Slider slider;
    [SerializeField] private Text progressText;

    public override void Init()
    {
        base.Init();
        bgImage.DOFade(0f, 0f);
        bgImage.raycastTarget = false;
        slider.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        DOTween.Kill(bgImage);
    }

    public void LoadScene(string sceneLocation, Action onSceneLoaded = null)
    {
        AudioService.Instance.StopBgm();
        StartCoroutine(LoadSceneWithProgresss(sceneLocation, onSceneLoaded));
    }

    private IEnumerator LoadSceneWithProgresss(string sceneLocation, Action onSceneLoaded = null)
    {
        // 1. BgImage 设置为透明，且不让后面的UI被点击到，所以设置 bgImage.raycastTarget = true;，用来遮挡射线检测
        bgImage.DOFade(0f, 0f);
        bgImage.raycastTarget = true;

        // 2. BgImage 在一个持续时间内淡入（0 → 1），结束后显示 Slider
        float duration = SceneLoadConstants.SceneLoadBgImageFadeTime;
        bgImage.DOFade(1f, duration);
        yield return new WaitForSeconds(duration);
        slider.gameObject.SetActive(true);

        // 3. 开始异步加载场景
        ResourcePackage package = YooAssets.GetPackage(YooAssetConstants.PackageName);
        SceneHandle handle = package.LoadSceneAsync(sceneLocation);

        // 进度条动态刷新：虚假进度 + 真实进度取较小值，避免虚假进度超过真实进度
        float progress = 0;
        float threshold = 0.99f;
        while (progress < threshold)
        {
            SetProgress(progress);
            progress += Time.deltaTime * UnityEngine.Random.Range(1, 3);
            // 虚假进度不超过真实进度，防止先到 99% 再干等
            progress = Mathf.Min(progress, handle.Progress * 0.99f);
            yield return null;
        }

        // 等待真实加载完成（浮点数安全比较）
        while (handle.Progress < 1f)
        {
            SetProgress(threshold);
            yield return null;
        }

        // 略微等待，让玩家看到 100%
        SetProgress(1);
        yield return new WaitForSeconds(0.2f);

        yield return handle.Task;
        Debug.Log("场景名称：" + handle.SceneName);
        onSceneLoaded?.Invoke();

        // 5. 关闭 Slider，BgImage 在一个持续时间内淡出（1 → 0）
        slider.gameObject.SetActive(false);
        // bgImage.DOFade(0f, duration);
        bgImage.DOFade(0f, 0f);
        // yield return new WaitForSeconds(duration);
        bgImage.raycastTarget = false;
    }
    private void SetProgress(float progress)
    {
        slider.value = progress;
        progressText.text = (progress * 100).ToString("F0") + "%";
    }
}
