using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoodsUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Text quantityText;
    [SerializeField] private Button plusButton;
    private int itemSOId;
    private int price;
    private int quantity = 0;
    private const int MaxQuantity = 99;
    // 商品对应的 ItemSO Id
    #region 外部访问
    public int ItemSOId => itemSOId;
    public int Price => price;
    public int Quantity => quantity;
    public void Init(InventoryItemSO itemSO)
    {
        iconImage.sprite = itemSO.Sprite;
        nameText.text = itemSO.Name;
        priceText.text = itemSO.Price.ToString();

        itemSOId = itemSO.Id;
        price = itemSO.Price;
    }
    public void Refresh()
    {
        minusButton.interactable = false;
        quantityText.text = "0";

        quantity = 0;
    }
    #endregion
    void Awake()
    {
        minusButton.onClick.AddListener(() => SetQuantity(quantity - 1));
        plusButton.onClick.AddListener(() => SetQuantity(quantity + 1));
    }
    private void SetQuantity(int value)
    {
        quantity = Mathf.Clamp(value, 0, MaxQuantity);
        quantityText.text = quantity.ToString();
        minusButton.interactable = quantity > 0;
        plusButton.interactable = quantity < MaxQuantity;
    }
}
