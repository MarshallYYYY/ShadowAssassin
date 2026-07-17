using UnityEngine;

/// <summary>
/// 玩家地面移动状态：融合 Idle 和 Move，通过 2D 混合树实现 Idle→Walk→Run 的平滑过渡。
/// 有移动输入时执行 FreeMove 或 LockedTargetMove，无输入时动画参数平滑回到 0。
/// </summary>
public class PlayerLocomotionState : IState
{
    private readonly PlayerController player;

    #region SmoothDampAngle 速度缓存
    private float rotationVelocity;
    private float lockRotationVelocity;
    #endregion

    public PlayerLocomotionState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
        player.Animator.CrossFadeInFixedTime(AnimatorConstants.LocomotionState, AnimatorConstants.LocomotionFadeDuration);
    }

    public void OnUpdate()
    {
        // 1. 锁定目标死亡 → 自动取消锁定
        if (player.IsLock && player.LockTarget != null)
        {
            EnemyController enemy = player.LockTarget.GetComponent<EnemyController>();
            if (enemy != null && enemy.IsDead)
            {
                player.ClearLock();
            }
        }

        // 2. 检测状态切换（优先级：攻击 > 翻滚 > 闪避）
        if (player.AttackType != PlayerAttackType.None)
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

        // 3. 执行移动
        if (player.IsLock)
        {
            LockedTargetMove();
        }
        else
        {
            FreeMove();
        }
    }

    public void OnExit()
    {
    }

    #region 自由移动
    private void FreeMove()
    {
        Vector2 moveInput = player.MoveInput;

        // 自由移动不使用 AxisX，确保从锁定切换回来时归零
        UpdateAxis(AnimatorConstants.AxisX, 0);

        if (moveInput != Vector2.zero)
        {
            // 将二维输入转换为三维方向
            Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;

            // 平滑旋转到目标朝向（叠加相机 Y 轴角度）
            SmoothRotate(inputDir, ref rotationVelocity, Camera.main.transform.eulerAngles.y);

            // 动画参数平滑过渡到移动幅度
            UpdateAxis(AnimatorConstants.AxisY, moveInput.magnitude);

            // 移动 + 重力
            player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * (player.transform.forward + Vector3.down));
        }
        else
        {
            // 无输入：动画参数平滑回到 0（混合树自动过渡到 Idle）
            UpdateAxis(AnimatorConstants.AxisY, 0);
        }
    }
    #endregion

    #region 锁定目标移动
    private void LockedTargetMove()
    {
        if (player.LockTarget == null)
            return;

        Vector2 moveInput = player.MoveInput;

        // 1. 始终朝向锁定目标
        Vector3 dirToTarget = player.LockTarget.position - player.transform.position;
        dirToTarget.y = 0f;

        if (dirToTarget.sqrMagnitude > 0.001f)
        {
            SmoothRotate(dirToTarget, ref lockRotationVelocity);
        }

        // 2. 八方向移动
        if (moveInput != Vector2.zero)
        {
            Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            Vector3 moveDir = player.transform.forward * inputDir.z + player.transform.right * inputDir.x;
            moveDir = moveDir.normalized;

            player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * (moveDir + Vector3.down));
        }

        // 3. 动画参数：有输入时平滑到目标值，无输入时回到 0（混合树自动过渡到 Idle）
        UpdateAxis(AnimatorConstants.AxisX, moveInput.x);
        UpdateAxis(AnimatorConstants.AxisY, moveInput.y);
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 平滑旋转到指定方向（XZ 平面），可叠加额外角度（如相机 Y 轴角度）
    /// </summary>
    private void SmoothRotate(Vector3 direction, ref float velocity, float extraAngle = 0f)
    {
        float targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + extraAngle;
        float rotation = Mathf.SmoothDampAngle(
            player.transform.eulerAngles.y, targetRotation, ref velocity, Constants.RotationSmoothTime);
        player.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }
    /// <summary>
    /// 平滑更新指定动画参数
    /// </summary>
    private void UpdateAxis(string paramName, float target)
    {
        float value = player.Animator.GetFloat(paramName);
        value = Mathf.MoveTowards(value, target, player.MoveSpeed * Time.deltaTime);
        player.Animator.SetFloat(paramName, value);
    }
    #endregion
}