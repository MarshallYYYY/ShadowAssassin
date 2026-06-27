using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    #region SerializeField
    /// <summary>
    /// Player - Look Root
    /// </summary>
    [SerializeField] private GameObject cameraTarget;
    /// <summary>
    /// 视角往上的最大角度
    /// </summary>
    [SerializeField] private float topClamp = 70.0f;
    /// <summary>
    /// 视角向下的最大角度
    /// </summary>
    [SerializeField] private float bottomClamp = -30.0f;
    private float lookScale = 2f;
    #endregion

    #region Data
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
    void Awake()
    {
        inputActions = new();
        inputActions.Player.Enable();
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        // 保存摄像机目标的Y轴角度
        cinemachineTargetYaw = cameraTarget.transform.rotation.eulerAngles.y;
    }
    #region Input Actions
    private ThirdPersonControl inputActions;
    private void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>() * lookScale;
    }
    #endregion
    void LateUpdate()
    {
        // if (thirdPersonMove.IsLock)
        //     return;

        // 判断有没有有效的鼠标移动：如果鼠标输入的移动量已经大于一个很小的阈值，就认为玩家真的在转视角。
        // look.sqrMagnitude：look 向量的长度平方 = look.x*look.x + look.y*look.y
        if (look.sqrMagnitude >= Threshold)
        {
            cinemachineTargetYaw += look.x;
            cinemachineTargetPitch -= look.y;
        }
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        cameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0f);
    }
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}