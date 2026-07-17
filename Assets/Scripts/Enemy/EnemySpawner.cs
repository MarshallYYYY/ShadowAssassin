using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人对象池生成器：预实例化一批 Enemy，通过 Spawn/Despawn 复用。
/// 挂在 DungeonScene 中的空物体上，配置生成点。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    #region 配置
    [Header("预制体")]
    [SerializeField] private EnemyController enemyPrefab;

    [Header("对象池")]
    [SerializeField] private int poolSize = 10;
    [SerializeField] private int initialSpawnCount = 3;

    [Header("生成点")]
    [SerializeField] private Transform[] spawnPoints;
    #endregion

    #region 池数据
    private readonly Queue<EnemyController> pool = new();
    private readonly List<EnemyController> activeEnemies = new();
    #endregion

    #region 事件
    /// <summary>
    /// 副本全部敌人清空时触发
    /// </summary>
    public event Action OnDungeonClear;
    #endregion

    #region 生命周期
    void Awake()
    {
        // 预实例化
        for (int i = 0; i < poolSize; i++)
        {
            EnemyController enemy = Instantiate(enemyPrefab, transform);
            enemy.gameObject.SetActive(false);
            enemy.OnDeathEvent += OnEnemyDeath;
            pool.Enqueue(enemy);
        }
    }

    void Start()
    {
        // 在生成点生成初始敌人
        for (int i = 0; i < initialSpawnCount && i < spawnPoints.Length; i++)
        {
            Spawn(spawnPoints[i].position);
        }
    }
    /// <summary>
    /// 从池中取出一个 Enemy 并放置到指定位置
    /// </summary>
    private EnemyController Spawn(Vector3 position)
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] 池已空，无可用 Enemy");
            return null;
        }

        EnemyController enemy = pool.Dequeue();
        enemy.gameObject.SetActive(true);
        enemy.OnSpawn(position);
        activeEnemies.Add(enemy);
        return enemy;
    }
    #endregion

    #region 公共方法

    /// <summary>
    /// 将 Enemy 回收到池中
    /// </summary>
    public void Despawn(EnemyController enemy)
    {
        if (!activeEnemies.Contains(enemy))
            return;

        activeEnemies.Remove(enemy);
        enemy.OnDespawn();
        pool.Enqueue(enemy);
        if (activeEnemies.Count == 0)
        {
            OnDungeonClear?.Invoke();
        }
    }
    #endregion

    #region 内部回调
    private void OnEnemyDeath(EnemyController enemy)
    {
        // 死亡回调由 DeadState 延迟调用 Despawn，此处仅用于扩展
    }
    #endregion
}
