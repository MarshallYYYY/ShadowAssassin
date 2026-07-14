using UnityEngine;

/// <summary>
/// 玩家闪避状态：CrossFade 播放闪避动画并触发残影效果，动画播完后切回 LocomotionState。
/// </summary>
public class PlayerAvoidState : IState
{
    private readonly PlayerController player;

    public PlayerAvoidState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        player.Animator.CrossFadeInFixedTime(AnimatorConstants.AvoidState, AnimatorConstants.AvoidFadeDuration);
        player.CurrentAnimTotalTime = AnimatorConstants.AvoidAnimTotalTime;
        player.CurrentAnimTime = 0;

        player.AfterImage.StartEffect();
        AudioService.Instance.PlaySfx(AudioConstants.PlayerAvoid);
    }

    public void OnUpdate()
    {
        player.CurrentAnimTime += Time.deltaTime;

        if (player.CurrentAnimTime >= player.CurrentAnimTotalTime * AnimatorConstants.AvoidEarlyExitRatio)
        {
            player.StateMachine.ChangeState(player.LocomotionState);
        }
    }

    public void OnExit()
    {
        player.CurrentAnimTime = 0;
    }
}
