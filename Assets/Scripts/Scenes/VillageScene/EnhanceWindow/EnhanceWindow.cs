using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceWindow : MonoBehaviour
{
    [SerializeField] private Button backButton;
    #region 左侧的装备列表
    [SerializeField] private Transform equipmentsTransform;
    private readonly List<EquipmentItemUI> equipmentItemUIs = new();
    #endregion
    #region Enhance Level
    [Header("Enhance Level")]
    [SerializeField] private Image equipmentIconImage;
    [SerializeField] private Transform starGroupTransform;
    #endregion
    #region Enhance Data
    [Header("Enhance Data")]
    [SerializeField] private Text hpText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text defenseText;
    #endregion
    #region Cost
    [Header("Cost")]
    [SerializeField] private Text costGoldCoinCountText;
    [SerializeField] private Text ownedGoldCoinText;
    [SerializeField] private Transform costItemGroupTransform;
    [SerializeField] private GameObject costItemUIPrefab;
    #endregion
    #region Enhance Button
    [Header("Enhance Button")]
    [SerializeField] private Button enhanceButton;
    #endregion
    #region Data
    private Equipment currentEquipment;
    private EquipmentSO currentEquipmentSO;
    #endregion
    void Awake()
    {
        backButton.onClick.AddListener(() => gameObject.SetActive(false));

        List<Equipment> equipments = PersistentService.Instance.GetAllEquipment();
        for (int i = 0; i < equipmentsTransform.childCount; i++)
        {
            EquipmentItemUI equipmentItemUI = equipmentsTransform.GetChild(i).GetComponent<EquipmentItemUI>();
            equipmentItemUI.Init(equipments[i]);
            equipmentItemUI.EquipmentSelected += OnEquipmentSelected;
            equipmentItemUIs.Add(equipmentItemUI);
        }

        enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);
    }
    /// <summary>
    /// 强化按钮的点击事件（按钮仅在金币和材料均足够时方可点击）：
    /// 扣除PlayerData中的金币和物品，增加目标装备的强化等级，最后刷新右侧内容
    /// </summary>
    private void OnEnhanceButtonClicked()
    {
        if (currentEquipment == null || currentEquipmentSO == null)
            return;

        if (currentEquipment.EnhanceLevel >= currentEquipmentSO.EquipmentEnhanceInfos.Length - 1)
            return;

        EquipmentEnhanceLevelInfo nextLevel = currentEquipmentSO.EquipmentEnhanceInfos[currentEquipment.EnhanceLevel + 1];

        // 扣除金币
        int currentGold = PersistentService.Instance.GetGoldCoin();
        PersistentService.Instance.SetGoldCoin(currentGold - nextLevel.GoldCoinCost);

        // 扣除材料
        for (int i = 0; i < nextLevel.CostItems.Count; i++)
        {
            CostItem costItem = nextLevel.CostItems[i];
            PersistentService.Instance.RemoveInventoryItemById(costItem.InventoryItemSO.Id, costItem.Count);
        }

        // 强化
        currentEquipment.EnhanceLevel++;

        // 刷新左侧UI：
        // 这里调用了所有装备的刷新，按逻辑来说应该只调用正在强化的装备的刷新，
        // 但刷新 6 个和刷 1 个没有任何性能差异（只是 SetActive(bool) 而已）。如果坚持只刷当前选中的，需要多增加一个字段，
        // 说实话，6 个 SetActive 的总代价小于一次 Find 的遍历——ForEach 反而更快。建议保持：
        equipmentItemUIs.ForEach(itemUI => itemUI.Refresh(false));
        // 刷新右侧UI
        OnEquipmentSelected(currentEquipment);

        PersistentService.Instance.AddQuestProgress(QuestCodeConstants.Enhance);
    }
    #region EquipmentItemUI 相关
    void OnEnable()
    {
        ownedGoldCoinText.text = $"({PersistentService.Instance.GetGoldCoin()})";
        // for (int i = 0; i < equipmentItemUIs.Count; i++)
        // {
        //     equipmentItemUIs[i].Refresh();
        // }
        equipmentItemUIs.ForEach(itemUI => itemUI.Refresh(true));
    }

    /// <summary>
    /// EquipmentItemUI 中点击某个具体的装备图标后的回调函数（事件处理函数）：刷新右侧内容
    /// </summary>
    /// <param name="equipment"></param>
    private void OnEquipmentSelected(Equipment equipment)
    {
        // 获取数据并存储为字段
        currentEquipment = equipment;
        currentEquipmentSO = StaticDataService.Instance.GetEquipmentSO(equipment.EquipmentName);
        if (currentEquipment == null || currentEquipmentSO == null) return;

        // 设置右侧（EnhanceInfo）当前装备的当前强化等级的图标和星级
        equipmentIconImage.sprite = currentEquipmentSO.Sprite;
        for (int i = 0; i < starGroupTransform.childCount; i++)
        {
            starGroupTransform.GetChild(i).gameObject.SetActive(i < currentEquipment.EnhanceLevel);
        }

        // 满级判断
        bool isMaxLevel = currentEquipment.EnhanceLevel >= currentEquipmentSO.EquipmentEnhanceInfos.Length - 1;

        // 设置右上方装备升级的属性变化
        EquipmentEnhanceLevelInfo currentLevelInfo = currentEquipmentSO.EquipmentEnhanceInfos[currentEquipment.EnhanceLevel];
        if (isMaxLevel)
        {
            hpText.text = $"生命值： + {currentLevelInfo.HP}";
            attackText.text = $"攻击力： + {currentLevelInfo.Attack}";
            defenseText.text = $"防御值： + {currentLevelInfo.Defense}";
            costGoldCoinCountText.text = "-";
            ClearCostItems();
            enhanceButton.interactable = false;
            return;
        }
        EquipmentEnhanceLevelInfo nextLevelInfo = currentEquipmentSO.EquipmentEnhanceInfos[currentEquipment.EnhanceLevel + 1];
        hpText.text = $"生命值： + {currentLevelInfo.HP}  →  + {nextLevelInfo.HP}";
        attackText.text = $"攻击力： + {currentLevelInfo.Attack}  →  + {nextLevelInfo.Attack}";
        defenseText.text = $"防御值： + {currentLevelInfo.Defense}  →  + {nextLevelInfo.Defense}";

        // 设置右下角装备升级所需的金币
        costGoldCoinCountText.text = nextLevelInfo.GoldCoinCost.ToString();
        // 当前金币
        int ownedGoldCoin = PersistentService.Instance.GetGoldCoin();
        ownedGoldCoinText.text = $"({ownedGoldCoin})";
        bool isGoldCoinEnough = ownedGoldCoin >= nextLevelInfo.GoldCoinCost;

        // 设置右下角装备升级所需的材料
        bool isMaterialsEnough = true;
        ClearCostItems();
        for (int i = 0; i < nextLevelInfo.CostItems.Count; i++)
        {
            CostItem costItem = nextLevelInfo.CostItems[i];
            int ownedCount = PersistentService.Instance.GetInventoryItemCount(costItem.InventoryItemSO.Id);

            CostItemUI costItemUI = Instantiate(costItemUIPrefab, costItemGroupTransform).GetComponent<CostItemUI>();
            costItemUI.Refresh(costItem, ownedCount);

            if (ownedCount < costItem.Count)
                isMaterialsEnough = false;
        }

        // 金币和材料都足够才可点击强化按钮
        enhanceButton.interactable = isGoldCoinEnough && isMaterialsEnough;
    }
    private void ClearCostItems()
    {
        for (int i = costItemGroupTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(costItemGroupTransform.GetChild(i).gameObject);
        }
    }
    #endregion
}