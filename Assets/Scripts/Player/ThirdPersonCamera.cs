using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 第三人称摄像机控制器，挂载到 Cinemachine Virtual Camera 所在的 GameObject 上。
/// 通过旋转 VirtualCamera.Follow 目标来控制视角（Yaw/Pitch）。
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ThirdPersonCamera : MonoBehaviour
{
    #region SerializeField
    [SerializeField] private PlayerController playerController;
    #endregion

    #region 常量
    private const float Threshold = 0.01f;
    /// <summary>
    /// 视角往上的最大角度
    /// </summary>
    private const float TopClamp = 70.0f;
    // [SerializeField] private float topClamp = 70.0f;
    /// <summary>
    /// 视角向下的最大角度
    /// </summary>
    private const float BottomClamp = -30.0f;
    // [SerializeField] private float bottomClamp = -30.0f;
    #endregion

    #region 组件与数据
    private CinemachineVirtualCamera virtualCamera;
    private ThirdPersonControl inputActions;

    /// <summary>
    /// 摄像机围绕角色的左右旋转
    /// </summary>
    private float cinemachineTargetYaw;
    /// <summary>
    /// 摄像机围绕角色的上下旋转
    /// </summary>
    private float cinemachineTargetPitch;
    private Vector2 look;
    #endregion

    #region 生命周期
    void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        inputActions = GameManager.Instance.InputActions;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        // 初始化 Yaw 为当前跟随目标的 Y 轴角度
        cinemachineTargetYaw = virtualCamera.Follow.rotation.eulerAngles.y;
    }

    /// <summary>
    /// 锁定时 SmoothDampAngle 的速度缓存
    /// </summary>
    private float lockYawVelocity;
    /// <summary>
    /// 锁定时 SmoothDampAngle 的速度缓存
    /// </summary>
    private float lockPitchVelocity;
    private bool wasLocking = false;

    void LateUpdate()
    {
        // 跟随目标可能为空（场景未正确配置）
        Transform followTarget = virtualCamera.Follow;
        if (followTarget == null)
            return;

        if (playerController != null && playerController.IsLock)
        {
            UpdateLockedCamera(followTarget);
        }
        else
        {
            if (wasLocking)
            {
                wasLocking = false;
                lockYawVelocity = 0f;
                lockPitchVelocity = 0f;
            }
            UpdateFreeCamera(followTarget);
        }
    }

    #region 锁定模式 和 自由视角模式
    /// <summary>
    /// 锁定模式：将视角平滑转向锁定目标
    /// </summary>
    private void UpdateLockedCamera(Transform followTarget)
    {
        wasLocking = true;

        if (playerController.LockTarget == null)
            return;

        // 锁定目标正在死亡 → 冻结视角，避免追踪倒下的身体导致 Pitch 暴涨
        EnemyController lockEnemy = playerController.LockTarget.GetComponent<EnemyController>();
        if (lockEnemy != null && lockEnemy.IsDead)
            return;

        // 计算从 摄像机跟随目标（Enemy） 指向 锁定目标（Player的子物体） 的方向向量
        Vector3 dirToTarget = playerController.LockTarget.position - followTarget.position;

        // Yaw（左右偏航）：通过 Atan2(x, z) 计算方向向量在 XZ 平面上的角度
        float targetYaw = Mathf.Atan2(dirToTarget.x, dirToTarget.z) * Mathf.Rad2Deg;

        // Pitch（上下俯仰）：先求水平距离，再通过高度差与水平距离的反正切计算俯仰角
        // 目标在上方时 dirToTarget.y > 0，Atan2 返回正值，取负号让摄像机仰视
        float horizontalDist = new Vector2(dirToTarget.x, dirToTarget.z).magnitude;
        float targetPitch = -Mathf.Atan2(dirToTarget.y, horizontalDist) * Mathf.Rad2Deg;
        // 限制俯仰角在 [bottomClamp, topClamp] 范围内，避免视角翻转
        targetPitch = ClampAngle(targetPitch, BottomClamp, TopClamp);

        // SmoothDampAngle 是弹簧物理模拟，需要跨帧保持速度状态
        cinemachineTargetYaw = Mathf.SmoothDampAngle(
            cinemachineTargetYaw, targetYaw, ref lockYawVelocity, Constants.RotationSmoothTime);
        cinemachineTargetPitch = Mathf.SmoothDampAngle(
            cinemachineTargetPitch, targetPitch, ref lockPitchVelocity, Constants.RotationSmoothTime);

        ApplyRotation(followTarget);
    }

    /// <summary>
    /// 自由视角模式：通过鼠标输入控制视角
    /// </summary>
    private void UpdateFreeCamera(Transform followTarget)
    {
        // 鼠标输入大于阈值时，累加 Yaw/Pitch
        // look.y 取负：鼠标向上推时视角向上看（Pitch 增大），符合直觉
        if (look.sqrMagnitude >= Threshold)
        {
            cinemachineTargetYaw += look.x;
            cinemachineTargetPitch -= look.y;
        }

        // Yaw 不限制范围（可以无限旋转），Pitch 限制在 [bottomClamp, topClamp] 范围内
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        ApplyRotation(followTarget);
    }
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
    /// <summary>
    /// 将 Yaw/Pitch 应用到摄像机跟随目标
    /// </summary>
    private void ApplyRotation(Transform followTarget)
    {
        followTarget.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0f);
    }
    #endregion
    #endregion

    #region Input Actions
    private void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>() * PersistentService.Instance.LookSensitivity;
    }
    void OnEnable()
    {
        inputActions.Enable();
    }
    void OnDisable()
    {
        inputActions.Disable();
    }
    #endregion
}
