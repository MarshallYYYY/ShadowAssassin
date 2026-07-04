using UnityEngine;
// [Serializable]
// public class InventoryItemSO
[CreateAssetMenu(menuName = "Scriptable Objects/InventoryItemSO")]
public class InventoryItemSO : ScriptableObject
{
    public string Name;
    public int Id;
    public string Description;
    public Sprite Sprite;
    public int Price;
}