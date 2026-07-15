using System;
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
public class PlayerController : MonoBehaviour, IStateMachineOwner
{
    #region 组件
    private CharacterController characterController;
    private Animator animator;
    private ThirdPersonControl inputActions;
    private AfterImageEffect afterImage;
    #endregion

    #region 状态机 和 状态
    private StateMachine<PlayerController> stateMachine;
    private PlayerLocomotionState locomotionState;
    private PlayerAttackState attackState;
    private PlayerRollState rollState;
    private PlayerAvoidState avoidState;
    private PlayerHitState hitState;
    private PlayerDeadState deadState;
    #endregion

    #region 外部赋值 SerializeField
    [SerializeField] private AttackAnimDataBaseSO attackAnimDataBaseSO;
    [SerializeField] private WeaponHitbox weaponHitbox;

    #region Combo Slider
    [Header("连击指示器")]
    [SerializeField] private Slider comboSlider;
    [SerializeField] private RectTransform separatorTemplate;
    [SerializeField] private RectTransform separators;
    [SerializeField] private Text currentComboText;
    #endregion

    #region 血条UI
    [Header("血条UI")]
    [SerializeField] private PlayerHealthBar playerHealthBar;
    #endregion
    #endregion

    #region 输入数据
    private Vector2 moveInput;
    private bool isLock = false;
    private Transform lockTarget;
    private PlayerAttackType attackType = PlayerAttackType.None;
    /// <summary>
    /// 霸体
    /// </summary>
    private bool isSuperArmor = false;
    private bool isRollPressed = false;
    private bool isAvoidPressed = false;
    #endregion

    #region 动作数据
    // TODO 将移动速度放入SO中
    private float moveSpeed = 5f;
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
    private float currentAnimTotalTime = 0;
    /// <summary>
    /// 当前攻击的 AttackAnimSO，用于获取判定时间窗口和伤害值
    /// </summary>
    private AttackAnimSO currentAttackAnimSO;
    #endregion

    #region 血量数据
    private float currentHP;
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
    public PlayerAttackType AttackType { get => attackType; set => attackType = value; }
    /// <summary>
    /// 霸体
    /// </summary>
    public bool IsSuperArmor { get => isSuperArmor; set => isSuperArmor = value; }
    public bool IsRollPressed { get => isRollPressed; set => isRollPressed = value; }
    public bool IsAvoidPressed { get => isAvoidPressed; set => isAvoidPressed = value; }
    #endregion

    #region 配置
    public Transform LockTarget => lockTarget;
    public AttackAnimDataBaseSO AttackAnimDataBaseSO => attackAnimDataBaseSO;
    #endregion

    #region UI - ComboSlider
    public Slider ComboSlider => comboSlider;
    #endregion

    #region 动作数据
    public float MoveSpeed { get => moveSpeed; }
    public int ComboIndex { get => comboIndex; set => comboIndex = value; }
    public float CurrentAnimTime { get => currentAnimTime; set => currentAnimTime = value; }
    public float CurrentAnimTotalTime { get => currentAnimTotalTime; set => currentAnimTotalTime = value; }
    public AttackAnimSO CurrentAttackAnimSO { get => currentAttackAnimSO; set => currentAttackAnimSO = value; }
    #endregion

    #region 状态机 和 状态实例
    public StateMachine<PlayerController> StateMachine => stateMachine;

    public PlayerLocomotionState LocomotionState => locomotionState;
    public PlayerAttackState AttackState => attackState;
    public PlayerRollState RollState => rollState;
    public PlayerAvoidState AvoidState => avoidState;
    // public PlayerHitState HitState => hitState;
    // public PlayerDeadState DeadState => deadState;
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

        // 初始化血量
        currentHP = PersistentService.Instance.GetPlayerMaxHP();
        // 保留 ? 是因为在 VillageScene 中没有血条UI
        playerHealthBar?.SetHP(currentHP);

        // 初始化武器
        if (weaponHitbox != null)
        {
            weaponHitbox.Init(this);
        }

        // 初始化状态机
        stateMachine = new StateMachine<PlayerController>(this);
        locomotionState = new PlayerLocomotionState(this);
        attackState = new PlayerAttackState(this);
        rollState = new PlayerRollState(this);
        avoidState = new PlayerAvoidState(this);
        hitState = new PlayerHitState(this);
        deadState = new PlayerDeadState(this);

