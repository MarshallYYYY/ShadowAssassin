using UnityEngine;

/// <summary>
/// 玩家移动状态：处理自由移动和锁定目标移动，检测攻击/翻滚/闪避/停止输入
/// </summary>
public class PlayerMoveState : IState
{
    private readonly PlayerController player;

    public PlayerMoveState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        // 无特殊操作
    }

    public void OnUpdate()
    {
        // 检测状态切换（优先级：攻击 > 翻滚 > 闪避 > 停止移动）
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
        if (player.MoveInput == Vector2.zero)
        {
            // 锁定时即使不移动也要持续朝向目标，不切回 IdleState
            if (player.IsLock)
            {
                LockedTargetMove();
                return;
            }
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }

        // 执行移动
        if (player.IsLock is false)
        {
            FreeMove();
        }
        else
        {
            LockedTargetMove();
        }
    }
    #region 自由移动
    /// <summary>
    /// SmoothDampAngle 的速度缓存，避免每帧重新计算
    /// </summary>
    private float rotationVelocity;
    private void FreeMove()
    {
        Vector2 move = player.MoveInput;
        if (move == Vector2.zero)
            return;

        // 将二维输入转换为三维方向，忽略 Y 轴，方便在地面上移动
        Vector3 inputDir = new Vector3(move.x, 0, move.y).normalized;

        // 输入方向→角度：Atan2(x, z) 返回该向量在 XZ 平面上相对于 +Z 轴的角度（弧度），* Mathf.Rad2Deg 转为角度。
        // + Camera.main.transform.eulerAngles.y — 叠加相机朝向：
        // 输入方向是相对于角色自身的，但玩家期望的是"按 W 朝屏幕里面走"。加上相机 Y 轴角度后，目标角度就变成了世界坐标系下的绝对角度。

        // 使用 Atan2 计算输入方向对应的角度，返回的是弧度，需要乘以 Rad2Deg 转为角度，
        // 再加上主相机当前的 Y 轴角度，让角色朝向相对摄像机的移动方向
        float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

        // 使用 SmoothDampAngle 让角色旋转过程更平滑，避免直接瞬间转向
        float rotation = Mathf.SmoothDampAngle(
            player.transform.eulerAngles.y, targetRotation, ref rotationVelocity, player.RotationSmoothTime);

        // 根据平滑后的角度设置角色朝向
        player.transform.rotation = Quaternion.Euler(0, rotation, 0);

        float axisY = player.Animator.GetFloat(AnimatorConstants.AxisY);
        axisY = Mathf.MoveTowards(axisY, move.magnitude, player.MoveSpeed * Time.deltaTime);
        player.Animator.SetFloat(AnimatorConstants.AxisY, axisY);

        player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * player.transform.forward);
        player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * Vector3.down);
    }
    #endregion

    #region 锁定目标移动
    /// <summary>
    /// 锁定目标时 SmoothDampAngle 的速度缓存
    /// </summary>
    private float lockRotationVelocity;
    private void LockedTargetMove()
    {
        if (player.LockTarget == null)
            return;

        Vector2 move = player.MoveInput;

        // 1. 始终朝向锁定目标
        Vector3 dirToTarget = player.LockTarget.position - player.transform.position;
        dirToTarget.y = 0f;

        if (dirToTarget.sqrMagnitude > 0.001f)
        {
            float targetRotation = Mathf.Atan2(dirToTarget.x, dirToTarget.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(
                player.transform.eulerAngles.y, targetRotation, ref lockRotationVelocity, player.RotationSmoothTime);
            player.transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        // 2. 读取 InputSystem 的移动输入，以角色自身朝向为参考做八方向移动
        Vector3 inputDir = new Vector3(move.x, 0f, move.y).normalized;

        if (inputDir.sqrMagnitude > 0.001f)
        {
            Vector3 moveDir = player.transform.forward * inputDir.z + player.transform.right * inputDir.x;
            moveDir = moveDir.normalized;

            player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * moveDir);
            player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * Vector3.down);
        }

        // 3. 更新动画参数
        float axisX = player.Animator.GetFloat(AnimatorConstants.AxisX);
        float axisY = player.Animator.GetFloat(AnimatorConstants.AxisY);

        axisX = Mathf.MoveTowards(axisX, move.x, Time.deltaTime * player.MoveSpeed);
        axisY = Mathf.MoveTowards(axisY, move.y, Time.deltaTime * player.MoveSpeed);

        player.Animator.SetFloat(AnimatorConstants.AxisX, axisX);
        player.Animator.SetFloat(AnimatorConstants.AxisY, axisY);
    }
    #endregion
    public void OnExit()
    {
        // 无特殊操作
    }

}
