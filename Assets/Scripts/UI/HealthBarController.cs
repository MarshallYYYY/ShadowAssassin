using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用血条控制器：即时更新前景血条，延迟血条用 DOTween 平滑追赶，产生"慢慢减少"效果。
/// 同时提供闪烁功能，用于敌人死亡时的血条 UI 效果。
/// 使用 RectTransform 的 anchorMax.x 实现填充效果，不依赖 Sprite。
/// </summary>
public class HealthBarController : MonoBehaviour
{
    #region SerializeField
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private RectTransform delayRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float delayDuration = 0.5f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private int flashCount = 3;
    #endregion

    #region 数据
    private Tween delayTween;
    private Tween flashTween;
    #endregion

    #region 生命周期
    void OnDestroy()
    {
        delayTween?.Kill();
        flashTween?.Kill();
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 设置血量：前景即时更新，延迟血条平滑追赶
    /// </summary>
    public void SetHP(float current, float max)
    {
        float ratio = Mathf.Clamp01(current / max);

        // 前景即时更新：通过 anchorMax.x 控制填充宽度
        fillRect.anchorMax = new Vector2(ratio, fillRect.anchorMax.y);

        // 延迟血条平滑追赶
        delayTween?.Kill();
        delayTween = DOTween.To(
            () => delayRect.anchorMax.x,
            x => delayRect.anchorMax = new Vector2(x, delayRect.anchorMax.y),
            ratio,
            delayDuration
        );
    }

    /// <summary>
    /// 显示血条
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 隐藏血条
    /// </summary>
    public void Hide()
    {
        flashTween?.Kill();
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 闪烁血条（敌人死亡时调用），结束后隐藏
    /// </summary>
    public void Flash()
    {
        flashTween?.Kill();

        // 闪烁：alpha 在 0 和 1 之间循环 flashCount 次
        flashTween = DOTween.To(
            () => canvasGroup.alpha,
            x => canvasGroup.alpha = x,
            0f,
            flashInterval
        ).SetLoops(flashCount * 2, LoopType.Yoyo)
         .OnComplete(() => Hide());
    }
    #endregion
}
