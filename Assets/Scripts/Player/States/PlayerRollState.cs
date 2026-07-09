using UnityEngine;

/// <summary>
/// 玩家翻滚状态：播放翻滚动画。
/// TODO: 目前未设置 currentActionTotalTime，Roll 结束后依赖 Animator 的 Exit Time 兜底，后续需补上 Roll clip 时长。
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
        player.SetAnimatorBeforeAction();
        player.Animator.SetTrigger(AnimatorConstants.Roll);
        player.CurrentActionTotalTime = 0;
        player.CurrentAnimTime = 0;
    }

    public void OnUpdate()
    {
        player.CurrentAnimTime += Time.deltaTime;

        // currentActionTotalTime == 0，下一帧即满足条件，切回 Idle
        // 翻滚动画依赖 Animator 的 applyRootMotion 继续播放
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
