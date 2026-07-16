using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// [DefaultExecutionOrder(-100)]
public class PersistentService : BaseService<PersistentService>
{
    public override void Init()
    {
        base.Init();
        InitGameConfig();
    }
    #region 数据
    // 写入操作直接写入持久化文件中，读取操作则通过读取内存中的变量来获取。
    #region Game Config
    private string gameConfigPath;
    private GameConfig gameConfig = new();
    #endregion

    #region Save File
    private DateTime startGameTime = new();
    private PlayerData playerData = new();
    #endregion
    #endregion

    #region GameConfig
    private void InitGameConfig()
    {
        gameConfigPath = $"{Application.persistentDataPath}/{PersistentConstants.GameConfig}.json";
        string path = gameConfigPath;
        Debug.Log(path);
        // 如果存在，就直接读取数据
        if (File.Exists(path))
        {
            string jsonStr = File.ReadAllText(path);
            // 这里只设置内存中的 globalConfig 即可，无需向 json 文件中写入。
            gameConfig = JsonConvert.DeserializeObject<GameConfig>(jsonStr);
        }
        // 如果不存在（仅第一次运行时不存在），就新建一个配置文件
        else
        {
            string jsonStr = JsonConvert.SerializeObject(new GameConfig());
            File.WriteAllText(path, jsonStr);
        }
    }

    /// <summary>
    /// 设置最后游玩时使用的存档索引
    /// </summary>
    /// <param name="index">LastSelectedSaveSlotIndex</param>
    public void SetLastSelectedIndex(int index)
    {
        gameConfig.LastSelectedSaveSlotIndex = index;
        SaveGameConfigFile();
    }
    public int GetLastSelectedIndex()
    {
        return gameConfig.LastSelectedSaveSlotIndex;
    }
    public void SetGameConfigVolume(float masterVolume, float bgmVolume, float sfxVolume)
    {
        gameConfig.MasterVolume = masterVolume;
        gameConfig.BgmVolume = bgmVolume;
        gameConfig.SfxVolume = sfxVolume;
        SaveGameConfigFile();
    }
    public void GetVolume(out float masterVolume, out float bgmVolume, out float sfxVolume)
    {
        masterVolume = gameConfig.MasterVolume;
        bgmVolume = gameConfig.BgmVolume;
        sfxVolume = gameConfig.SfxVolume;
    }
    private void SaveGameConfigFile()
    {
        string jsonStr = JsonConvert.SerializeObject(gameConfig);
        File.WriteAllText(gameConfigPath, jsonStr);
    }
    #endregion

