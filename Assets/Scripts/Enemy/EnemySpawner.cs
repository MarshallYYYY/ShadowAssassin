using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人对象池生成器：预实例化一批 Enemy，通过 Spawn/Despawn 复用。
/// 挂在 DungeonScene 中的空物体上，配置生成点。
/// 三波怪物：第1波1只、第2波3只、第3波5只，每波在 spawnPoints 中随机选点，每个点最多生成一只。
/// 第3波全部清空后触发 OnDungeonClear。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    #region 配置
    [Header("预制体")]
    [SerializeField] private EnemyController enemyPrefab;

    [Header("对象池")]
    [SerializeField] private int poolSize = 10;

    [Header("生成点")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("波次配置")]
    [SerializeField] private float waveInterval = 2f;
    #endregion

    #region 波次数据
    private static readonly int[] WaveCounts = { 1, 3, 5 };
    private int currentWave = 0;
    private float waveTimer = 0f;
    private bool isWaitingForNextWave = false;
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
        SpawnWave(0);
    }

    void Update()
    {
        // 当前波次已清空且还有下一波 → 等待间隔后生成下一波
        if (isWaitingForNextWave)
        {
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveInterval)
            {
                isWaitingForNextWave = false;
                SpawnWave(currentWave);
            }
        }
    }
    #endregion

    #region 波次逻辑
    /// <summary>
    /// 生成指定波次的敌人
    /// </summary>
    private void SpawnWave(int waveIndex)
    {
        int spawnCount = WaveCounts[waveIndex];
        // 从 spawnPoints 中随机选取 spawnCount 个不重复的点
        List<Vector3> positions = PickRandomSpawnPositions(spawnCount);

        for (int i = 0; i < positions.Count; i++)
        {
            Spawn(positions[i]);
        }
    }

    /// <summary>
    /// 从 spawnPoints 中随机选取 count 个不重复的位置
    /// </summary>
    private List<Vector3> PickRandomSpawnPositions(int count)
    {
        List<Vector3> available = new();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            available.Add(spawnPoints[i].position);
        }

        // Fisher-Yates 洗牌后取前 count 个
        for (int i = 0; i < available.Count - 1 && i < count; i++)
        {
            int j = UnityEngine.Random.Range(i, available.Count);
            // Vector3 temp = available[i];
            // available[i] = available[j];
            // available[j] = temp;

            // 元组解构赋值（Tuple deconstruction assignment）
            (available[i], available[j]) = (available[j], available[i]);
        }

        int resultCount = Mathf.Min(count, available.Count);
        return available.GetRange(0, resultCount);
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

        // 当前波次已清空
        if (activeEnemies.Count == 0)
        {
            currentWave++;
            if (currentWave >= WaveCounts.Length)
            {
                // 第三波清空 → 副本通关
                OnDungeonClear?.Invoke();
            }
            else
            {
                // 还有下一波 → 等待间隔后生成
                isWaitingForNextWave = true;
                waveTimer = 0f;
            }
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
