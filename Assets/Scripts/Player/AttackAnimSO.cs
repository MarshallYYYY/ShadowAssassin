using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = ("YYY"), menuName = ("Scriptable Objects/AttackAnimSO"))]
[CreateAssetMenu(menuName = ("Scriptable Objects/AttackAnimSO"))]
public class AttackAnimSO : ScriptableObject
{
    public float Length;
    public float TotalFrame;
    /// <summary>
    /// 进入"判定"阶段的帧
    /// </summary>
    public int EnterHitFrame;
    /// <summary>
    /// 进入"收尾"阶段的帧
    /// </summary>
    public int EnterFollowThroughFrame;
    // public int OffsetFrame;

    /// <summary>
    /// 判定阶段起始的归一化时间（0~1）
    /// </summary>
    public float EnterHitTime => (float)(EnterHitFrame / TotalFrame) * Length;

    /// <summary>
    /// 收尾阶段起始的归一化时间（0~1），此阶段可移动/接下一招
    /// </summary>
    public float EnterFollowThroughTime => (float)(EnterFollowThroughFrame / TotalFrame) * Length;

    // public float OffsetTime => (float)(OffsetFrame / TotalFrame) * Length;
}