    #region ======= PlayerData（存档文件 和 玩家数据） =======
    /// <summary>
    /// 目标索引对应的存档文件是否存在
    /// </summary>
    /// <param name="index">限制：1/2/3</param>
    /// <returns></returns>
    public bool IsExistsSaveFile(int index)
    {
        string path = GetSaveSlotPath(index);
        return File.Exists(path);
    }
    /// <summary>
    /// 通过某个具体的存档进入游戏时调用
    /// </summary>
    /// <param name="index"></param>
    /// <param name="isNewGame"></param>
    public void SetPlayData(int index, bool isNewGame = false)
    {
        if (isNewGame)
        {
            // 新游戏新建 playerData，并新建存档文件，同时将 playerData 的数据写入存档文件
            playerData = new();

            // TODO：创建一个PlayerDataSO，用来存储人物的基本属性数据，
            // 然后new的时候通过StaticDataService读取SO文件中的数据给PlayerData赋值

            List<QuestSO> questSOs = StaticDataService.Instance.GetAllQuestSO();
            for (int i = 0; i < questSOs.Count; i++)
            {
                QuestSO questSO = questSOs[i];
                Quest quest = new()
                {
                    QuestCode = questSO.QuestCode,
                    ProgressCount = 0,
                    IsClaimed = false,
                };
                playerData.Quests.Add(quest);
            }

            List<EquipmentSO> equipmentSOs = StaticDataService.Instance.GetAllEquipmentSO();
            for (int i = 0; i < equipmentSOs.Count; i++)
            {
                EquipmentSO equipmentSO = equipmentSOs[i];
                Equipment equipment = new()
                {
                    EquipmentName = equipmentSO.EquipmentName,
                    EnhanceLevel = 0,
                };
                playerData.Equipments.Add(equipment);

                // 新建Player的时候，将所有装备的初始（0强化）属性添加到playerData上。
                EquipmentEnhanceLevelInfo zeroEnhanceLevelInfo = equipmentSO.EquipmentEnhanceInfos[0];
                AddPlayerAttributes(zeroEnhanceLevelInfo.HP, zeroEnhanceLevelInfo.Attack, zeroEnhanceLevelInfo.Defense);
            }

            SaveGameTime(index);
        }
        else
        {
            // 不是新游戏就读取存档文件中的数据赋值给 playerData
            string path = GetSaveSlotPath(index);
            string jsonStr = File.ReadAllText(path);
            playerData = JsonConvert.DeserializeObject<PlayerData>(jsonStr);
        }
    }
    #region private
    private string GetSaveSlotPath(int index)
    {
        return $"{Application.persistentDataPath}/{PersistentConstants.SaveSlot}{index}.json";
    }
    #endregion
    #endregion
    #region LoadGamePanel
    public void GetSaveSlotBasicInfo(int index, out int level, out float totalPlayTime, out DateTime lastPlayTime)
    {
        string path = GetSaveSlotPath(index);
        string jsonStr = File.ReadAllText(path);
        PlayerData data = JsonConvert.DeserializeObject<PlayerData>(jsonStr);
        level = data.Level;
        totalPlayTime = data.TotalPlayTime;
        lastPlayTime = data.LastPlayTime;
    }
    public void DeleteSaveSlotFile(int index)
    {
        playerData = null;
        string path = GetSaveSlotPath(index);
        File.Delete(path);
    }
    #endregion
    #region 最后游玩时间 和 总计游玩时长
    /// <summary>
    /// 选择存档进入游戏时，设置开始游戏时间
    /// </summary>
    /// <param name="index"></param>
    public void SetStartGameTimeOnEnterGame(int index)
    {
        // 如果不是0，就代表选择有效的存档开始游戏
        if (index != 0)
        {
            startGameTime = DateTime.Now;
        }
    }
    /// <summary>
    /// 设置目标存档的 最后游玩时间 和 总计游玩时长
    /// </summary>
    /// <param name="index"></param>
    public void SaveGameTime(int index)
    {
        DateTime nowTime = DateTime.Now;
        // Debug.LogError("开始游戏时间：" + startGameTime);
        TimeSpan currentPlayTime = nowTime - startGameTime;
        // Debug.LogError($"本次游玩时长：{currentPlayTime.ToString(@"hh\:mm\:ss")}");

        playerData.LastPlayTime = nowTime;
        /* TimeSpan.Seconds：获取整数秒（向下取整）
           currentPlayTime.Seconds 只返回秒的零头部分（0~59），不包含分钟。
           2026-7-2 01:21:34：一开始用的 Seconds，可把我给坑惨了，会少计算游玩时间。
        */
        // TimeSpan.TotalSeconds：获取秒数（包含小数，精确到毫秒）
        playerData.TotalPlayTime += (float)currentPlayTime.TotalSeconds;
        // Debug.LogError($"总计游玩时长：{TimeSpan.FromSeconds(playerData.TotalPlayTime).ToString(@"hh\:mm\:ss")}");
        SaveGame(index);

        // 重置计时基点
        startGameTime = nowTime;
    }
    #region 保存游戏
    /// <summary>
    /// 保存游戏
    /// </summary>
    /// <param name="index"></param>
    private void SaveGame(int index)
    {
        string path = GetSaveSlotPath(index);
        string jsonStr = JsonConvert.SerializeObject(playerData);
        File.WriteAllText(path, jsonStr);
    }
    #endregion
    #endregion
    #region CharacterInformationPage
    /// <summary>
    /// CharacterInformationPage 时用到，用于展示人物信息
    /// </summary>
    /// <returns></returns>
    public PlayerData GetPlayerData()
    {
        return playerData;
    }
    #endregion
    #region Purchase & BlindBox & Equipment Enhance
    public int GoldCoin { get => playerData.GoldCoin; set => playerData.GoldCoin = value; }
    #endregion
    #region Inventory
    public void AddInventoryItem(InventoryItem item)
    {
        playerData.InventoryItems.Add(item);
    }
    public void DiscardInventoryItem(InventoryItem item)
    {
        playerData.InventoryItems.Remove(item);
    }
    /// <summary>
    /// 通过uid获取一个背包物体
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public InventoryItem GetInventoryItem(string uid)
    {
        return playerData.InventoryItems.Find(item => item.Uid == uid);
    }
    /// <summary>
    /// 获取排序后的所有背包物体
    /// </summary>
    /// <returns></returns>
    public List<InventoryItem> GetSortedAllInventoryItem()
    {
        List<InventoryItem> items = new(playerData.InventoryItems);
        items.Sort(new InventoryItemComparer());
        return items;
    }
    #endregion
    #region Quest
    public List<Quest> AllQuests => playerData.Quests;
    // public Quest GetQuest(string questCode)
    // {
    //     return playerData.Quests.Find(quest => quest.QuestCode == questCode);
    // }
    public int GetQuestProgress(string questCode)
    {
        return playerData.Quests.Find(quest => quest.QuestCode == questCode).ProgressCount;
    }
    /// <summary>
    /// 根据任务代码让目标任务的进度+1
    /// </summary>
    /// <param name="questCode"></param>
    public void AddQuestProgress(string questCode)
    {
        Quest quest = playerData.Quests.Find(quest => quest.QuestCode == questCode);
        if (quest == null)
            return;
        QuestSO questSO = StaticDataService.Instance.GetQuestSO(questCode);
        if (questSO == null)
            return;

        // 已达上限，不再增加
        if (quest.ProgressCount >= questSO.RequiredCount)
            return;
        quest.ProgressCount++;
    }
    #endregion

