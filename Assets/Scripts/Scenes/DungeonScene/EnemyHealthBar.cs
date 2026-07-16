using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人血条 UI：即时更新前景血条，延迟血条用 DOTween 平滑追赶，产生"慢慢减少"效果。
/// 通过静态字段追踪"最后被攻击的敌人"，实现仅显示最近攻击敌人的血条。
/// 死亡时闪烁血条后隐藏。
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    #region 静态追踪
    /// <summary>
    /// 当前正在显示血条的敌人（只显示最近被攻击的）
    /// </summary>
    private static EnemyHealthBar lastShownBar;
    #endregion

    #region SerializeField
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform delayRect;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private Text hpText;
    #endregion

    #region 常量
    private const float DelayDuration = 0.5f;
    private const float FlashInterval = 0.1f;
    private const int FlashCount = 3;
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
    /// 隐藏血条（对象池初始化和回收时调用）
    /// </summary>
    public void HideBar()
    {
        if (lastShownBar == this)
            lastShownBar = null;

        Hide();
    }
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
            DelayDuration
        );

        hpText.text = $"{current} / {max}";
    }

    /// <summary>
    /// 被攻击时调用：隐藏上一个受击敌人的血条，显示当前受击敌人的血条
    /// </summary>
    public void OnHit()
    {
        if (lastShownBar != null && lastShownBar != this)
            lastShownBar.Hide();

        lastShownBar = this;

        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 死亡时调用：闪烁后隐藏，清除静态引用
    /// </summary>
    public void OnDeath()
    {
        if (lastShownBar == this)
            lastShownBar = null;

        flashTween?.Kill();

        // 闪烁：alpha 在 0 和 1 之间循环 FlashCount 次
        canvasGroup.DOFade(0f, FlashInterval)
            .SetLoops(FlashCount * 2, LoopType.Yoyo)
            .OnComplete(() => Hide());
    }
    #endregion

    #region 内部方法
    private void Hide()
    {
        flashTween?.Kill();
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    #endregion
}
