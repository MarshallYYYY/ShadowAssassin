public class GameConfig
{
    /// <summary>
    /// 游戏运行之初，选择存档的时候，代表上一次选择的存档索引；
    /// 当进入游戏后，代表当前的存档索引。
    /// </summary>
    public int LastSelectedSaveSlotIndex { get; set; } = 0;
    /// <summary>
    /// 主音量
    /// </summary>
    public float MasterVolume { get; set; } = 1f;
    public float BgmVolume { get; set; } = 1f;

    /// <summary>
    /// SFX = Sound Effects（音效）
    /// </summary>
    public float SfxVolume { get; set; } = 1f;
}