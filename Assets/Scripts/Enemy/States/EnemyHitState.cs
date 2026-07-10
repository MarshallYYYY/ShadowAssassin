using UnityEngine;

/// <summary>
/// 敌人受击状态：短暂硬直后恢复，若 Player 在侦测范围内则追击，否则巡逻。
/// </summary>
public class EnemyHitState : IState
{
    private readonly EnemyController enemy;
    private float hitTimer;
    private float hitDuration = 0.3f;

    public EnemyHitState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        hitTimer = 0f;
        // TODO: 播放受击动画/闪烁效果
    }

    public void OnUpdate()
    {
        hitTimer += Time.deltaTime;

        if (hitTimer >= hitDuration)
        {
            // 硬直结束后：Player 在侦测范围内 → 追击，否则 → 巡逻
            if (enemy.DistanceToPlayer() <= enemy.DetectRange)
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
            else
            {
                enemy.StateMachine.ChangeState(enemy.PatrolState);
            }
        }
    }

    public void OnExit()
    {
    }
}
