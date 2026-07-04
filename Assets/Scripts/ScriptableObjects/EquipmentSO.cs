using System;
using System.Collections.Generic;
using UnityEngine;

// 一个装备类型 → 一个 .asset
[CreateAssetMenu(menuName = "Scriptable Objects/EquipmentSO")]
public class EquipmentSO : ScriptableObject
{
    public string EquipmentName;
    public Sprite Sprite;
    /// <summary>
    /// 索引即强化等级（0→基础, 1→+1, 2→+2, 3→+3）
    /// CostItems = 强化到该等级所需消耗（0级无消耗）
    /// </summary>
    // public EquipmentEnhanceLevelInfo[] EquipmentEnhanceInfos = new EquipmentEnhanceLevelInfo[4];
    public EquipmentEnhanceLevelInfo[] EquipmentEnhanceInfos;
}

/// <summary>
/// 装备强化等级信息：既包含装备属性，又包含金币和材料的消耗情况
/// </summary>
[Serializable]
public class EquipmentEnhanceLevelInfo
{
    [Header("属性")]
    public int HP;
    public int Attack;
    public int Defense;
    [Header("金币和物品消耗")]
    public int GoldCoinCost;
    public List<CostItem> CostItems = new();
}

[Serializable]
public class CostItem
{
    public InventoryItemSO InventoryItemSO;
    public int Count;
}