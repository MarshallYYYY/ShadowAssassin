using UnityEngine;

/// <summary>
/// 敌人攻击状态：停止移动，对 Player 造成伤害，有冷却时间。
/// TODO: 目前用 Debug.Log 代替攻击动画，后续替换为实际动画播放
/// </summary>
public class EnemyAttackState : IState
{
    private readonly EnemyController enemy;

    public EnemyAttackState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        Transform player = enemy.GetPlayerTransform();
        if (player == null)
        {
            enemy.StateMachine.ChangeState(enemy.PatrolState);
            return;
        }

        float distance = enemy.DistanceToPlayer();

        // Player 离开攻击范围 → 追击
        if (distance > enemy.AttackRange)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // 朝向 Player
        enemy.FaceTarget(player.position);

        // 攻击冷却结束 → 执行攻击
        if (!enemy.IsInAttackCooldown)
        {
            // TODO: 替换为实际攻击动画
            Debug.Log($"[Enemy] 攻击 Player，造成 {enemy.AttackDamage} 点伤害");

            // 对 Player 造成伤害
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.TakeDamage(enemy.AttackDamage);

            enemy.RecordAttack();
        }
    }

    public void OnExit()
    {
    }
}