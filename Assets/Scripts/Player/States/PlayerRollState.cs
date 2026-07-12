using UnityEngine;

/// <summary>
/// 玩家翻滚状态：播放翻滚动画。
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
        // 不清除 AxisX/AxisY，保留方向信息让翻滚混合树播放对应方向的动画
        player.Animator.SetTrigger(AnimatorConstants.Roll);
        player.CurrentActionTotalTime = AnimatorConstants.RollAnimTotalTime;
        player.CurrentAnimTime = 0;
    }

    public void OnUpdate()
    {
        player.CurrentAnimTime += Time.deltaTime;

        if (player.CurrentAnimTime >= player.CurrentActionTotalTime)
        {
            player.StateMachine.ChangeState(player.LocomotionState);
        }
    }

    public void OnExit()
    {
        player.CurrentAnimTime = 0;
    }
}
