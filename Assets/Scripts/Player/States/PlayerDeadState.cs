using UnityEngine;

/// <summary>
/// 玩家死亡状态：播放死亡动画，由 DungeonSceneUIManager 监听 OnPlayerDeath 事件显示死亡窗口。
/// </summary>
public class PlayerDeadState : IState
{
    private readonly PlayerController player;

    public PlayerDeadState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        player.Animator.Play(AnimatorConstants.DeadState);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}
