using UnityEngine;

/// <summary>
/// 玩家闪避状态：播放闪避动画并触发残影效果。
/// TODO: 临时方案——用 Invoke 硬编码 1 秒后关闭残影。
///       后续应改为检测 Avoid 动画退出时自动调 StopEffect()。
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
        player.SetAnimatorBeforeAction();
        player.Animator.SetTrigger(AnimatorConstants.Avoid);
        player.CurrentActionTotalTime = AnimatorConstants.AvoidAnimTotalTime;
        player.CurrentAnimTime = 0;
        player.AfterImage.StartEffect();
        // 临时：1 秒后关闭残影
        player.Invoke(nameof(PlayerController.StopAfterImage), 1f);
    }

    public void OnUpdate()
    {
        player.CurrentAnimTime += Time.deltaTime;

        // currentActionTotalTime == 0，下一帧即满足条件，切回 Idle
        // 闪避动画依赖 Animator 的 applyRootMotion 继续播放
        if (player.CurrentAnimTime >= player.CurrentActionTotalTime)
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public void OnExit()
    {
        player.CurrentAnimTime = 0;
    }
}
