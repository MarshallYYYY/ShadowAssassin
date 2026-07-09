using UnityEngine;

/// <summary>
/// 玩家待机状态：平滑过渡动画参数到 0，检测输入切换到其他状态
/// </summary>
public class PlayerIdleState : IState
{
    private readonly PlayerController player;

    public PlayerIdleState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        // 无特殊操作
    }

    public void OnUpdate()
    {
        // 锁定时始终朝向目标（攻击/翻滚等动作结束后回到 Idle 也保持朝向）
        if (player.IsLock && player.LockTarget != null)
        {
            FaceLockTarget();
        }

        // 平滑过渡到 Idle 动画
        float axisY = player.Animator.GetFloat(AnimatorConstants.AxisY);
        axisY = Mathf.MoveTowards(axisY, 0, player.MoveSpeed * Time.deltaTime);
        player.Animator.SetFloat(AnimatorConstants.AxisY, axisY);

        float axisX = player.Animator.GetFloat(AnimatorConstants.AxisX);
        axisX = Mathf.MoveTowards(axisX, 0, player.MoveSpeed * Time.deltaTime);
        player.Animator.SetFloat(AnimatorConstants.AxisX, axisX);

        // 检测状态切换（优先级：攻击 > 翻滚 > 闪避 > 移动）
        if (player.AttackType != AttackType.None)
        {
            player.StateMachine.ChangeState(player.AttackState);
            return;
        }
        if (player.IsRollPressed)
        {
            player.IsRollPressed = false;
            player.StateMachine.ChangeState(player.RollState);
            return;
        }
        if (player.IsAvoidPressed)
        {
            player.IsAvoidPressed = false;
            player.StateMachine.ChangeState(player.AvoidState);
            return;
        }
        if (player.MoveInput != Vector2.zero)
        {
            player.StateMachine.ChangeState(player.MoveState);
            return;
        }
    }

    public void OnExit()
    {
        // 无特殊操作
    }

    #region 锁定朝向

    /// <summary>
    /// 锁定时 SmoothDampAngle 的速度缓存
    /// </summary>
    private float lockRotationVelocity;
    private void FaceLockTarget()
    {
        Vector3 dirToTarget = player.LockTarget.position - player.transform.position;
        dirToTarget.y = 0f;

        if (dirToTarget.sqrMagnitude > 0.001f)
        {
            float targetRotation = Mathf.Atan2(dirToTarget.x, dirToTarget.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(
                player.transform.eulerAngles.y, targetRotation, ref lockRotationVelocity, player.RotationSmoothTime);
            player.transform.rotation = Quaternion.Euler(0, rotation, 0);
        }
    }
    #endregion
}
