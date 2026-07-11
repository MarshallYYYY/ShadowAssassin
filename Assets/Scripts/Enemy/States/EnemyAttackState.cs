using UnityEngine;

/// <summary>
/// 敌人攻击状态：停止移动，随机播放 HorizontalAttack 或 DownwardAttack，对 Player 造成伤害，有冷却时间。
/// 攻击动画播完后回到追击状态。
/// </summary>
public class EnemyAttackState : IState
{
    private readonly EnemyController enemy;
    private bool isAttacking;
    private float attackTimer;
    private float attackDuration;

    public EnemyAttackState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        isAttacking = false;
        attackTimer = 0f;
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

        // 朝向 Player
        enemy.FaceTarget(player.position);

        // 正在播放攻击动画 → 用计时器等动画播完后切回追击
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDuration)
            {
                isAttacking = false;
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
            return;
        }

        // Player 离开攻击范围 → 追击
        if (distance > enemy.AttackRange)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // 攻击冷却结束 → 执行攻击
        if (!enemy.IsInAttackCooldown)
        {
            // 随机选择攻击动画
            string attackAnim = Random.value < 0.5f
                ? EnemyAnimConstants.HorizontalAttack
                : EnemyAnimConstants.DownwardAttack;
            enemy.PlayAnim(attackAnim);
            isAttacking = true;
            attackTimer = 0f;
            attackDuration = 1.5f; // 攻击动画时长（Approximate）

            // 对 Player 造成伤害
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController?.TakeDamage(enemy.AttackDamage);

            enemy.RecordAttack();
        }
    }

    public void OnExit()
    {
        isAttacking = false;
    }
}
