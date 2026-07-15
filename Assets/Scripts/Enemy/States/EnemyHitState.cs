using UnityEngine;

/// <summary>
/// 敌人受击状态：播放受击动画，用计时器等动画播完后恢复。
/// </summary>
public class EnemyHitState : IState
{
    private readonly EnemyController enemy;
    private float hitTimer;
    private float hitDuration = 1.0f;

    public EnemyHitState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        // AudioService.Instance.PlaySfx(AudioConstants.EnemyHit);
        hitTimer = 0f;
        enemy.PlayAnim(EnemyAnimConstants.GetHit);
    }

    public void OnUpdate()
    {
        hitTimer += Time.deltaTime;

        if (hitTimer >= hitDuration)
        {
            // 受击动画播完：Player 在侦测范围内 → 追击，否则 → 待机
            if (enemy.DistanceToPlayer() <= enemy.DetectRange)
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
            else
            {
                enemy.StateMachine.ChangeState(enemy.IdleState);
            }
        }
    }

    public void OnExit()
    {
    }
}
