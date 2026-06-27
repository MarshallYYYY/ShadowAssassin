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
    public const string PackageName = "DefaultPackage";

    private const string ScenesFolderPath = "Assets/Scenes/";
    public const string StartScene = ScenesFolderPath + nameof(StartScene);
    public const string VillageScene = ScenesFolderPath + nameof(VillageScene);

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
}