        stateMachine.ChangeState(locomotionState);
    }

    void Update()
    {
        stateMachine.Update();
    }

    void OnDestroy()
    {
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
        if (isLock)
        {
            // 取消锁定
            isLock = false;
            lockTarget = null;
        }
        else
        {
            // 锁定最近的 Enemy
            EnemyController nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null)
            {
                lockTarget = nearestEnemy.transform;
                isLock = true;
            }
        }
    }
    /// <summary>
    /// 查找最近的存活 Enemy
    /// </summary>
    private EnemyController FindNearestEnemy()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        EnemyController nearestEnemy = null;
        float nearestSqrDist = float.MaxValue;

        foreach (EnemyController enemy in enemies)
        {
            if (enemy.IsDead)
                continue;

            float sqrDist = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < nearestSqrDist)
            {
                nearestSqrDist = sqrDist;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
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
        attackType = PlayerAttackType.Light;
    }
    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        attackType = PlayerAttackType.Heavy;
    }
    #endregion

    #region 公共方法 — 供 States/状态机 调用
    #region 攻击动画、武器伤害盒子的开闭、受击伤害判定
    /// <summary>
    /// 播放攻击动画并更新连击 UI
    /// </summary>
    public void PlayAttack(int index, PlayerAttackType type)
    {
        isSuperArmor = type == PlayerAttackType.Heavy;
        AnimationClip clip;

        if (type == PlayerAttackType.Light)
        {
            // 轻攻击：播放第 index 击，索引推进
            AttackAnimSO attackAnimSO = attackAnimDataBaseSO.LightAttackAnims[index];
            clip = attackAnimSO.Clip;
            comboIndex = index + 1;
            currentAttackAnimSO = attackAnimSO;

            // 更新 ComboSlider 相关内容
            comboSlider.gameObject.SetActive(true);

            ClearSeparators();
            currentAnimTotalTime = clip.length;
            CreateSeparator(currentAnimTotalTime, attackAnimSO.EnterHitTime);
            CreateSeparator(currentAnimTotalTime, attackAnimSO.EnterFollowThroughTime);

            currentComboText.text = comboIndex.ToString();

            // 根据连击索引播放对应的轻攻击音效
            string[] lightAttackSfx = {
                AudioConstants.PlayerLightAttack_01,
                AudioConstants.PlayerLightAttack_02,
                AudioConstants.PlayerLightAttack_03,
                AudioConstants.PlayerLightAttack_04,
                AudioConstants.PlayerLightAttack_05,
            };
            AudioService.Instance.PlaySfx(lightAttackSfx[index]);
        }
        else
        {
            // 重攻击：0 - 蓄力前刺，1 - 跳跃斩击，2 - 派生攻击
            // 连击归零，不显示 Slider
            AttackAnimSO attackAnimSO = attackAnimDataBaseSO.HeavyAttackAnims[index];
            clip = attackAnimSO.Clip;
            comboIndex = 0;
            currentAttackAnimSO = attackAnimSO;
            currentAnimTotalTime = clip.length;
            comboSlider.gameObject.SetActive(false);

            // 根据重攻击形态播放对应音效
            string[] heavyAttackSfx = {
                AudioConstants.PlayerStabAttack,
                AudioConstants.PlayerLeapingAttack,
                AudioConstants.PlayerDeriveAttack,
            };
            AudioService.Instance.PlaySfx(heavyAttackSfx[index]);
        }
        // animator.CrossFadeInFixedTime(clip.name, 0f);
        animator.CrossFadeInFixedTime(clip.name, AnimatorConstants.AttackFadeDuration);
        currentAnimTime = 0f;
    }
    /// <summary>
    /// 启用武器伤害判定
    /// </summary>
    public void EnableWeaponHitbox()
    {
        if (weaponHitbox != null && currentAttackAnimSO != null)
        {
            weaponHitbox.EnableHitbox(currentAttackAnimSO.Damage);
        }
    }

    /// <summary>
    /// 关闭武器伤害判定
    /// </summary>
    public void DisableWeaponHitbox()
    {
        weaponHitbox?.DisableHitbox();
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="isPlayHitAnim">是否播放受击动画（仅 HorizontalAttack 触发）</param>
    public void TakeDamage(float damage, bool isPlayHitAnim = false)
    {
        if (currentHP <= 0f)
            return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0f);
        playerHealthBar.SetHP(currentHP);

        if (currentHP <= 0f)
        {
            stateMachine.ChangeState(deadState);
        }
        // 重攻击期间不会被打断，只扣血不播受击动画
        else if (isPlayHitAnim && !isSuperArmor)
        {
            stateMachine.ChangeState(hitState);
        }
    }
    #endregion

    #region 敌人死亡后，取消锁定
    /// <summary>
    /// 取消锁定，清空锁定目标
    /// </summary>
    public void ClearLock()
    {
        isLock = false;
        lockTarget = null;
    }
    #endregion
    #region 辅助方法
    /// <summary>
    /// 清除一次性输入标志（状态切换时由 StateMachine 自动调用）
    /// 注意：attackType 不在此清除，因为它由 AttackState.OnEnter 消费后自行清零
    /// </summary>
    public void ClearOneShotInputs()
    {
        isRollPressed = false;
        isAvoidPressed = false;
    }
    #endregion


    #endregion

    #region Separator：包含私有和公共方法
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
        while (separators.childCount > 0)
        {
            Transform child = separators.GetChild(0);
            child.SetParent(null);
            DestroyImmediate(child.gameObject);
        }
    }
    #endregion
}
