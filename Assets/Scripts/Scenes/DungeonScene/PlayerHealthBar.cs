using DG.Tweening;
using UnityEngine;

/// <summary>
/// 玩家血条 UI：即时更新前景血条，延迟血条用 DOTween 平滑追赶，产生"慢慢减少"效果。
/// 使用 RectTransform 的 anchorMax.x 实现填充效果，不依赖 Sprite。
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    #region 外部赋值 SerializeField
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private RectTransform delayRect;
    #endregion

    #region 常量
    private const float DelayDuration = 0.5f;
    #endregion

    #region 数据
    private float maxHP;
    private Tween delayTween;
    #endregion

    #region 生命周期
    void Awake()
    {
        maxHP = PersistentService.Instance.GetPlayerMaxHP();
    }
    void OnDestroy()
    {
        delayTween?.Kill();
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 设置血量：前景即时更新，延迟血条平滑追赶
    /// </summary>
    public void SetHP(float current)
    {
        float ratio = Mathf.Clamp01(current / maxHP);

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
    }
    #endregion
}
