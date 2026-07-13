using UnityEngine;

/// <summary>
/// 玩家受击状态：Play（硬切）播放受击动画，动画播完后切回 LocomotionState。
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
        player.Animator.Play(AnimatorConstants.HitState);
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
