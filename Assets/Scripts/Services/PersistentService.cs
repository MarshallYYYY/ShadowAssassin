using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

    #region PlayerData（存档文件 和 玩家数据）
    /// <summary>
    /// 目标索引对应的存档文件是否存在
    /// </summary>
    /// <param name="index">限制：1/2/3</param>
    /// <returns></returns>
    public bool IsExistsSaveFile(int index)
    {
        string path = $"{Application.persistentDataPath}/{PersistentConstants.SaveSlot}{index}.json";
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
            SaveGameSaveFile(index);
        }
        else
        {
            // 不是新游戏就读取存档文件中的数据赋值给 playerData
            string path = $"{Application.persistentDataPath}/{PersistentConstants.SaveSlot}{index}.json";
            string jsonStr = File.ReadAllText(path);
            playerData = JsonConvert.DeserializeObject<PlayerData>(jsonStr);
        }
    }
    #region 最后游玩时间 和 总计游玩时长
    /// <summary>
    /// 选择存档进入游戏时，设置开始游戏时间并获取存档中的总计游戏时间
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
    public void SetGameTime(int index)
    {
        DateTime nowTime = DateTime.Now;
        // Debug.LogError("开始游戏时间：" + startGameTime);
        TimeSpan currentPlayTime = nowTime - startGameTime;
        // Debug.LogError($"本次游玩时长：{currentPlayTime.ToString(@"hh\:mm\:ss")}");

        playerData.LastPlayTime = nowTime;
        // TimeSpan.TotalSeconds：获取秒数（包含小数，精确到毫秒）
        // TimeSpan.Seconds：获取整数秒（向下取整）
        playerData.TotalPlayTime += currentPlayTime.Seconds;
        // Debug.LogError($"总计游玩时长：{TimeSpan.FromSeconds(playerData.TotalPlayTime).ToString(@"hh\:mm\:ss")}");
        SaveGameSaveFile(index);

        // 重置计时基点
        startGameTime = nowTime;
    }
    #endregion
    #region 加载游戏面板- SaveSlot
    public void GetSaveSlotBasicInfo(int index, out int level, out float totalPlayTime, out DateTime lastPlayTime)
    {
        string path = $"{Application.persistentDataPath}/{PersistentConstants.SaveSlot}{index}.json";
        string jsonStr = File.ReadAllText(path);
        PlayerData data = JsonConvert.DeserializeObject<PlayerData>(jsonStr);
        level = data.Level;
        totalPlayTime = data.TotalPlayTime;
        lastPlayTime = data.LastPlayTime;
    }
    public void DeleteSaveSlotFile(int index)
    {
        playerData = null;
        string path = $"{Application.persistentDataPath}/{PersistentConstants.SaveSlot}{index}.json";
        File.Delete(path);
    }
    #endregion
    public PlayerData GetPlayerData()
    {
        return playerData;
    }
    private void SaveGameSaveFile(int index)
    {
        string path = $"{Application.persistentDataPath}/{PersistentConstants.SaveSlot}{index}.json";
        string jsonStr = JsonConvert.SerializeObject(playerData);
        File.WriteAllText(path, jsonStr);
    }
    #endregion
}
