public static class AnimatorConstants
{
    #region Locomotion
    public const string AxisX = nameof(AxisX);
    public const string AxisY = nameof(AxisY);
    #endregion

    #region Animator State （节点）名称
    public const string LocomotionState = "Idle And Run";
    public const string RollState = "Roll";
    public const string AvoidState = "Avoid";
    public const string HitState = "Hit";
    public const string DeadState = "Dead";
    #endregion

    #region 动画时长（秒）
    public const float RollAnimTotalTime = 1.167f;
    public const float AvoidAnimTotalTime = 0.667f;
    public const float HitAnimTotalTime = 0.467f;
    #endregion

    #region CrossFadeInFixedTime 过渡时长（秒）
    public const float LocomotionFadeDuration = 0.15f;
    public const float AttackFadeDuration = 0.1f;
    public const float RollFadeDuration = 0.1f;
    public const float AvoidFadeDuration = 0.1f;
    #endregion

    #region 当前动画播放到N%的时候，便切换到新动画，让动画的过渡更加丝滑
    public const float AvoidEarlyExitRatio = 0.6f;
    public const float RollEarlyExitRatio = 0.7f;
    #endregion
}
