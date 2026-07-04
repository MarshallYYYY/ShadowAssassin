using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/AttackAnimDataBaseSO"))]
public class AttackAnimDataBaseSO : ScriptableObject
{
    // public Dictionary<AttackType, List<AttackAnim>> AttackAnimList = new();
    public List<AttackAnimSO> LightAttackAnims = new();
    public List<AttackAnimSO> HeavyAttackAnims = new();
}

[Serializable]
public class AttackAnimSO
{
    public AnimationClip Clip;
    public string Name;
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
