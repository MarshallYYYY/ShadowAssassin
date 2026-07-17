using UnityEngine;
using UnityEngine.UI;

public class DungeonSceneUIManager : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("DeathWindow")]
    [SerializeField] private GameObject deathWindow;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button backVillageButton;

    [Header("VictoryWindow")]
    [SerializeField] private GameObject victoryWindow;
    [SerializeField] private Button victoryBackVillageButton;

    void Awake()
    {
        deathWindow.SetActive(false);
        retryButton.onClick.AddListener(OnRetryButtonClicked);
        backVillageButton.onClick.AddListener(BackVillageScene);

        victoryWindow.SetActive(false);
        victoryBackVillageButton.onClick.AddListener(BackVillageScene);
    }
    private void OnRetryButtonClicked()
    {
        SceneLoadService.Instance.LoadScene(SceneLoadConstants.DungeonScene, OnSceneLoaded);
        static void OnSceneLoaded()
        {
            AudioService.Instance.PlayBgm(AudioConstants.BgmDungeonScene);
            GameManager.Instance.SwitchToPlayMode();
        }
    }
    private void BackVillageScene()
    {
        SceneLoadService.Instance.LoadScene(SceneLoadConstants.VillageScene, OnSceneLoaded);
        static void OnSceneLoaded()
        {
            AudioService.Instance.PlayBgm(AudioConstants.BgmVillageScene);
            GameManager.Instance.SwitchToPlayMode();
        }
    }
    void OnEnable()
    {
        playerController.OnPlayerDeath += ShowDeathWindow;
        enemySpawner.OnDungeonClear += ShowVictoryWindow;
    }

    void OnDisable()
    {
        playerController.OnPlayerDeath -= ShowDeathWindow;
        enemySpawner.OnDungeonClear -= ShowVictoryWindow;
    }

    private void ShowDeathWindow()
    {
        GameManager.Instance.SwitchToUIMode();
        deathWindow.SetActive(true);
    }

    private void ShowVictoryWindow()
    {
        GameManager.Instance.SwitchToUIMode();
        victoryWindow.SetActive(true);

        PersistentService.Instance.AddQuestProgress(QuestCodeConstants.Dungeon);
        PersistentService.Instance.AddReward(1000, 1000);
    }
}
