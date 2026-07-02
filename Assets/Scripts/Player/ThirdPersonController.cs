using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AfterImageEffect))]
public class ThirdPersonController : MonoBehaviour
{
    #region InputActionAsset and Component
    private ThirdPersonControl inputActions;
    private CharacterController characterController;
    private Animator animator;
    #endregion

    #region 外部赋值
    [SerializeField] private Transform lockTarget;
    [SerializeField] private AnimationClip[] lightAttackClips;
    [SerializeField] private AnimationClip[] heavyAttackClips;
    [SerializeField] private AttackAnimDataBaseSO attackAnimDB;

    #region Combo Slider
    [SerializeField] private Slider comboSlider;
    [SerializeField] private RectTransform separatorTemplate;
    [SerializeField] private RectTransform separators;
    [SerializeField] private Text currentComboText;
    #endregion
    #endregion

    #region Data
    #region 移动
    // 目标朝向角度，用于角色转向
    // private float targetRotation = 0f;

    /// <summary>
    /// 保存当前移动输入向量，x 表示左右，y 表示前后
    /// </summary>
    private Vector2 move;
    private bool isLock = false;
    /// <summary>
    /// SmoothDampAngle 的速度缓存，避免每帧重新计算
    /// </summary>
    private float rotationVelocity;
    private bool isCanMove = true;
    #endregion

    #region 动作
    private AttackType attckType = AttackType.None;
    /// <summary>
    /// 轻攻击连击索引（0 = 无连击，1~N = 已打出第N击）
    /// </summary>
    private int comboIndex = 0;
    /// <summary>
    /// 当前动作动画的已播放时长（从 0 累加到 totalTime）
    /// </summary>
    private float currentAnimTime = 0;
    /// <summary>
    /// 当前动作动画的总时长
    /// </summary>
    private float currentActionTotalTime = 0;
    #endregion
    #endregion

