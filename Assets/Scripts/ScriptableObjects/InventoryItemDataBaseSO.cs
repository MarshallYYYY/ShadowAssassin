using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/InventorySO"))]
public class InventoryItemDataBaseSO : ScriptableObject
{
    public List<InventoryItemSO> Items = new();
}