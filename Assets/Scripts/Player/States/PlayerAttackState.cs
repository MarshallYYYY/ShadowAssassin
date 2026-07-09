using UnityEngine;

/// <summary>
/// 玩家攻击状态：处理轻攻击连击链、重攻击派生、连击指示器 UI。
/// 核心规则：
/// 1. Idle/Move 时可按轻/重攻击自由起手
/// 2. 轻攻击进入连击链，仅在收尾阶段（EnterFollowThroughTime）可接下一击
/// 3. 重攻击打断一切，不可连击
/// 4. 动画播完 → 恢复 Idle
/// </summary>
public class PlayerAttackState : IState
{
    private readonly PlayerController player;

    public PlayerAttackState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        // 此时从 Idle/Move 进入，ComboIndex == 0，表示新攻击
        if (player.AttackType == AttackType.None)
            return;

        // 如果处于 Move 状态 且 点击了重攻击键，则重攻击播放的是 重攻击 - 跳跃斩击 动作
        if (player.Animator.GetFloat(AnimatorConstants.AxisY) >= 0.9f && player.AttackType == AttackType.Heavy)
        {
            player.PlayAttack(1, player.AttackType);
        }
        else
        {
            // 处于 Idle 状态：轻/重攻击都可以自由起手
            player.PlayAttack(0, player.AttackType);
        }

        player.AttackType = AttackType.None;
    }

    public void OnUpdate()
    {
        // 1. 更新动画计时
        player.CurrentAnimTime += Time.deltaTime;
        player.ComboSlider.value = player.CurrentAnimTime;

        // TODO: 攻击过程中只能被「受击」和「死亡」中断，这两个状态尚未实现

        // 2. 处理连击输入
        if (player.AttackType != AttackType.None
            && player.ComboIndex > 0
            && player.ComboIndex < player.AttackAnimDataBaseSO.LightAttackAnims.Count)
        {
            // 轻攻击连击（2-5） 和 重攻击 - 转身突刺：仅在前四段轻攻击连击的收尾阶段可触发
            int currentIndex = player.ComboIndex - 1;
            bool isCanAttack = player.CurrentAnimTime >= player.AttackAnimDataBaseSO.LightAttackAnims[currentIndex].EnterFollowThroughTime;
            if (isCanAttack)
            {
                // 轻攻击走连击索引；重攻击打第 3 击（派生技：转身突刺）
                int index = (player.AttackType == AttackType.Light) ? player.ComboIndex : 2;
                player.PlayAttack(index, player.AttackType);
            }
            player.AttackType = AttackType.None;
        }

        // 3. 动画播完 → 切回 Idle
        if (player.CurrentAnimTime >= player.CurrentActionTotalTime)
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public void OnExit()
    {
        // 清理攻击数据
        player.ComboIndex = 0;
        player.CurrentAnimTime = 0;
        player.ComboSlider.gameObject.SetActive(false);
        player.ClearSeparators();
    }
}
