using UnityEngine;

/// <summary>
/// 敌人巡逻状态：在生成点附近随机游走，检测到 Player 进入侦测范围后切换到追击。
/// </summary>
public class EnemyPatrolState : IState
{
    private readonly EnemyController enemy;
    private Vector3 patrolTarget;
    private float waitTimer;
    private bool isWaiting;

    public EnemyPatrolState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        PickNewPatrolTarget();
        isWaiting = false;
        waitTimer = 0f;
        enemy.PlayAnim(EnemyAnimConstants.Walk);
    }

    public void OnUpdate()
    {
        // 检测 Player 是否在侦测范围内
        if (enemy.DistanceToPlayer() <= enemy.DetectRange)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= enemy.PatrolWaitTime)
            {
                PickNewPatrolTarget();
                isWaiting = false;
                enemy.PlayAnim(EnemyAnimConstants.Walk);
            }
        }
        else
        {
            // 朝巡逻目标移动
            Vector3 dir = patrolTarget - enemy.transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.5f)
            {
                // 到达目标点，等待一段时间
                isWaiting = true;
                waitTimer = 0f;
            }
            else
            {
                enemy.FaceTarget(patrolTarget);
                enemy.MoveTo(dir, enemy.MoveSpeed);
            }
        }
    }

    public void OnExit()
    {
    }

    /// <summary>
    /// 在生成点附近的圆内随机取一个点
    /// </summary>
    private void PickNewPatrolTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * enemy.PatrolRadius;
        patrolTarget = enemy.SpawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
    }
}
