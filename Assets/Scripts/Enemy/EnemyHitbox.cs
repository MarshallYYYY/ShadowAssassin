using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人伤害盒，挂在 GoblinHitbox 物体上。
/// 攻击判定阶段启用 Collider，通过 OnTriggerEnter 检测 Player 并造成伤害。
/// 使用 HashSet 去重，同一敌人每次攻击只命中一次。
/// </summary>
[RequireComponent(typeof(MeshCollider))]
public class EnemyHitbox : MonoBehaviour
{
    #region 数据
    private float currentDamage;
    private bool currentShouldPlayHitAnim;
    private bool isActive;
    private readonly HashSet<PlayerController> hitTargets = new();
    #endregion

    #region 生命周期
    void Awake()
    {
        MeshCollider collider = GetComponent<MeshCollider>();
        collider.isTrigger = true;
        collider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;

        if (other.CompareTag(Constants.PlayerTag) == false)
            return;

        if (!other.TryGetComponent<PlayerController>(out var playerController))
            return;

        if (hitTargets.Contains(playerController))
            return;

        hitTargets.Add(playerController);
        float damage = currentDamage;
        if (Random.value < PersistentService.Instance.DodgeRate / 100f)
        {
            damage = 0f;
            Debug.Log("闪避");
        }
        playerController.TakeDamage(damage, currentShouldPlayHitAnim);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 启用伤害判定：开启 MeshCollider、清空命中记录
    /// </summary>
    public void EnableHitbox(float damage, bool isPlayHitAnim)
    {
        currentDamage = damage;
        currentShouldPlayHitAnim = isPlayHitAnim;
        isActive = true;
        hitTargets.Clear();
        GetComponent<MeshCollider>().enabled = true;
    }

    /// <summary>
    /// 关闭伤害判定：关闭 MeshCollider
    /// </summary>
    public void DisableHitbox()
    {
        isActive = false;
        GetComponent<MeshCollider>().enabled = false;
    }
    #endregion
}
