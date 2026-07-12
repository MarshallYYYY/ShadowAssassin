using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器伤害盒，挂在 Sword 物体上。
/// 攻击判定阶段启用 Collider，通过 OnTriggerEnter 检测 Enemy 并造成伤害。
/// 使用 HashSet 去重，同一敌人每次攻击只命中一次。
/// </summary>
[RequireComponent(typeof(MeshCollider))]

public class WeaponHitbox : MonoBehaviour
{
    #region 数据
    private float currentDamage;
    private bool isActive;
    private PlayerController playerController;
    #endregion

    #region 生命周期
    void Awake()
    {
        MeshCollider collider = GetComponent<MeshCollider>();
        collider.isTrigger = true;
        collider.enabled = false;
        // collider.convex = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;
        // if (!other.CompareTag(EnemyConstants.EnemyTag))
        if (other.gameObject.tag is not EnemyConstants.EnemyTag)
            return;

        EnemyController enemy = other.GetComponent<EnemyController>();
        enemy.TakeDamage(currentDamage);
    }
    #endregion

    #region 公共方法
    public void Init(PlayerController playerController)
    {
        this.playerController = playerController;
    }
    /// <summary>
    /// 启用伤害判定：开启 Collider、清空命中记录
    /// </summary>
    public void EnableHitbox(float damage)
    {
        currentDamage = damage;
        isActive = true;
        GetComponent<Collider>().enabled = true;
    }

    /// <summary>
    /// 关闭伤害判定：关闭 Collider
    /// </summary>
    public void DisableHitbox()
    {
        isActive = false;
        GetComponent<Collider>().enabled = false;
    }
    #endregion
}

