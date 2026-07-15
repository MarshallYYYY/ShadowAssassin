using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家攻击状态：处理轻攻击连击链、重攻击派生、连击指示器 UI、武器伤害判定。
/// 核心规则：
/// 1. Idle/Move 时可按轻/重攻击自由起手
/// 2. 轻攻击进入连击链，仅在收尾阶段（EnterFollowThroughTime）可接下一击
/// 3. 重攻击打断一切，不可连击
/// 4. 判定阶段（EnterHitTime ~ EnterFollowThroughTime）启用武器 Collider
/// 5. 动画播完 → 恢复 Idle
/// </summary>
public class PlayerAttackState : IState
{
    private readonly PlayerController player;
    private bool isHitboxActive;

    public PlayerAttackState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        isHitboxActive = false;

        if (player.AttackType == PlayerAttackType.None)
            return;

        // 如果处于 往前奔跑的状态 且 点击了重攻击键，则重攻击播放的是 重攻击 - 跳跃斩击 动作
        if (player.Animator.GetFloat(AnimatorConstants.AxisY) >= 0.9f && player.AttackType == PlayerAttackType.Heavy)
        {
            player.PlayAttack(1, player.AttackType);
        }
        else
        {
            // 处于 Idle 状态：轻/重攻击都可以自由起手
            player.PlayAttack(0, player.AttackType);
        }

        player.AttackType = PlayerAttackType.None;
    }

    public void OnUpdate()
    {
        // 1. 更新动画计时
        player.CurrentAnimTime += Time.deltaTime;
        player.ComboSlider.value = player.CurrentAnimTime;

        // 2. 武器伤害判定窗口：EnterHitTime --- EnterFollowThroughTime
        AttackAnimSO currentAttackAnimSO = player.CurrentAttackAnimSO;
        if (currentAttackAnimSO != null)
        {
            float enterHitTime = currentAttackAnimSO.EnterHitTime;
            float enterFollowThroughTime = currentAttackAnimSO.EnterFollowThroughTime;

            // 如果伤害盒子未激活并且攻击动画处于伤害窗口
            if (!isHitboxActive && enterHitTime <= player.CurrentAnimTime && player.CurrentAnimTime < enterFollowThroughTime)
            {
                player.EnableWeaponHitbox();
                isHitboxActive = true;
            }
            // 如果伤害盒子已激活并且攻击动画处于收尾阶段
            else if (isHitboxActive && player.CurrentAnimTime >= enterFollowThroughTime)
            {
                player.DisableWeaponHitbox();
                isHitboxActive = false;
            }
        }

        // 3. 处理连击输入
        List<AttackAnimSO> lightAttackAnims = player.AttackAnimDataBaseSO.LightAttackAnims;
        if (player.AttackType != PlayerAttackType.None
            && player.ComboIndex > 0
            && player.ComboIndex < lightAttackAnims.Count)
        {
            // 轻攻击连击（2-5） 和 重攻击 - 转身突刺：仅在前四段轻攻击连击的收尾阶段可触发
            // 当前正在播放的攻击动画的索引
            int currentAttackAnimIndex = player.ComboIndex - 1;
            bool isCanAttack = player.CurrentAnimTime >= lightAttackAnims[currentAttackAnimIndex].EnterFollowThroughTime;
            if (isCanAttack)
            {
                // 这个判定是多余的，删去
                // 切换连击前先关闭武器判定
                // if (isHitboxActive)
                // {
                //     player.DisableWeaponHitbox();
                //     isHitboxActive = false;
                // }

                // 轻攻击走连击索引；重攻击打第 3 击（派生技：转身突刺）
                // 下一个要播放的攻击动画的索引
                int nextAttackAnimIndex = (player.AttackType == PlayerAttackType.Light) ? player.ComboIndex : 2;
                player.PlayAttack(nextAttackAnimIndex, player.AttackType);
            }
            player.AttackType = PlayerAttackType.None;
        }

        // 4. 动画播完 → 切回 LocomotionState（LocomotionState.OnEnter 会自动 CrossFade）
        if (player.CurrentAnimTime >= player.CurrentAnimTotalTime)
        {
            // 重攻击的 attackType 不在连击逻辑中消费，需要在此清零，
            // 防止切回 LocomotionState 后残留的 attackType 自动触发新攻击
            player.AttackType = PlayerAttackType.None;
            player.StateMachine.ChangeState(player.LocomotionState);
        }
    }

    public void OnExit()
    {
        // 确保武器判定已关闭
        player.DisableWeaponHitbox();

        // 清理攻击数据
        player.ComboIndex = 0;
        player.CurrentAnimTime = 0;
        player.IsSuperArmor = false;

        player.ComboSlider.gameObject.SetActive(false);
        player.ClearSeparators();
    }
}
