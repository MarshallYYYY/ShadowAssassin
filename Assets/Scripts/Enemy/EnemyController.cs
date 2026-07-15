using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人控制器：基于 FSM 驱动巡逻、追击、攻击、受击、死亡状态。
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CapsuleCollider))]
public class EnemyController : MonoBehaviour, IStateMachineOwner
{
    #region 组件
    private Animator animator;
    private CharacterController characterController;
    private CapsuleCollider capsuleCollider;
    private StateMachine<EnemyController> stateMachine;
    #endregion

    #region 状态实例
    private EnemyIdleState idleState;
    private EnemyPatrolState patrolState;
    private EnemyChaseState chaseState;
    private EnemyAttackState attackState;
    private EnemyHitState hitState;
    private EnemyDeadState deadState;
    #endregion

    #region 配置
    [SerializeField] private EnemySO config;
    [SerializeField] private GameObject enemyHealthBarPrefab;
    [SerializeField] private EnemyHitbox hitbox;
    #endregion

    #region 运行时数据
    private float currentHP;
    private Vector3 spawnPosition;
    private float lastAttackTime;
    private bool isDead = false;
    private EnemyHealthBar enemyHealthBar;
    #endregion

    #region 事件
    /// <summary>
    /// 死亡时触发，通知 Spawner 回收
    /// </summary>
    public event Action<EnemyController> OnDeathEvent;
    /// <summary>
    /// 受击时触发，用于更新 UI
    /// </summary>
    public event Action<EnemyController> OnHitEvent;
    #endregion

    #region 公开属性
    #region 组件与状态机
    public CharacterController CharacterController => characterController;
    public StateMachine<EnemyController> StateMachine => stateMachine;
    public EnemyIdleState IdleState => idleState;
    public EnemyPatrolState PatrolState => patrolState;
    public EnemyChaseState ChaseState => chaseState;
    public EnemyAttackState AttackState => attackState;
    #endregion
    #region 血量
    public bool IsDead => isDead;
    #endregion

    #region 移动
    public float MoveSpeed => config.MoveSpeed;
    public float ChaseSpeed => config.ChaseSpeed;
    #endregion

    #region 检测
    public float DetectRange => config.DetectRange;
    public float AttackRange => config.AttackRange;
    public bool IsInAttackCooldown => Time.time - lastAttackTime < config.AttackCooldown;
    #endregion

    #region 巡逻
    public float PatrolRadius => config.PatrolRadius;
    public Vector3 SpawnPosition => spawnPosition;
    #endregion

    #region 攻击
    public List<AttackAnimSO> AttackAnims => config.AttackAnims;
    #endregion
    #endregion

    #region 生命周期
    void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // 初始化状态机
        stateMachine = new StateMachine<EnemyController>(this);
        idleState = new EnemyIdleState(this);
        patrolState = new EnemyPatrolState(this);
        chaseState = new EnemyChaseState(this);
        attackState = new EnemyAttackState(this);
        hitState = new EnemyHitState(this);
        deadState = new EnemyDeadState(this);

        enemyHealthBarRoot = GameObject.Find("EnemyHealthBarRoot").transform;
    }

    void Update()
    {
        stateMachine.Update();
    }

    void OnDestroy()
    {
        OnDeathEvent = null;
        OnHitEvent = null;
    }
    #endregion

    #region 公共方法
    private Transform enemyHealthBarRoot;
    /// <summary>
    /// 对象池取出时初始化
    /// </summary>
    public void OnSpawn(Vector3 position)
    {
        spawnPosition = position;
        transform.position = position;
        currentHP = config.MaxHP;
        isDead = false;
        lastAttackTime = 0f;

        // 启用碰撞
        characterController.enabled = true;
        capsuleCollider.enabled = true;

        // 创建血条 UI
        if (enemyHealthBarPrefab != null && enemyHealthBarRoot != null)
        {
            GameObject barGo = Instantiate(enemyHealthBarPrefab, enemyHealthBarRoot);
            enemyHealthBar = barGo.GetComponent<EnemyHealthBar>();
            enemyHealthBar.HideBar();
        }

        // 重置状态
        stateMachine.ChangeState(idleState);
    }

    /// <summary>
    /// 对象池回收
    /// </summary>
    public void OnDespawn()
    {
        // 销毁血条 UI
        if (enemyHealthBar != null)
        {
            enemyHealthBar.HideBar();
            Destroy(enemyHealthBar.gameObject);
            enemyHealthBar = null;
        }

        isDead = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0f);
        OnHitEvent?.Invoke(this);

        // 更新血条
        if (enemyHealthBar != null)
        {
            enemyHealthBar.OnHit();
            enemyHealthBar.SetHP(currentHP, config.MaxHP);
        }

        if (currentHP <= 0f)
        {
            isDead = true;
            enemyHealthBar?.OnDeath();
            OnDeathEvent?.Invoke(this);
            stateMachine.ChangeState(deadState);
        }
        else
        {
            stateMachine.ChangeState(hitState);
        }
    }

    /// <summary>
    /// 朝目标方向移动（含重力）
    /// </summary>
    public void MoveTo(Vector3 direction, float speed)
    {
        Vector3 motion = speed * Time.deltaTime * direction.normalized;
        motion.y = -9.8f * Time.deltaTime; // 重力
        characterController.Move(motion);
    }

    /// <summary>
    /// 平滑朝向目标
    /// </summary>
    public void FaceTarget(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, Time.deltaTime * config.RotationSpeed);
        }
    }

    /// <summary>
    /// 获取与 Player 的距离
    /// </summary>
    public float DistanceToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
        if (player == null)
            return float.MaxValue;
        return Vector3.Distance(transform.position, player.transform.position);
    }

    /// <summary>
    /// 获取 Player 的 Transform
    /// </summary>
    public Transform GetPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
        return player.transform;
    }

    /// <summary>
    /// 记录攻击时间（攻击状态调用）
    /// </summary>
    public void RecordAttack()
    {
        lastAttackTime = Time.time;
    }

    /// <summary>
    /// 启用武器伤害判定
    /// </summary>
    public void EnableHitbox(float damage, bool isPlayHitAnim)
    {
        hitbox?.EnableHitbox(damage, isPlayHitAnim);
    }

    /// <summary>
    /// 关闭武器伤害判定
    /// </summary>
    public void DisableHitbox()
    {
        hitbox?.DisableHitbox();
    }

    /// <summary>
    /// 播放指定动画状态（CrossFade）
    /// </summary>
    public void PlayAnim(string stateName, float fixedTransitionDuration = 0f)
    {
        animator.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
    }

    /// <summary>
    /// 清除一次性输入标志（状态切换时由 StateMachine 自动调用）
    /// </summary>
    public void ClearOneShotInputs()
    {
        // Enemy 目前没有一次性输入，预留接口
    }
    #endregion
}
