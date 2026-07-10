using UnityEngine;

/// <summary>
/// 玩家死亡状态：禁用输入和移动。
/// TODO: 目前为最小实现，后续添加死亡动画和游戏结束 UI
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
        // 关闭武器判定
        player.DisableWeaponHitbox();
    }

    public void OnUpdate()
    {
        // 死亡状态不响应任何输入
    }

    public void OnExit()
    {
    }
}
