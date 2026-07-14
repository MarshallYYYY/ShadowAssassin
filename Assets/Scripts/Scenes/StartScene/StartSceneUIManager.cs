using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneUIManager : MonoBehaviour
{
    // private void Awake()
    private void Start()
    {
        InitIndexPanel();
        InitBackButtons();
        InitLoadGamePanel();
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(false);
    }
    void OnEnable()
    {
        for (int i = 0; i < PersistentConstants.SaveSlotCount; i++)
        {
            SaveSlot saveSlot = saveSlotGroup.GetChild(i).GetComponent<SaveSlot>();
            saveSlot.ContentButtonClicked += OnContentButtonClicked;
            saveSlot.DeleteButtonClicked += OnDeleteButtonClicked;
        }
    }
    void OnDisable()
    {
        for (int i = 0; i < PersistentConstants.SaveSlotCount; i++)
        {
            SaveSlot saveSlot = saveSlotGroup.GetChild(i).GetComponent<SaveSlot>();
            saveSlot.ContentButtonClicked -= OnContentButtonClicked;
            saveSlot.DeleteButtonClicked -= OnDeleteButtonClicked;
        }
    }
    private void OnDestroy()
    {
        // if (titleText != null)
        // {
        //     DOTween.Kill(titleText);
        // }
        titleTextTween?.Kill();
    }
    #region Index Panel
    private void InitIndexPanel()
    {
        titleTextTween = titleText.DOFade(0.3f, 2f).SetLoops(-1, LoopType.Yoyo);
        tipText.gameObject.SetActive(false);
        InitButtonGroup();
    }
    [Header("Index Panel")]
    [SerializeField] private Text titleText;
    private Tween titleTextTween;
    [SerializeField] private Text tipText;

    #region Button Group
    [Header("Button Group")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button aboutButton;
    private void InitButtonGroup()
    {
        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        continueGameButton.onClick.AddListener(OnContinueGameButtonClicked);
        loadGameButton.onClick.AddListener(OnLoadGameButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        aboutButton.onClick.AddListener(OnAboutButtonClicked);

        SetContinueAndLoadButton();
    }
    /// <summary>
    /// 设置 继续游戏按钮 和 加载游戏按钮 的可交互性
    /// </summary>
    private void SetContinueAndLoadButton()
    {
        continueGameButton.interactable = PersistentService.Instance.GetLastSelectedIndex() != 0;

        loadGameButton.interactable = false;
        for (int i = 0; i < PersistentConstants.SaveSlotCount; i++)
        {
            if (PersistentService.Instance.IsExistsSaveFile(i + 1))
            {
                // 只要有一个存档json文件存在，那么就可以点击 加载游戏 按钮。
                loadGameButton.interactable = true;
                break;
            }
        }
    }

    private void OnNewGameButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        for (int i = 0; i < PersistentConstants.SaveSlotCount; i++)
        {
            int index = i + 1;
            // 如果 SaveSlotN.json 不存在，那么就创建
            if (PersistentService.Instance.IsExistsSaveFile(index) is false)
            {
                // 新建玩家数据和存档json
                EnterGame(index, true);
                return;
            }
        }
        // 存档槽已满，打开提示弹窗
        ShowTipText();
    }
    private void EnterGame(int index, bool isNewGame = false)
    {
        PersistentService.Instance.SetLastSelectedIndex(index);
        PersistentService.Instance.SetStartGameTimeOnEnterGame(index);
        PersistentService.Instance.SetPlayData(index, isNewGame);
        SceneLoadService.Instance.LoadScene(
            SceneLoadConstants.VillageScene,
            () => AudioService.Instance.PlayBgm(AudioConstants.BgmVillageScene));
    }
    private Tween delayTween = null;
    private void ShowTipText()
    {
        delayTween?.Kill();
        tipText.DOFade(1f, 0f);
        tipText.gameObject.SetActive(true);
        tipText.DOFade(0f, 2f);
        // 也可以使用协程
        delayTween = DOVirtual.DelayedCall(2f, () => tipText.gameObject.SetActive(false));
    }
    private void OnContinueGameButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        // LastSelectedSaveSlotIndex 在 PersistenceService 读取 GameConfig 文件的时候已经设置好了
        // 这里也可以不调用 SetLastSelectedIndex()
        int index = PersistentService.Instance.GetLastSelectedIndex();
        EnterGame(index);
    }

    private void OnLoadGameButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        loadGamePanel.SetActive(true);
    }

    private void OnSettingsButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        settingsPanel.SetActive(true);
    }

    private void OnAboutButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        aboutPanel.SetActive(true);
    }
    #endregion

    #endregion
    #region 三个 Panel 的返回按钮
    [Header("三个 Panel 的返回按钮")]
    [SerializeField] private Button[] backButtons = new Button[3];
    private void OnBackButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        loadGamePanel.SetActive(false);
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(false);
    }
    private void InitBackButtons()
    {
        for (int i = 0; i < backButtons.Length; i++)
        {
            Button button = backButtons[i];
            button.onClick.AddListener(OnBackButtonClicked);
        }
    }
    #endregion

    #region Load Game Panel
    [Header("Load Game Panel")]
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private Transform saveSlotGroup;
    [SerializeField] private GameObject deleteConfirm;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    private void InitLoadGamePanel(bool isRefresh = false)
    {
        if (isRefresh is false)
        {
            loadGamePanel.SetActive(false);
            deleteConfirm.SetActive(false);
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        for (int i = 0; i < PersistentConstants.SaveSlotCount; i++)
        {
            SaveSlot saveSlot = saveSlotGroup.GetChild(i).GetComponent<SaveSlot>();
            saveSlot.RefreshUI();
        }
    }

    #region 单个的SaveSlot
    /// <summary>
    /// 读取存档数据，进入游戏场景
    /// </summary>
    /// <param name="index"></param>
    /// <param name="path"></param>
    private void OnContentButtonClicked(int index)
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        EnterGame(index);
    }
    private int deleteSaveSlotIndex = 0;
    private void OnDeleteButtonClicked(int index)
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        deleteConfirm.SetActive(true);
        deleteSaveSlotIndex = index;
    }
    #endregion
    private void OnConfirmButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIConfirm);
        int index = deleteSaveSlotIndex;
        // 如果要删除的存档是上次使用的存档，那么设置 LastSelectedSaveSlot 为 0，这会让 继续游戏按钮 不可点击。
        if (PersistentService.Instance.GetLastSelectedIndex() == index)
        {
            PersistentService.Instance.SetLastSelectedIndex(0);
        }
        PersistentService.Instance.DeleteSaveSlotFile(index);

        InitLoadGamePanel(true);
        SetContinueAndLoadButton();

        deleteConfirm.SetActive(false);
    }
    private void OnCancelButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UICancel);
        deleteConfirm.SetActive(false);
    }
    #endregion

    #region Settings Panel
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    #endregion

    #region About Panel
    [Header("About Panel")]
    [SerializeField] private GameObject aboutPanel;
    #endregion
}
