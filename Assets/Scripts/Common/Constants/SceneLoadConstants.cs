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
