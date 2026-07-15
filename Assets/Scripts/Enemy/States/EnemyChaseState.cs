using UnityEngine;

/// <summary>
/// 敌人追击状态：向 Player 移动，进入攻击范围则切换到攻击，脱战则回到巡逻。
/// </summary>
public class EnemyChaseState : IState
{
    private readonly EnemyController enemy;

    public EnemyChaseState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        enemy.PlayAnim(EnemyAnimConstants.Run, 0.1f);
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

        // 进入攻击范围进行攻击
        if (distance <= enemy.AttackRange)
        {
            enemy.StateMachine.ChangeState(enemy.AttackState);
            return;
        }

        // 脱战：超出侦测范围 1.5 倍
        if (distance > enemy.DetectRange * 1.5f)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }

        // 朝 Player 移动
        Vector3 dir = player.position - enemy.transform.position;
        enemy.FaceTarget(player.position);
        enemy.MoveTo(dir, enemy.ChaseSpeed);
    }

    public void OnExit()
    {
    }
}
