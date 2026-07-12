/// <summary>
/// 通用状态机，管理状态切换与每帧更新。
/// 泛型参数 T 为持有状态机的宿主类型（如 PlayerController、EnemyController），
/// 便于后续扩展时通过 Owner 访问宿主上下文。
/// </summary>
public class StateMachine<T> where T : IStateMachineOwner
{
    /// <summary>当前激活的状态</summary>
    public IState CurrentState { get; private set; }

    /// <summary>宿主引用，供调试或扩展使用</summary>
    public T Owner { get; }

    public StateMachine(T owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// 切换到目标状态：先调用当前状态的 OnExit，清除一次性输入标志，再进入新状态的 OnEnter
    /// </summary>
    public void ChangeState(IState newState)
    {
        CurrentState?.OnExit();
        // 清除一次性输入标志，防止状态切换后残留的输入自动触发新状态
        Owner.ClearOneShotInputs();
        CurrentState = newState;
        CurrentState.OnEnter();
    }

    /// <summary>
    /// 每帧驱动当前状态
    /// </summary>
    public void Update()
    {
        CurrentState?.OnUpdate();
    }
}

/// <summary>
/// 状态机宿主接口，要求宿主实现清除一次性输入的逻辑
/// </summary>
public interface IStateMachineOwner
{
    /// <summary>
    /// 清除一次性输入标志（如翻滚、闪避、攻击等），在每次状态切换时调用
    /// </summary>
    void ClearOneShotInputs();
}
