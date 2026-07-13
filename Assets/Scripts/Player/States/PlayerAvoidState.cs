using UnityEngine;

/// <summary>
/// 玩家闪避状态：播放闪避动画并触发残影效果。
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
        // 不清除 AxisX/AxisY，保留方向信息让闪避混合树播放对应方向的动画
        player.Animator.SetTrigger(AnimatorConstants.Avoid);
        player.CurrentAnimTotalTime = AnimatorConstants.AvoidAnimTotalTime;
        player.CurrentAnimTime = 0;

        player.AfterImage.StartEffect();
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
