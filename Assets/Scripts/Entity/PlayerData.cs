using System;
using System.Collections.Generic;

public class PlayerData
{
    #region 基本信息
    public string Name { get; } = "熊熊";
    public int Level { get; set; } = 1;
    public float Exp { get; set; } = 0;
    #endregion

    #region 游戏时间
    public DateTime LastPlayTime { get; set; }
    public float TotalPlayTime { get; set; } = 0;
    #endregion

    #region 资源信息
    public int GoldCoin { get; set; } = 1000;
    // public int Diamond { get; set; } = 0;
    /// <summary>
    /// 水晶
    /// </summary>
    // public int Crystal { get; set; } = 0;
    public List<InventoryItem> InventoryItems = new();
    #endregion

    #region 人物属性
    public float HP { get; set; } = 100;
    public float Attack { get; set; } = 10;
    public float Defense { get; set; } = 5;
    /// <summary>
    /// 暴击率
    /// </summary>
    public float CriticalRate { get; set; } = 5;
    /// <summary>
    /// 闪避率
    /// </summary>
    public float DodgeRate { get; set; } = 2;
    #endregion
    /// <summary>
    /// key: Equipment, value: EnhanceLevel
    /// </summary>
    public Dictionary<Equipment, int> EquipmentEnhanceDict = new()
    {
        { Equipment.Shoulder, 0 },
        { Equipment.Top, 0 },
        { Equipment.Bottom, 0 },
        { Equipment.Belt, 0 },
        { Equipment.Shoes, 0 },
        { Equipment.Weapon, 0 }
    };
    /// <summary>
    /// 在使用某个存档首次进入游戏的时候，通过读取配置文件进行写入任务目录
    /// </summary>
    public List<Quest> Quests { get; set; } = new();
}
/// <summary>
/// 现在的效果是：同一种物品如果持有多个，则会显示多个。
/// 之所以保留这个类，是因为后期可以扩展，比如：显示数量，
/// 这样就可以让同一种物品只占用一个背包格子，然后右下角可以来一个显示数量的文本。
/// </summary>
public class InventoryItem
{
    public string Uid;
    /// <summary>
    /// Id 跟 InventoryItemSO 的 Id 一一对应
    /// </summary>
    public int Id;
}
public enum Equipment
{
    Shoulder,
    Top,
    Bottom,
    Belt,
    Shoes,
    Weapon
}
public class Quest
{
    // 对应的配置文件中，任务奖励有：Id, QuestName, QuestCode, RequiredCount, Exp, Coin，但这些都是不变的；
    // 在存档文件的类中，只需要记录QuestCode和其他能够变化的值即可，其余的从配置文件读取即可。
    /// <summary>
    /// 任务代号
    /// </summary>
    public string QuestCode { get; set; }
    /// <summary>
    /// 任务的进度
    /// </summary>
    public int ProgressCount { get; set; } = 0;
    public bool IsClaimed { get; set; } = false;
}