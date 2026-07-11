using UnityEngine;

/// <summary>
/// 敌人死亡状态：播放死亡动画，禁用碰撞，延迟后由 Spawner 回收。
/// </summary>
public class EnemyDeadState : IState
{
    private readonly EnemyController enemy;
    private float deadTimer;

    /// <summary>
    /// 延迟回收的时间
    /// </summary>
    private const float DespawnDelayTime = 2f;

    public EnemyDeadState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        deadTimer = 0f;

        // 播放死亡动画
        enemy.PlayAnim(EnemyAnimConstants.Dead);

        // 禁用碰撞，防止死后继续被检测
        CharacterController characterController = enemy.CharacterController;
        if (characterController != null)
            characterController.enabled = false;

        CapsuleCollider capsuleCollider = enemy.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
            capsuleCollider.enabled = false;
    }

    public void OnUpdate()
    {
        deadTimer += Time.deltaTime;

        if (deadTimer >= DespawnDelayTime)
        {
            // 通知 Spawner 回收
            EnemySpawner spawner = Object.FindObjectOfType<EnemySpawner>();
            spawner.Despawn(enemy);
        }
    }

    public void OnExit()
    {
    }
}
