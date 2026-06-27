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
        int index = PersistentService.Instance.GetLastSelectedIndex();
        PersistentService.Instance.SetGameTime(index);
    }
    private void OnBackMainMenuButtonClicked()
    {
        OnSaveGameButtonClicked();
        SceneLoadService.Instance.LoadScene(SceneLoadConstants.StartScene);
    }

    private void OnQuitGameButtonClicked()
    {
#if UNITY_EDITOR
        // 编辑器模式下：停止播放
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 打包后：退出应用程序
        Application.Quit();
#endif
    }
}