    #region 可在Inspector面板调节
    [Header("调节测试")]
    /// <summary>
    /// 平滑旋转时间，值越小转向越快，越大越平滑
    /// </summary>
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lockedTargetRotateSpeed = 5f;
    #endregion
    private AfterImageEffect afterImage;
    void Awake()
    {
        afterImage = GetComponent<AfterImageEffect>(); // 挂在 Player 上，Awake 时拿到
        // 初始化输入系统，并监听角色移动输入
        inputActions = GameManager.Instance.InputActions;
        // inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.LockTarget.performed += ctx => isLock = !isLock;

        inputActions.Player.Avoid.performed += OnAvoid;
        inputActions.Player.Roll.performed += OnRoll;
        inputActions.Player.LightAttack.performed += ctx => attckType = AttackType.Light;
        inputActions.Player.HeavyAttack.performed += ctx => attckType = AttackType.Heavy;


        // 获取角色控制器组件，用于执行移动
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        comboSlider.gameObject.SetActive(false);
        separatorTemplate.gameObject.SetActive(false);
    }
    // void OnEnable()
    // {
    //     inputActions.Enable();
    // }
    // void OnDisable()
    // {
    //     inputActions.Disable();
    // }
    void OnDestroy()
    {
        // 只取消回调，不 Dispose —— inputActions 是 GameManager 的共享实例
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.Avoid.performed -= OnAvoid;
            inputActions.Player.Roll.performed -= OnRoll;
            // LockTarget 和 Attack 是 lambda，无法取消，影响不大
        }
    }
    void Update()
    {
        HandleMove();
        HandleAction();
    }
    #region Input System
    private void OnMove(InputAction.CallbackContext context)
    {
        // 读取输入系统中的移动值
        move = context.ReadValue<Vector2>();
    }
    private void OnRoll(InputAction.CallbackContext context)
    {
        SetAnimatorBeforeAction();
        animator.SetTrigger(AnimatorConstants.Roll);
        // TODO: 目前未设置 currentActionTotalTime，Roll 结束后 isCanMove 不会自动恢复，
        //       依赖 HandleAction 中 outTimer（已废弃）或 Animator 的 Exit Time 兜底，后续需补上 Roll clip 时长。
        currentActionTotalTime = 0;
    }

    private void OnAvoid(InputAction.CallbackContext context)
    {
        SetAnimatorBeforeAction();
        animator.SetTrigger(AnimatorConstants.Avoid);
        currentActionTotalTime = 0;
        afterImage.StartEffect();
        // TODO: 临时方案——用 Invoke 硬编码 1 秒后关闭残影。
        //       后续应改为检测 Avoid 动画退出时自动调 StopEffect()。
        Invoke(nameof(StopAfterImage), 1f);
    }
    /// <summary>临时：Invoke 回调，1 秒后关闭残影效果</summary>
    private void StopAfterImage()
    {
        afterImage.StopEffect();
    }
    #endregion

    #region Move
    private void HandleMove()
    {
        if (isCanMove is false)
            return;
        if (isLock is false)
        {
            FreeMove();
        }
        else
        {
            LockedTargetMove();
        }
    }
    private void FreeMove()
    {
        if (move != Vector2.zero)
        {
            // 将二维输入转换为三维方向，忽略 Y 轴，方便在地面上移动
            Vector3 inputDir = new Vector3(move.x, 0, move.y).normalized;

            // 使用 Atan2 计算输入方向对应的角度，返回的是弧度，需要乘以 Rad2Deg 转为角度
            // 再加上主相机当前的 Y 轴角度，让角色朝向相对摄像机的移动方向
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

            // 使用 SmoothDampAngle 让角色旋转过程更平滑，避免直接瞬间转向
            float rotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                rotationSmoothTime);

            // 根据平滑后的角度设置角色朝向
            transform.rotation = Quaternion.Euler(0, rotation, 0);

            float axisY = animator.GetFloat(AnimatorConstants.AxisY);
            axisY = Mathf.MoveTowards(axisY, move.magnitude, moveSpeed * Time.deltaTime);
            animator.SetFloat(AnimatorConstants.AxisY, axisY);

            characterController.Move(moveSpeed * Time.deltaTime * transform.forward);
            characterController.Move(moveSpeed * Time.deltaTime * Vector3.down);
        }
        else
        {
            // 平滑的过渡到 Idle 动画
            float axisY = animator.GetFloat(AnimatorConstants.AxisY);
            axisY = Mathf.MoveTowards(axisY, move.magnitude, moveSpeed * Time.deltaTime);
            animator.SetFloat(AnimatorConstants.AxisY, axisY);
            // animator.SetFloat(AnimatorConstants.AxisY, 0);
        }
    }
    private void LockedTargetMove()
    {
        if (lockTarget == null)
            return;

        // 1. 计算目标方向，锁定目标时角色始终朝向目标
        Vector3 dirToTarget = lockTarget.position - transform.position;
        dirToTarget.y = 0f;

        if (dirToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * lockedTargetRotateSpeed
            );
        }

        // 2. 读取 InputSystem 的移动输入
        Vector3 inputDir = new Vector3(move.x, 0f, move.y).normalized;

        // 3. 以当前角色朝向为参考，做八方向移动
        if (inputDir.sqrMagnitude > 0.001f)
        {
            Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
            moveDir = moveDir.normalized;

            characterController.Move(moveSpeed * Time.deltaTime * moveDir);
            characterController.Move(moveSpeed * Time.deltaTime * Vector3.down);
        }

        // 4. 更新动画参数
        float axisX = animator.GetFloat(AnimatorConstants.AxisX);
        float axisY = animator.GetFloat(AnimatorConstants.AxisY);

        axisX = Mathf.MoveTowards(axisX, move.x, Time.deltaTime * moveSpeed);
        axisY = Mathf.MoveTowards(axisY, move.y, Time.deltaTime * moveSpeed);

        animator.SetFloat(AnimatorConstants.AxisX, axisX);
        animator.SetFloat(AnimatorConstants.AxisY, axisY);
    }
    #endregion
    #region Action 动作
    /// <summary>
    /// 每帧处理动作输入与动画状态。
    /// 核心规则：
    /// 1. Idle 时可按轻/重攻击自由起手
    /// 2. 轻攻击进入连击链，仅在收尾阶段（EnterFollowThroughTime）可接下一击
    /// 3. 重攻击打断一切，不可连击
    /// 4. 动画播完 → 恢复移动、连击归零、Slider 隐藏
    /// </summary>
    private void HandleAction()
    {
        // 1. 更新动画计时
        if (!isCanMove)
        {
            currentAnimTime += Time.deltaTime;
            comboSlider.value = currentAnimTime;
        }

        // 2. 处理排队中的攻击输入
        if (attckType != AttackType.None)
        {
            if (isCanMove)
            {
                // Idle 状态：如果处于移动 且 点击了重攻击键，则重攻击播放的是 重攻击 - 跳跃斩击 动作
                if (animator.GetFloat(AnimatorConstants.AxisY) >= 0.9f && attckType == AttackType.Heavy)
                {
                    PlayAttack(1, attckType);
                }
                else
                {
                    // Idle 状态：轻/重攻击都可以自由起手，但是重攻击播放的是 重攻击 - 普通 动作，轻攻击则是播放第一个动作。
                    PlayAttack(0, attckType);
                }
            }
            // 连击过程中：
            else if (attckType != AttackType.None
                     && comboIndex > 0
                     && comboIndex < lightAttackClips.Length)
            {
                // 轻攻击连击（2-5） 和 重攻击 - 转身突刺：仅在前四段轻攻击连击的收尾阶段可触发
                int currentIndex = comboIndex - 1;
                bool isCanAttack = currentAnimTime >= attackAnimDB.LightAttackAnims[currentIndex].EnterFollowThroughTime;
                if (isCanAttack)
                {
                    // 轻攻击走连击索引；重攻击打第 3 击（派生技：转身突刺）
                    int index = (attckType == AttackType.Light) ? comboIndex : 2;
                    PlayAttack(index, attckType);
                }
            }
            attckType = AttackType.None;
        }

        // 3. 动画播完 → 恢复移动、连击归零、Slider 隐藏
        if (!isCanMove && currentAnimTime >= currentActionTotalTime)
        {
            isCanMove = true;
            comboIndex = 0;
            currentAnimTime = 0;
            comboSlider.gameObject.SetActive(false);
            ClearSeparators();
        }
    }
    private void PlayAttack(int index, AttackType attckType)
    {
        // TODO：释放攻击动作过程中，还要限制翻滚和闪避的使用

        SetAnimatorBeforeAction();
        AnimationClip clip;

        if (attckType == AttackType.Light)
        {
            // 轻攻击：播放第 index 击，索引推进
            clip = lightAttackClips[index];
            comboIndex = index + 1;

            // 更新 ComboSlider 相关内容
            comboSlider.gameObject.SetActive(true);

            ClearSeparators();
            AttackAnim attackAnim = attackAnimDB.LightAttackAnims[index];
            currentActionTotalTime = attackAnim.Length;
            CreateSeparator(currentActionTotalTime, attackAnim.EnterHitTime);
            CreateSeparator(currentActionTotalTime, attackAnim.EnterFollowThroughTime);

            currentComboText.text = (index + 1).ToString();
        }
        else
        {
            // 重攻击：永远打第 0 击，连击归零，不显示 Slider
            clip = heavyAttackClips[index];
            comboIndex = 0;
            currentActionTotalTime = clip.length;
            comboSlider.gameObject.SetActive(false);

            animator.CrossFade(clip.name, 0f);
        }
        animator.CrossFade(clip.name, 0f);
        currentAnimTime = 0f;
    }
    private void SetAnimatorBeforeAction()
    {
        animator.SetFloat(AnimatorConstants.AxisX, 0);
        animator.SetFloat(AnimatorConstants.AxisY, 0);
        // move = Vector2.zero;
        isCanMove = false;
    }
    #region Separator
    private void CreateSeparator(float maxValue, float value)
    {
        comboSlider.minValue = 0;
        comboSlider.maxValue = maxValue;

        RectTransform separator = Instantiate(original: separatorTemplate, parent: separators);
        separator.gameObject.SetActive(true);

        float percent = Mathf.InverseLerp(0, maxValue, value);
        separator.anchorMin = new Vector2(percent, 0.5f);
        separator.anchorMax = new Vector2(percent, 0.5f);

        separator.anchoredPosition = Vector2.zero;
    }
    private void ClearSeparators()
    {
        // 使用 DestroyImmediate 立即销毁
        while (separators.childCount > 0)
        {
            Transform child = separators.GetChild(0);
            child.SetParent(null);
            DestroyImmediate(child.gameObject);
        }
    }
    #endregion
    #endregion
}

public enum AttackType
{
    None,
    Light,
    Heavy,
}