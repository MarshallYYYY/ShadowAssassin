using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseWindow : MonoBehaviour
{
    #region UI
    [SerializeField] private Button backButton;
    [SerializeField] private Text goldCoinText;
    [SerializeField] private Transform merchantGoods;
    [SerializeField] private GameObject goodsPrefab;
    [SerializeField] private Button purchaseButton;
    #endregion
    #region UI - TipPanel
    [Header("TipPanel")]
    [SerializeField] private GameObject tipPanel;
    [SerializeField] private Text tipText;
    [SerializeField] private Button confirmButton;
    #endregion
    #region Data
    private readonly List<GoodsUI> goodsUIs = new();
    #endregion
    void Awake()
    {
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
        goldCoinText.text = PersistentService.Instance.GetGoldCoin().ToString();
        InitMerchantGoods();
        purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);

        tipPanel.SetActive(false);
        confirmButton.onClick.AddListener(() => tipPanel.SetActive(false));
    }
    private void InitMerchantGoods()
    {
        // 清空初始子物体（场景中摆的占位模板）
        for (int i = merchantGoods.childCount - 1; i >= 0; i--)
        {
            Destroy(merchantGoods.GetChild(i).gameObject);
        }
        List<InventoryItemSO> inventoryItemSOs = StaticDataService.Instance.GetAllInventoryItemSO();
        // 倒序是为了让价格更便宜的物品在前面：森林藤蔓、黑色矿石、银色矿石
        for (int i = inventoryItemSOs.Count - 1; i >= 0; i--)
        {
            InventoryItemSO itemSO = inventoryItemSOs[i];
            if (itemSO.Price != -1f)
            {
                // Debug.Log(itemSO.Name + "   " + itemSO.Price);
                GoodsUI goodsUI = Instantiate(goodsPrefab, merchantGoods).GetComponent<GoodsUI>();
                goodsUI.Init(itemSO);
                goodsUIs.Add(goodsUI);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(merchantGoods as RectTransform);
    }
    void OnEnable()
    {
        goodsUIs.ForEach(goodsUI => goodsUI.Refresh());
        goldCoinText.text = PersistentService.Instance.GetGoldCoin().ToString();
    }
    private void OnPurchaseButtonClicked()
    {
        int totalPrice = 0;
        for (int i = 0; i < goodsUIs.Count; i++)
        {
            totalPrice += goodsUIs[i].Price * goodsUIs[i].Quantity;
        }
        if (totalPrice == 0)
        {
            ShowTipPanel("没有选择任何物品！");
            return;
        }

        int goldCoin = PersistentService.Instance.GetGoldCoin();
        Debug.Log(totalPrice + ", " + goldCoin);
        if (goldCoin < totalPrice)
        {
            ShowTipPanel("金币好像不够了呢");
            return;
        }

        // 扣钱 + 加物品到背包
        PersistentService.Instance.SetGoldCoin(goldCoin - totalPrice);
        goldCoinText.text = PersistentService.Instance.GetGoldCoin().ToString();
        foreach (GoodsUI goodsUI in goodsUIs)
        {
            for (int i = 0; i < goodsUI.Quantity; i++)
            {
                InventoryItem item = new()
                {
                    Uid = Guid.NewGuid().ToString(),
                    Id = goodsUI.ItemSOId,
                };
                PersistentService.Instance.AddInventoryItem(item);
            }
        }
        PersistentService.Instance.AddQuestProgress(QuestCodeConstants.Purchase);
        ShowTipPanel("购买成功！");
    }
    private void ShowTipPanel(string content)
    {
        tipText.text = content;
        tipPanel.SetActive(true);
    }
}
