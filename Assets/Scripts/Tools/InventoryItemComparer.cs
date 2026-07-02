using System.Collections.Generic;

public class InventoryItemComparer : IComparer<InventoryItem>
{
    public int Compare(InventoryItem x, InventoryItem y)
    {
        InventoryItemSO itemSO1 = StaticDataService.Instance.GetInventoryItemSO(x.Id);
        InventoryItemSO itemSO2 = StaticDataService.Instance.GetInventoryItemSO(y.Id);
        // 按照 Id 从大到小排序
        // 1, 0, -1
        int idComparison = itemSO2.Id.CompareTo(itemSO1.Id);
        return idComparison;
    }
}