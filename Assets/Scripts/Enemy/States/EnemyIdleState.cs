using UnityEngine;

/// <summary>
/// 敌人待机状态：原地停留 1~3 秒并播放 Idle 动画，检测到 Player 进入侦测范围则切换到追击，
/// 否则倒计时结束后切换到巡逻状态走向新目标点。
/// </summary>
public class EnemyIdleState : IState
{
    private readonly EnemyController enemy;
    private float idleTimer;
    private float idleDuration;

    public EnemyIdleState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        idleTimer = 0f;
        idleDuration = Random.Range(1f, 3f);
        enemy.PlayAnim(EnemyAnimConstants.Idle, 0.2f);
    }

    public void OnUpdate()
    {
        // 检测 Player 是否在侦测范围内
        if (enemy.DistanceToPlayer() <= enemy.DetectRange)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            enemy.StateMachine.ChangeState(enemy.PatrolState);
        }
    }

    public void OnExit()
    {
    }
}
