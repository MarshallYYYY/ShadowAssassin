using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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
        //Color c = bgImage.color;
        //c.a = 0;
        //bgImage.color = c;
        bgImage.DOFade(0f, 0f);
        bgImage.raycastTarget = false;
        slider.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        DOTween.Kill(bgImage);
    }
    public void LoadScene(string sceneLocation)
    {
        //    var deps = AssetDatabase.GetDependencies(
        //"Assets/Scenes/VillageScene.unity",
        //true);

        //    foreach (var dep in deps)
        //    {
        //        Debug.Log(dep);
        //    }
        StartCoroutine(LoadSceneWithProgresss(sceneLocation));
    }
    private IEnumerator LoadSceneWithProgresss(string sceneLocation)
    {
        // 1. BgImage 设置为透明，且不让后面的UI被点击到，所以设置 bgImage.raycastTarget = true;，用来遮挡射线检测
        //Color c = bgImage.color;
        //c.a = 0;
        //bgImage.color = c;
        bgImage.DOFade(0f, 0f);
        bgImage.raycastTarget = true;

        // 2. BgImage 在一个持续时间内淡入（0 → 1），结束后显示 Slider
        float duration = SceneLoadConstants.SceneLoadBgImageFadeTime;
        // bgImage.DOColor(Color.black, duration);
        bgImage.DOFade(1f, duration);
        yield return new WaitForSeconds(duration);
        bgImage.DOColor(Color.white, 0f);
        // yield return null;
        slider.gameObject.SetActive(true);

        // 3. 开始异步加载场景
        var package = YooAssets.GetPackage(SceneLoadConstants.PackageName);
        SceneHandle handle = package.LoadSceneAsync(sceneLocation);
        // 进度条动态刷新
        float progress = 0;
        float threshold = 0.99f;
        while (progress < threshold)
        {
            SetProgress(progress);
            progress += Time.deltaTime * Random.Range(1, 3);
            yield return null;
        }
        // 真实进度
        /*
        while (handle.Progress < 1f)
        {
            Debug.LogWarning(handle.Progress);
            SetProgress(handle.Progress);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        Debug.LogWarning(handle.Progress);
        */

        // 如果虚假的进度条的所用时间少于真实进度所需时间，那么就让进度一直停留在99%（threshold）
        while (handle.Progress != 1)
        {
            SetProgress(threshold);
            yield return null;
        }

        // 略微等待，让玩家看到 100%
        SetProgress(1);
        yield return new WaitForSeconds(0.2f);

        yield return handle.Task;
        Debug.Log("场景名称：" + handle.SceneName);


        // 5. 关闭 Slider，BgImage 在一个持续时间内淡出（1 → 0）
        slider.gameObject.SetActive(false);
        bgImage.DOFade(0f, duration);
        yield return new WaitForSeconds(duration);
        bgImage.raycastTarget = false;
    }
    private void SetProgress(float progress)
    {
        slider.value = progress;
        progressText.text = (progress * 100).ToString("F0") + "%";
    }
}
