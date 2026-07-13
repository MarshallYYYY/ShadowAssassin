using UnityEngine;

/// <summary>
/// 玩家受击状态：短暂硬直后恢复。
/// TODO: 目前为最小实现，后续添加受击动画和击退效果
/// </summary>
public class PlayerHitState : IState
{
    private readonly PlayerController player;

    public PlayerHitState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        player.Animator.SetTrigger(AnimatorConstants.Hit);
        player.CurrentAnimTotalTime = AnimatorConstants.HitAnimTotalTime;
        player.CurrentAnimTime = 0;
    }

    public void OnUpdate()
    {
        player.CurrentAnimTime += Time.deltaTime;

        if (player.CurrentAnimTime >= player.CurrentAnimTotalTime)
        {
            player.StateMachine.ChangeState(player.LocomotionState);
        }
    }

    public void OnExit()
    {
        player.CurrentAnimTime = 0;
    }
}