    #region Equipment Enhance
    public List<Equipment> AllEquipments => playerData.Equipments;
    // public Equipment GetEquipment(string equipmentName)
    // {
    //     return playerData.Equipments.Find(equipment => equipment.EquipmentName == equipmentName);
    // }
    /// <summary>
    /// 获取背包中指定Id物品的数量
    /// </summary>
    public int GetInventoryItemCount(int itemId)
    {
        int count = 0;
        for (int i = 0; i < playerData.InventoryItems.Count; i++)
        {
            if (playerData.InventoryItems[i].Id == itemId)
                count++;
        }
        return count;
    }
    /// <summary>
    /// 从背包中移除指定Id和数量的物品
    /// </summary>
    public void RemoveInventoryItemById(int itemId, int removeCount)
    {
        for (int i = playerData.InventoryItems.Count - 1; i >= 0 && removeCount > 0; i--)
        {
            if (playerData.InventoryItems[i].Id == itemId)
            {
                playerData.InventoryItems.RemoveAt(i);
                removeCount--;
            }
        }
    }
    /// <summary>
    /// 增加角色属性数值
    /// </summary>
    /// <param name="hp"></param>
    /// <param name="attack"></param>
    /// <param name="defense"></param>
    public void AddPlayerAttributes(float hp, float attack, float defense)
    {
        playerData.HP += hp;
        playerData.Attack += attack;
        playerData.Defense += defense;
    }
    #endregion

    #region 战斗相关
    public float MaxHP => playerData.HP;
    public float Attack => playerData.Attack;
    public float Defense => playerData.Defense;
    #endregion
}