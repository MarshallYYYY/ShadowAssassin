using UnityEngine;

/// <summary>
/// 武器伤害盒，挂在 Sword 物体上。
/// 在攻击的判定阶段启用 Collider Trigger，检测碰到的 Enemy 并造成伤害。
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    private Collider hitboxCollider;
    private float currentDamage;

    /// <summary>
    /// 当前激活的 PlayerController，用于通知命中事件
    /// </summary>
    private PlayerController playerController;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
    }

    /// <summary>
    /// 设置 PlayerController 引用
    /// </summary>
    public void Initialize(PlayerController controller)
    {
        playerController = controller;
    }

    /// <summary>
    /// 启用伤害判定，设置当前伤害值
    /// </summary>
    public void EnableHitbox(float damage)
    {
        currentDamage = damage;
        hitboxCollider.enabled = true;
    }

    /// <summary>
    /// 关闭伤害判定
    /// </summary>
    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // 只检测 Enemy Layer
        if (other.gameObject.layer != LayerMask.NameToLayer(EnemyConstants.EnemyLayer))
            return;

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy == null)
            return;

        enemy.TakeDamage(currentDamage);
        playerController?.OnHitEnemy(enemy);
    }
}
