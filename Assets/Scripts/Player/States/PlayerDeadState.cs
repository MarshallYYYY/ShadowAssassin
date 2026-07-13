using UnityEngine;

/// <summary>
/// 玩家死亡状态：Play（硬切）播放死亡动画，加载村庄场景。
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
        SceneLoadService.Instance.LoadScene(SceneLoadConstants.VillageScene);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}
