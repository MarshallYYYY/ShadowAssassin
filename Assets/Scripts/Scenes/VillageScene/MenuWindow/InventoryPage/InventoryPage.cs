using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPage : MonoBehaviour
{
    #region GoldCoin
    [Header("GoldCoin")]
    [SerializeField] private Text goldCoinText;
    #endregion
    #region ScrollView
    [Header("ScrollView")]
    [SerializeField] private GameObject inventoryItemUIPrefab;
    [SerializeField] private GameObject scrollView;
    #endregion
    #region DetailPanel
    [Header("DetailPanel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button discardButton;
    #endregion
    #region DiscardItemPanel
    [Header("DiscardItemPanel")]
    [SerializeField] private GameObject discardItemPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    #endregion
    #region Data
    private string currentSelectedItemUid;
    public string CurrentSelectedItemUid
    {
        get => currentSelectedItemUid;
        set
        {
            currentSelectedItemUid = value;
            RefreshDetailPanel();
        }
    }
    #endregion

    void Awake()
    {
        discardButton.onClick.AddListener(OnDiscardButtonClicked);

        confirmButton.onClick.AddListener(OnDiscardConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }
    void OnEnable()
    {
        RefreshUI();
    }
    /// <summary>
    /// DeepSeekV4Pro：Unity 官方原则：Awake：初始化自己。Start：初始化与其他对象的交互。
    /// 该函数放在Awake()中，会导致运行到InventoryItemUI的Refresh的关于stars的for循环时，报空引用。
    /// 放在Start()中，一切正常。
    /// </summary>
    private void RefreshUI()
    {
        goldCoinText.text = PersistentService.Instance.GetGoldCoin().ToString();
        RefreshScrollView();
        detailPanel.SetActive(false);
        discardItemPanel.SetActive(false);
    }
    private const int MinCount = 28;
    private void RefreshScrollView()
    {
        // 清理滚动容器中原本的物品
        RectTransform scrollContent = scrollView.GetComponent<ScrollRect>().content;
        for (int i = 0; i < scrollContent.childCount; i++)
        {
            Destroy(scrollContent.GetChild(i).gameObject);
        }
        // foreach (InventoryItem item in PersistentService.Instance.GetSortedAllInventoryItem())
        // {
        //     Transform inventoryItemUITransform = Instantiate(inventoryItemUIPrefab.transform, scrollContent);
        //     InventoryItemUI itemUI = inventoryItemUITransform.GetComponent<InventoryItemUI>();
        //     itemUI.Refresh(item, this);
        // }
        List<InventoryItem> items = PersistentService.Instance.GetSortedAllInventoryItem();
        int slotCount = Mathf.Max(MinCount, items.Count);
        for (int i = 0; i < slotCount; i++)
        {
            Transform itemUITransform = Instantiate(inventoryItemUIPrefab.transform, scrollContent);
            InventoryItemUI itemUI = itemUITransform.GetComponent<InventoryItemUI>();

            if (i < items.Count)
            {
                // 有物品：正常显示
                itemUI.Refresh(items[i], this);
            }
            else
            {
                // 空槽：只有背景
                itemUI.SetEmpty();
            }
        }
    }
    private void RefreshDetailPanel()
    {
        if (detailPanel.activeSelf is false)
        {
            detailPanel.SetActive(true);
        }
        // 找到uid对应的动态数据
        InventoryItem inventoryItem = PersistentService.Instance.GetInventoryItem(currentSelectedItemUid);
        // 刷新详情界面
        InventoryItemSO itemSO = StaticDataService.Instance.GetInventoryItemSO(inventoryItem.Id);
        itemNameText.text = itemSO.Name;
        iconImage.sprite = itemSO.Sprite;
        descriptionText.text = itemSO.Description;
    }
    #region Button Events

    private void OnDiscardButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        discardItemPanel.SetActive(true);
    }


    private void OnCancelButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UICancel);
        discardItemPanel.SetActive(false);
    }

    private void OnDiscardConfirmButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIConfirm);
        // 找到uid对应的动态数据
        InventoryItem inventoryItem = PersistentService.Instance.GetInventoryItem(currentSelectedItemUid);
        PersistentService.Instance.DiscardInventoryItem(inventoryItem);
        RefreshUI();
    }
    #endregion
}