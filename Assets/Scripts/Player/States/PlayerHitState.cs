using UnityEngine;

/// <summary>
/// 玩家受击状态：短暂硬直后恢复。
/// TODO: 目前为最小实现，后续添加受击动画和击退效果
/// </summary>
public class PlayerHitState : IState
{
    private readonly PlayerController player;
    private float hitTimer;
    // TODO：换成受击动画的时长
    private float hitDuration = 0.3f;

    public PlayerHitState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        hitTimer = 0f;
    }

    public void OnUpdate()
    {
        hitTimer += Time.deltaTime;

        if (hitTimer >= hitDuration)
        {
            player.StateMachine.ChangeState(player.LocomotionState);
        }
    }

    public void OnExit()
    {
    }
}
