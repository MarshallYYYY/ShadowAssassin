/// <summary>
/// 敌人 Animator 状态名，与 EnemyAnimatorController.controller 中的 State 名称对应
/// </summary>
public static class EnemyAnimConstants
{
    public const string Idle = nameof(Idle);
    public const string Walk = nameof(Walk);
    public const string Run = nameof(Run);
    public const string HorizontalAttack = nameof(HorizontalAttack);
    public const string DownwardAttack = nameof(DownwardAttack);
    public const string GetHit = nameof(GetHit);
    public const string Dead = nameof(Dead);
}
