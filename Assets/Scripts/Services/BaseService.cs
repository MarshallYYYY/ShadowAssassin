using UnityEngine;

public abstract class BaseService<T> : MonoBehaviour where T : BaseService<T>
{
    private static T instance = null;
    public static T Instance { get => instance; }

    // 由外部（GameManager）调用，在 Awake 时初始化实例
    public virtual void Init()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else if (instance != this)
        {
            Debug.LogWarning($"{typeof(T).Name} 已被注册，当前对象将被忽略。");
        }
    }
}