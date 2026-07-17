using UnityEngine;
using UnityEngine.UI;

public class SystemPage : MonoBehaviour
{
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button backMainMenuButton;
    [SerializeField] private Button quitGameButton;
    void Awake()
    {
        saveGameButton.onClick.AddListener(OnSaveGameButtonClicked);
        backMainMenuButton.onClick.AddListener(OnBackMainMenuButtonClicked);
        quitGameButton.onClick.AddListener(OnQuitGameButtonClicked);
    }
    private void OnSaveGameButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        int index = PersistentService.Instance.GetLastSelectedIndex();
        PersistentService.Instance.SaveGameTime(index);
    }
    private void OnBackMainMenuButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        OnSaveGameButtonClicked();
        SceneLoadService.Instance.LoadScene(SceneLoadConstants.StartScene, OnSceneLoaded);
        static void OnSceneLoaded()
        {
            AudioService.Instance.PlayBgm(AudioConstants.BgmStartScene);
            GameManager.Instance.SwitchToUIMode();
        }
    }

    private void OnQuitGameButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
#if UNITY_EDITOR
        // 编辑器模式下：停止播放
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 打包后：退出应用程序
        Application.Quit();
#endif
    }
}