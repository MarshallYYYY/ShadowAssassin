using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人攻击状态：停止移动，随机播放攻击动画，在判定窗口（EnterHitTime ~ EnterFollowThroughTime）启用伤害盒，
/// 通过 OnTriggerEnter 检测 Player 并造成伤害，有冷却时间。
/// 攻击动画播完后回到追击状态。
/// </summary>
public class EnemyAttackState : IState
{
    private readonly EnemyController enemy;
    private bool isAttacking;
    private float attackTimer;
    private float attackDuration;
    private AttackAnimSO currentAttackAnimSO;
    private bool isHitboxActive;

    public EnemyAttackState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        isAttacking = false;
        attackTimer = 0f;
        currentAttackAnimSO = null;
        isHitboxActive = false;
    }

    public void OnUpdate()
    {
        Transform player = enemy.GetPlayerTransform();
        if (player == null)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }

        float distance = enemy.DistanceToPlayer();

        // 朝向 Player
        enemy.FaceTarget(player.position);

        // 正在播放攻击动画 → 在判定窗口内启用伤害盒，动画播完切回追击
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            if (currentAttackAnimSO != null)
            {
                float enterHitTime = currentAttackAnimSO.EnterHitTime;
                float enterFollowThroughTime = currentAttackAnimSO.EnterFollowThroughTime;

                // 判定窗口内 → 启用伤害盒
                if (isHitboxActive is false && enterHitTime <= attackTimer && attackTimer < enterFollowThroughTime)
                {
                    bool isPlayHitAnim = currentAttackAnimSO.Clip.name == EnemyAnimConstants.HorizontalAttack;
                    enemy.EnableHitbox(currentAttackAnimSO.Damage, isPlayHitAnim);
                    isHitboxActive = true;
                }
                // 进入收尾阶段 → 关闭伤害盒
                else if (isHitboxActive && attackTimer >= enterFollowThroughTime)
                {
                    enemy.DisableHitbox();
                    isHitboxActive = false;
                }
            }

            // 动画播完 → 切回追击
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
            List<AttackAnimSO> attackAnims = enemy.AttackAnims;
            if (attackAnims == null || attackAnims.Count == 0)
                return;

            // 随机选择攻击动画
            currentAttackAnimSO = attackAnims[Random.Range(0, attackAnims.Count)];
            enemy.PlayAnim(currentAttackAnimSO.Clip.name);
            isAttacking = true;
            attackTimer = 0f;
            attackDuration = currentAttackAnimSO.Clip.length;
            isHitboxActive = false;
            // AudioService.Instance.PlaySfx(AudioConstants.EnemyAttack);

            enemy.RecordAttack();
        }
    }

    public void OnExit()
    {
        isAttacking = false;
        enemy.DisableHitbox();
        currentAttackAnimSO = null;
        isHitboxActive = false;
    }
}
