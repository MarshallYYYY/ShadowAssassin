using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/InventorySO"))]
public class InventoryItemDataBaseSO : ScriptableObject
{
    public List<InventoryItemSO> Items = new();
}

[Serializable]
public class InventoryItemSO
{
    public string Name;
    public int Id;
    public string Description;
    public Sprite Sprite;
    public int Price;
}