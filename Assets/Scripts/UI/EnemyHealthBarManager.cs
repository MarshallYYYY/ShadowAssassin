using UnityEngine;

/// <summary>
/// 敌人血条管理器：挂在 Canvas 上，订阅 PlayerController.OnHitEnemyEvent。
/// 跟踪最近攻击的敌人，实时更新其血条；敌人死亡时闪烁血条后隐藏。
/// </summary>
public class EnemyHealthBarManager : MonoBehaviour
{
    #region SerializeField
    [SerializeField] private HealthBarController enemyHealthBar;
    [SerializeField] private PlayerController playerController;
    #endregion

    #region 数据
    private EnemyController currentEnemy;
    private bool isFlashing;
    #endregion

    #region 生命周期
    void Start()
    {
        // 隐藏血条
        enemyHealthBar?.Hide();

        // 订阅玩家命中事件
        if (playerController != null)
        {
            playerController.OnHitEnemyEvent += OnHitEnemy;
        }
        else
        {
            // 自动查找
            playerController = Object.FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.OnHitEnemyEvent += OnHitEnemy;
            }
        }
    }

    void Update()
    {
        if (isFlashing || currentEnemy == null)
            return;

        // 持续更新当前敌人的血条
        enemyHealthBar.SetHP(currentEnemy.CurrentHP, currentEnemy.MaxHP);
    }

    void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnHitEnemyEvent -= OnHitEnemy;
        }
    }
    #endregion

    #region 内部回调
    private void OnHitEnemy(EnemyController enemy)
    {
        if (enemy == null)
            return;

        // 如果正在闪烁，不切换（等待闪烁结束）
        if (isFlashing)
            return;

        // 取消之前敌人的死亡订阅
        if (currentEnemy != null)
        {
            currentEnemy.OnDeathEvent -= OnEnemyDeath;
        }

        // 切换到新敌人
        currentEnemy = enemy;
        currentEnemy.OnDeathEvent += OnEnemyDeath;

        // 显示并更新血条
        enemyHealthBar.Show();
        enemyHealthBar.SetHP(enemy.CurrentHP, enemy.MaxHP);
    }

    private void OnEnemyDeath(EnemyController enemy)
    {
        if (currentEnemy != enemy)
            return;

        // 闪烁后隐藏
        isFlashing = true;
        enemyHealthBar.Flash();

        // 延迟清除引用（等闪烁动画播完）
        this.Invoke(nameof(ClearCurrentEnemy), 0.1f);
    }

    private void ClearCurrentEnemy()
    {
        currentEnemy = null;
        isFlashing = false;
    }
    #endregion
}
