using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueWindow : MonoBehaviour
{
    #region Setence UI
    [Header("Setence")]
    [SerializeField] private Text npcNameText;
    [SerializeField] private Text setenceText;
    [SerializeField] private Button nextButton;
    #endregion
    #region KnightPanel
    [Header("KnightPanel")]
    [SerializeField] private GameObject knightPanel;
    [SerializeField] private Button departButton;
    [SerializeField] private Button cancelKnightButton;
    #endregion

    #region MerchantPanel
    [Header("MerchantPanel")]
    [SerializeField] private GameObject merchantPanel;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button blindBoxButton;
    [SerializeField] private Button cancelMerchantButton;
    [SerializeField] private GameObject purchaseWindow;
    [SerializeField] private GameObject blindBoxWindow;
    #endregion
    #region BlacksmithPanel
    [Header("BlacksmithPanel")]
    [SerializeField] private GameObject blacksmithPanel;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button cancelBlacksmithButton;
    [SerializeField] private GameObject equipmentEnhanceWindow;
    #endregion
    #region Setences Data
    private int setenceIndex = 0;
    private List<string> setences;
    private DialogueSO dialogueSO;
    #endregion
    #region 外部可访问
    public event Action OnDialogueEnd;
    public void Init()
    {
        purchaseWindow.SetActive(false);
        blindBoxWindow.SetActive(false);

        equipmentEnhanceWindow.SetActive(false);

        knightPanel.SetActive(false);
        merchantPanel.SetActive(false);
        blacksmithPanel.SetActive(false);
    }
    public void SetDialogueContent(DialogueSO dialogueSO)
    {
        this.dialogueSO = dialogueSO;
        GameManager.Instance.SwitchToUIMode();
        setenceIndex = 0;
        setences = new(dialogueSO.Sentences);

        npcNameText.text = dialogueSO.NpcName;
        setenceText.text = setences[setenceIndex];
    }
    #endregion
    void Awake()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);

        departButton.onClick.AddListener(() =>
        {
            AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
            SceneLoadService.Instance.LoadScene(
                SceneLoadConstants.DungeonScene,
                () => AudioService.Instance.PlayBgm(AudioConstants.BgmDungeonScene));
            GameManager.Instance.SwitchToPlayMode();
        });
        cancelKnightButton.onClick.AddListener(() =>
        {
            AudioService.Instance.PlaySfx(AudioConstants.UICancel);
            knightPanel.SetActive(false);
            EndDialogue();
        });

        purchaseButton.onClick.AddListener(() =>
        {
            AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
            purchaseWindow.SetActive(true);
        });
        blindBoxButton.onClick.AddListener(() =>
        {
            AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
            blindBoxWindow.SetActive(true);
        });
        cancelMerchantButton.onClick.AddListener(() =>
        {
            AudioService.Instance.PlaySfx(AudioConstants.UICancel);
            merchantPanel.SetActive(false);
            EndDialogue();
        });

        enhanceButton.onClick.AddListener(() =>
        {
            AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
            equipmentEnhanceWindow.SetActive(true);
        });
        cancelBlacksmithButton.onClick.AddListener(() =>
        {
            AudioService.Instance.PlaySfx(AudioConstants.UICancel);
            blacksmithPanel.SetActive(false);
            EndDialogue();
        });
    }

    #region Button Events  
    private void OnNextButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        setenceIndex++;
        // 如果句子索引不等于总数量
        if (setenceIndex != setences.Count)
        {
            setenceText.text = setences[setenceIndex];
        }
        // 最后一个句子已经说完了
        else
        {
            switch (dialogueSO.NpcGameObjectName)
            {
                case NpcGameObjectNameConstants.King:
                    EndDialogue();
                    break;
                case NpcGameObjectNameConstants.Knight:
                    knightPanel.SetActive(true);
                    break;
                case NpcGameObjectNameConstants.Merchant:
                    merchantPanel.SetActive(true);
                    break;
                case NpcGameObjectNameConstants.Blacksmith:
                    blacksmithPanel.SetActive(true);
                    break;
            }
        }
    }
    private void EndDialogue()
    {
        OnDialogueEnd?.Invoke();
        GameManager.Instance.SwitchToPlayMode();
        gameObject.SetActive(false);
    }
    #endregion


}
