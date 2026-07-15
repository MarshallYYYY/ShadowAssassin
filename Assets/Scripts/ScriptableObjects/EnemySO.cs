using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    #region 血量
    [Header("血量")]
    public float MaxHP = 100f;
    #endregion

    #region 移动
    [Header("移动")]
    public float MoveSpeed = 2f;
    public float ChaseSpeed = 4f;
    public float RotationSpeed = 5f;
    #endregion

    #region 检测
    [Header("检测")]
    public float DetectRange = 8f;
    public float AttackRange = 1.5f;
    public float AttackCooldown = 1.5f;
    #endregion

    #region 巡逻
    [Header("巡逻")]
    public float PatrolRadius = 5f;
    public float PatrolWaitTime = 2f;
    #endregion

    public List<AttackAnimSO> AttackAnims = new();
}