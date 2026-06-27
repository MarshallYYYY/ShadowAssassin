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
    public int Diamond { get; set; } = 0;
    /// <summary>
    /// 水晶
    /// </summary>
    public int Crystal { get; set; } = 0;
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
    /// 首次进入游戏的时候通过读取配置文件进行写入任务目录
    /// </summary>
    public List<Task> Tasks { get; set; } = new();

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
public class Task
{
    // 对应的配置文件中，任务奖励有：Id, Name, TargetFinishedCount, Exp, Coin，但这些都是不变的；
    // 在存档文件的类中，只需要记录能够变化的值即可，其余的从配置文件读取即可。
    public int Id { get; set; }
    /// <summary>
    /// 任务的完成次数
    /// </summary>
    public int FinishedCount { get; set; }
    public bool IsClaimed { get; set; }
}