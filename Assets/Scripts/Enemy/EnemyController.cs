using System;
using UnityEngine;

/// <summary>
/// 敌人控制器：基于 FSM 驱动巡逻、追击、攻击、受击、死亡状态。
/// 使用 Capsule 代替模型，挂载 CharacterController 进行移动。
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CapsuleCollider))]
public class EnemyController : MonoBehaviour
{
    #region 组件
    private CharacterController characterController;
    private CapsuleCollider capsuleCollider;
    private StateMachine<EnemyController> stateMachine;
    #endregion

    #region 状态实例
    private EnemyPatrolState patrolState;
    private EnemyChaseState chaseState;
    private EnemyAttackState attackState;
    private EnemyHitState hitState;
    private EnemyDeadState deadState;
    #endregion

    #region 配置
    [SerializeField] private EnemySO config;
    #endregion

    #region 运行时数据
    private float currentHP;
    private Vector3 spawnPosition;
    private float lastAttackTime;
    private bool isDead = false;
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
    public EnemyPatrolState PatrolState => patrolState;
    public EnemyChaseState ChaseState => chaseState;
    public EnemyAttackState AttackState => attackState;
    public EnemyHitState HitState => hitState;
    public EnemyDeadState DeadState => deadState;
    #endregion

    #region 配置
    public EnemySO Config => config;
    #endregion

    #region 血量
    public float MaxHP => config.MaxHP;
    public float CurrentHP => currentHP;
    public bool IsDead => isDead;
    #endregion

    #region 移动
    public float MoveSpeed => config.MoveSpeed;
    public float ChaseSpeed => config.ChaseSpeed;
    public float RotationSpeed => config.RotationSpeed;
    #endregion

    #region 检测
    public float DetectRange => config.DetectRange;
    public float AttackRange => config.AttackRange;
    public float AttackCooldown => config.AttackCooldown;
    public float AttackDamage => config.AttackDamage;
    public bool IsInAttackCooldown => Time.time - lastAttackTime < config.AttackCooldown;
    #endregion

    #region 巡逻
    public float PatrolRadius => config.PatrolRadius;
    public float PatrolWaitTime => config.PatrolWaitTime;
    public Vector3 SpawnPosition => spawnPosition;
    #endregion

    #region 攻击
    public float LastAttackTime => lastAttackTime;
    #endregion
    #endregion

    #region 生命周期
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // 初始化状态机
        stateMachine = new StateMachine<EnemyController>(this);
        patrolState = new EnemyPatrolState(this);
        chaseState = new EnemyChaseState(this);
        attackState = new EnemyAttackState(this);
        hitState = new EnemyHitState(this);
        deadState = new EnemyDeadState(this);
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

        // 重置状态
        stateMachine.ChangeState(patrolState);
    }

    /// <summary>
    /// 对象池回收
    /// </summary>
    public void OnDespawn()
    {
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

        if (currentHP <= 0f)
        {
            isDead = true;
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
    #endregion
}
