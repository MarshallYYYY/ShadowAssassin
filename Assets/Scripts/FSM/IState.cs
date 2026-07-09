/// <summary>
/// 通用状态接口，Player 和 Enemy 的状态均实现此接口
/// </summary>
public interface IState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}
