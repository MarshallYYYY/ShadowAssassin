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
    }

    public void OnUpdate()
    {
        // 1. 锁定目标死亡 → 自动取消锁定
        if (player.IsLock && player.LockTarget != null)
        {
            var enemy = player.LockTarget.GetComponent<EnemyController>();
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
        Vector2 move = player.MoveInput;

        if (move != Vector2.zero)
        {
            // 将二维输入转换为三维方向
            Vector3 inputDir = new Vector3(move.x, 0, move.y).normalized;

            // Atan2 计算输入方向角度 + 相机 Y 轴角度 = 世界坐标系下的目标朝向
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

            // SmoothDampAngle 平滑旋转
            float rotation = Mathf.SmoothDampAngle(
                player.transform.eulerAngles.y, targetRotation, ref rotationVelocity, Constants.PlayerRotationSmoothTime);
            player.transform.rotation = Quaternion.Euler(0, rotation, 0);

            // 动画参数平滑过渡到移动幅度
            float axisY = player.Animator.GetFloat(AnimatorConstants.AxisY);
            axisY = Mathf.MoveTowards(axisY, move.magnitude, player.MoveSpeed * Time.deltaTime);
            player.Animator.SetFloat(AnimatorConstants.AxisY, axisY);

            // 移动 + 重力
            player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * player.transform.forward);
            player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * Vector3.down);
        }
        else
        {
            // 无输入：动画参数平滑回到 0（混合树自动过渡到 Idle）
            float axisY = player.Animator.GetFloat(AnimatorConstants.AxisY);
            axisY = Mathf.MoveTowards(axisY, 0, player.MoveSpeed * Time.deltaTime);
            player.Animator.SetFloat(AnimatorConstants.AxisY, axisY);
        }
    }
    #endregion

    #region 锁定目标移动
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
                player.transform.eulerAngles.y, targetRotation, ref lockRotationVelocity, Constants.PlayerRotationSmoothTime);
            player.transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        // 2. 八方向移动
        if (move != Vector2.zero)
        {
            Vector3 inputDir = new Vector3(move.x, 0f, move.y).normalized;
            Vector3 moveDir = player.transform.forward * inputDir.z + player.transform.right * inputDir.x;
            moveDir = moveDir.normalized;

            player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * moveDir);
            player.CharacterController.Move(player.MoveSpeed * Time.deltaTime * Vector3.down);
        }

        // 3. 动画参数：有输入时平滑到目标值，无输入时回到 0（混合树自动过渡到 Idle）
        float axisX = player.Animator.GetFloat(AnimatorConstants.AxisX);
        float axisY = player.Animator.GetFloat(AnimatorConstants.AxisY);

        axisX = Mathf.MoveTowards(axisX, move.x, Time.deltaTime * player.MoveSpeed);
        axisY = Mathf.MoveTowards(axisY, move.y, Time.deltaTime * player.MoveSpeed);

        player.Animator.SetFloat(AnimatorConstants.AxisX, axisX);
        player.Animator.SetFloat(AnimatorConstants.AxisY, axisY);
    }
    #endregion
}
