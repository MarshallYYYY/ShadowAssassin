using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 基于 FSM 的玩家控制器，替代 ThirdPersonController。
/// 持有所有组件引用与共享数据，处理 Input 回调，由 StateMachine 驱动各状态逻辑。
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AfterImageEffect))]
public class PlayerController : MonoBehaviour
{
    #region 组件
    private CharacterController characterController;
    private Animator animator;
    private AfterImageEffect afterImage;
    private ThirdPersonControl inputActions;
    #endregion

    #region 状态机
    private StateMachine<PlayerController> stateMachine;
    private PlayerIdleState idleState;
    private PlayerMoveState moveState;
    private PlayerAttackState attackState;
    private PlayerRollState rollState;
    private PlayerAvoidState avoidState;
    #endregion

    #region 外部赋值
    [SerializeField] private Transform lockTarget;
    [SerializeField] private AttackAnimDataBaseSO attackAnimDataBaseSO;

    #region Combo Slider
    [Header("连击指示器")]
    [SerializeField] private Slider comboSlider;
    [SerializeField] private RectTransform separatorTemplate;
    [SerializeField] private RectTransform separators;
    [SerializeField] private Text currentComboText;
    #endregion
    #endregion

    #region 可在Inspector面板调节
    [Header("调节测试")]
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private float moveSpeed = 5f;
    #endregion

    #region 输入数据
    private Vector2 moveInput;
    private bool isLock = false;
    private AttackType attackType = AttackType.None;
    private bool isRollPressed = false;
    private bool isAvoidPressed = false;
    #endregion

    #region 动作数据
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

    #region 公开属性 — 供 States 访问
    #region 组件
    public CharacterController CharacterController => characterController;
    public Animator Animator => animator;
    public AfterImageEffect AfterImage => afterImage;
    #endregion

    #region 输入数据
    public Vector2 MoveInput => moveInput;
    public bool IsLock => isLock;
    public AttackType AttackType { get => attackType; set => attackType = value; }
    public bool IsRollPressed { get => isRollPressed; set => isRollPressed = value; }
    public bool IsAvoidPressed { get => isAvoidPressed; set => isAvoidPressed = value; }
    #endregion

    #region 配置
    public Transform LockTarget => lockTarget;
    public AttackAnimDataBaseSO AttackAnimDataBaseSO => attackAnimDataBaseSO;
    public float MoveSpeed => moveSpeed;
    public float RotationSmoothTime => rotationSmoothTime;
    #endregion

    #region UI
    public Slider ComboSlider => comboSlider;
    public RectTransform SeparatorTemplate => separatorTemplate;
    public RectTransform Separators => separators;
    public Text CurrentComboText => currentComboText;
    #endregion

    #region 动作数据
    public int ComboIndex { get => comboIndex; set => comboIndex = value; }
    public float CurrentAnimTime { get => currentAnimTime; set => currentAnimTime = value; }
    public float CurrentActionTotalTime { get => currentActionTotalTime; set => currentActionTotalTime = value; }
    #endregion

    #region 状态机
    public StateMachine<PlayerController> StateMachine => stateMachine;
    #endregion

    #region 状态实例
    public PlayerIdleState IdleState => idleState;
    public PlayerMoveState MoveState => moveState;
    public PlayerAttackState AttackState => attackState;
    public PlayerRollState RollState => rollState;
    public PlayerAvoidState AvoidState => avoidState;
    #endregion
    #endregion

    #region 生命周期
    void Awake()
    {
        afterImage = GetComponent<AfterImageEffect>();
        inputActions = GameManager.Instance.InputActions;

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.LockTarget.performed += OnLockTarget;
        inputActions.Player.Avoid.performed += OnAvoid;
        inputActions.Player.Roll.performed += OnRoll;
        inputActions.Player.LightAttack.performed += OnLightAttack;
        inputActions.Player.HeavyAttack.performed += OnHeavyAttack;

        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        comboSlider.gameObject.SetActive(false);
        separatorTemplate.gameObject.SetActive(false);

        // 初始化状态机
        stateMachine = new StateMachine<PlayerController>(this);
        idleState = new PlayerIdleState(this);
        moveState = new PlayerMoveState(this);
        attackState = new PlayerAttackState(this);
        rollState = new PlayerRollState(this);
        avoidState = new PlayerAvoidState(this);

        stateMachine.ChangeState(idleState);
    }

    void Update()
    {
        stateMachine.Update();
    }

    void OnDestroy()
    {
        // 使用命名方法注册回调，可在 OnDestroy 中正确取消订阅
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.LockTarget.performed -= OnLockTarget;
            inputActions.Player.Avoid.performed -= OnAvoid;
            inputActions.Player.Roll.performed -= OnRoll;
            inputActions.Player.LightAttack.performed -= OnLightAttack;
            inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;
        }
    }
    #endregion

    #region Input System
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void OnLockTarget(InputAction.CallbackContext context)
    {
        isLock = !isLock;
    }
    private void OnRoll(InputAction.CallbackContext context)
    {
        isRollPressed = true;
    }
    private void OnAvoid(InputAction.CallbackContext context)
    {
        isAvoidPressed = true;
    }
    private void OnLightAttack(InputAction.CallbackContext context)
    {
        attackType = AttackType.Light;
    }
    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        attackType = AttackType.Heavy;
    }
    #endregion

    #region 辅助方法 — 供 States 调用
    /// <summary>
    /// 执行攻击动作前的通用设置：动画参数归零
    /// </summary>
    public void SetAnimatorBeforeAction()
    {
        animator.SetFloat(AnimatorConstants.AxisX, 0);
        animator.SetFloat(AnimatorConstants.AxisY, 0);
    }

    /// <summary>
    /// 播放攻击动画并更新连击 UI
    /// </summary>
    public void PlayAttack(int index, AttackType type)
    {
        SetAnimatorBeforeAction();
        AnimationClip clip;

        if (type == AttackType.Light)
        {
            // 轻攻击：播放第 index 击，索引推进
            AttackAnimSO attackAnimSO = attackAnimDataBaseSO.LightAttackAnims[index];
            clip = attackAnimSO.Clip;
            comboIndex = index + 1;

            // 更新 ComboSlider 相关内容
            comboSlider.gameObject.SetActive(true);

            ClearSeparators();
            currentActionTotalTime = attackAnimSO.Length;
            CreateSeparator(currentActionTotalTime, attackAnimSO.EnterHitTime);
            CreateSeparator(currentActionTotalTime, attackAnimSO.EnterFollowThroughTime);

            currentComboText.text = (index + 1).ToString();
        }
        else
        {
            // 重攻击：0 - 蓄力前刺，1 - 跳跃斩击，2 - 派生攻击
            // 连击归零，不显示 Slider
            clip = attackAnimDataBaseSO.HeavyAttackAnims[index].Clip;
            comboIndex = 0;
            currentActionTotalTime = clip.length;
            comboSlider.gameObject.SetActive(false);

            animator.CrossFade(clip.name, 0f);
        }
        animator.CrossFade(clip.name, 0f);
        currentAnimTime = 0f;
    }

    /// <summary>临时：Invoke 回调，1 秒后关闭残影效果</summary>
    public void StopAfterImage()
    {
        afterImage.StopEffect();
    }
    #endregion

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

    public void ClearSeparators()
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
}
