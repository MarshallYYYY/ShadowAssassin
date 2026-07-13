using UnityEngine;

/// <summary>
/// 玩家翻滚状态：CrossFade 播放翻滚动画，动画播完后切回 LocomotionState。
/// </summary>
public class PlayerRollState : IState
{
    private readonly PlayerController player;

    public PlayerRollState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        player.Animator.CrossFadeInFixedTime(AnimatorConstants.RollState, AnimatorConstants.RollFadeDuration);
        player.CurrentAnimTotalTime = AnimatorConstants.RollAnimTotalTime;
        player.CurrentAnimTime = 0;
    }

    public void OnUpdate()
    {
        player.CurrentAnimTime += Time.deltaTime;

        if (player.CurrentAnimTime >= player.CurrentAnimTotalTime * AnimatorConstants.RollEarlyExitRatio)
        {
            player.StateMachine.ChangeState(player.LocomotionState);
        }
    }

    public void OnExit()
    {
        player.CurrentAnimTime = 0;
    }
}
