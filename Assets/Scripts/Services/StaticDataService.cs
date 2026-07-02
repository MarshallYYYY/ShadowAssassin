using System.Collections.Generic;
using UnityEngine;

public class StaticDataService : BaseService<StaticDataService>
{
    public override void Init()
    {
        base.Init();
    }
    #region Inventory
    [SerializeField] private InventoryItemDataBaseSO inventoryItemDataBaseSO;
    public List<InventoryItemSO> GetAllInventoryItemSO()
    {
        return inventoryItemDataBaseSO.Items;
    }
    public InventoryItemSO GetInventoryItemSO(int id)
    {
        // foreach (InventoryItemSO itemSO in inventorySO.Items)
        // {
        //     if (itemSO.Id == id)
        //     {
        //         return itemSO;
        //     }
        // }
        // return null;
        return inventoryItemDataBaseSO.Items.Find(itemSO => itemSO.Id == id);
    }
    #endregion
    #region Dialogue
    [SerializeField] private DialogueDataBaseSO dialogueDataBaseSO;
    public DialogueSO GetDialogueSO(string npcGameObjectName)
    {
        return dialogueDataBaseSO.Dialogues.Find(dialogueSO => dialogueSO.NpcGameObjectName == npcGameObjectName);
    }
    #endregion
    #region Quest
    [SerializeField] private QuestDataBaseSO questDataBaseSO;
    public List<QuestSO> GetAllQuestSO()
    {
        return questDataBaseSO.Quests;
    }
    public QuestSO GetQuestSO(string questCode)
    {
        return questDataBaseSO.Quests.Find(questSO => questSO.QuestCode == questCode);
    }
    #endregion
}