public static class YooAssetConstants
{
    public const string PackageName = "DefaultPackage";
}
public static class Constants
{
    public const string NpcTag = "NPC";
    public const string PlayerTag = "Player";
}
public static class PersistentConstants
{
    /// <summary>
    /// 存档槽的数量
    /// </summary>
    public const int SaveSlotCount = 3;

    public const string GameConfig = nameof(GameConfig);
    public const string SaveSlot = nameof(SaveSlot);
}
public static class SceneLoadConstants
{
    private const string ScenesFolderPath = "Assets/Scenes/";
    public const string StartScene = ScenesFolderPath + nameof(StartScene);
    public const string VillageScene = ScenesFolderPath + nameof(VillageScene);
    public const string DungeonScene = ScenesFolderPath + nameof(DungeonScene);

    /// <summary>
    /// 场景加载过程中背景图片的淡入淡出的持续时间
    /// </summary>
    public const float SceneLoadBgImageFadeTime = 1f;
}
public static class AnimatorConstants
{
    public const string AxisX = nameof(AxisX);
    public const string AxisY = nameof(AxisY);

    public const string Avoid = nameof(Avoid);
    public const string Roll = nameof(Roll);
    public const string Hit = nameof(Hit);
    public const string Dead = nameof(Dead);

    public const float AvoidAnimTotalTime = 0.667f;
    public const float RollAnimTotalTime = 1.167f;
    public const float HitAnimTotalTime = 0.467f;
    public const float DeadAnimTotalTime = 1.267f;
}
public class NpcGameObjectNameConstants
{
    public const string King = nameof(King);
    public const string Knight = nameof(Knight);
    public const string Merchant = nameof(Merchant);
    public const string Blacksmith = nameof(Blacksmith);
}
public class QuestCodeConstants
{
    public const string Talk = nameof(Talk);
    public const string Purchase = nameof(Purchase);
    public const string BlindBox = nameof(BlindBox);
    public const string Enhance = nameof(Enhance);
    public const string Dungeon = nameof(Dungeon);
}

public class EquipmentConstants
{
    public const string Shoulder = nameof(Shoulder);
    public const string Top = nameof(Top);
    public const string Bottom = nameof(Bottom);
    public const string Belt = nameof(Belt);
    public const string Shoes = nameof(Shoes);
    public const string Weapon = nameof(Weapon);
}

public static class EnemyConstants
{
    public const string EnemyTag = "Enemy";
    public const string EnemyLayer = "Enemy";
}

/// <summary>
/// 敌人 Animator 状态名，与 EnemyAnimatorController.controller 中的 State 名称对应
/// </summary>
public static class EnemyAnimConstants
{
    public const string Walk = nameof(Walk);
    public const string Run = nameof(Run);
    public const string HorizontalAttack = nameof(HorizontalAttack);
    public const string DownwardAttack = nameof(DownwardAttack);
    public const string GetHit = nameof(GetHit);
    public const string Dead = nameof(Dead);
}