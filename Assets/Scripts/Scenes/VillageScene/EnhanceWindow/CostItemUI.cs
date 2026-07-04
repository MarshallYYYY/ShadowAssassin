using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CostItemUI : MonoBehaviour
{
    [SerializeField] private Image costItemIconImage;
    [SerializeField] private Text costItemNameText;
    [SerializeField] private Text costCountText;
    [SerializeField] private Text ownedText;

    public void Refresh(CostItem costItem, int ownedCount)
    {
        costItemIconImage.sprite = costItem.InventoryItemSO.Sprite;
        costItemNameText.text = costItem.InventoryItemSO.Name;
        costCountText.text = costItem.Count.ToString();
        ownedText.text = $"({ownedCount})";
    }
}
