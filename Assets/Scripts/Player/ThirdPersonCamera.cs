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
    /// <summary>
    /// 视角往上的最大角度
    /// </summary>
    [SerializeField] private float topClamp = 70.0f;
    /// <summary>
    /// 视角向下的最大角度
    /// </summary>
    [SerializeField] private float bottomClamp = -30.0f;
    [SerializeField] private float lookScale = 0.5f;
    [SerializeField] private float lockRotationSmoothTime = 0.5f;
    #endregion

    #region 组件与数据
    private CinemachineVirtualCamera virtualCamera;
    private ThirdPersonControl inputActions;

    private const float Threshold = 0.01f;
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
    private float lockPitchVelocity;
    void LateUpdate()
    {
        // 跟随目标可能为空（场景未正确配置）
        Transform followTarget = virtualCamera.Follow;
        if (followTarget == null)
            return;

        // ===== 锁定模式：禁止鼠标控制视角，将视角平滑转向锁定目标 =====
        if (playerController != null && playerController.IsLock)
        {
            if (playerController.LockTarget != null)
            {
                // 计算从摄像机跟随目标指向锁定目标的方向向量
                Vector3 dirToTarget = playerController.LockTarget.position - followTarget.position;

                // Yaw（左右偏航）：通过 Atan2(x, z) 计算方向向量在 XZ 平面上的角度
                float targetYaw = Mathf.Atan2(dirToTarget.x, dirToTarget.z) * Mathf.Rad2Deg;

                // Pitch（上下俯仰）：先求水平距离，再通过高度差与水平距离的反正切计算俯仰角
                // 目标在上方时 dirToTarget.y > 0，Atan2 返回正值，取负号让摄像机仰视
                float horizontalDist = new Vector2(dirToTarget.x, dirToTarget.z).magnitude;
                float targetPitch = -Mathf.Atan2(dirToTarget.y, horizontalDist) * Mathf.Rad2Deg;
                // 限制俯仰角在 [bottomClamp, topClamp] 范围内，避免视角翻转
                targetPitch = ClampAngle(targetPitch, bottomClamp, topClamp);

                // SmoothDampAngle 是一个弹簧物理模拟，它需要跨帧保持速度状态。lockYawVelocity 存储的是当前旋转的角速度。
                // 使用 SmoothDampAngle 平滑过渡，避免按下锁定键时视角瞬间跳转
                cinemachineTargetYaw = Mathf.SmoothDampAngle(
                    cinemachineTargetYaw, targetYaw, ref lockYawVelocity, lockRotationSmoothTime);
                cinemachineTargetPitch = Mathf.SmoothDampAngle(
                    cinemachineTargetPitch, targetPitch, ref lockPitchVelocity, lockRotationSmoothTime);

                // 将最终的 Yaw 和 Pitch 应用到摄像机跟随目标的旋转
                followTarget.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0f);
            }
            return;
        }

        // ===== 自由视角模式：通过鼠标输入控制视角 =====
        // 判断有没有有效的鼠标移动：如果鼠标输入的移动量已经大于一个很小的阈值，就认为玩家真的在转视角。
        // look.sqrMagnitude：look 向量的长度平方 = look.x*look.x + look.y*look.y
        if (look.sqrMagnitude >= Threshold)
        {
            cinemachineTargetYaw += look.x;
            // look.y 取负：鼠标向上推时视角向上看（Pitch 增大），符合直觉
            cinemachineTargetPitch -= look.y;
        }
        // Yaw 不限制范围（可以无限旋转），Pitch 限制在 [bottomClamp, topClamp] 范围内
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // 将最终的 Yaw 和 Pitch 应用到摄像机跟随目标的旋转
        followTarget.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0f);
    }
    #endregion

    #region Input Actions
    private void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>() * lookScale;
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

    #region 辅助
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
    #endregion
}
