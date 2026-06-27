using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/AttackAnimDataBaseSO"))]
public class AttackAnimDataBaseSO : ScriptableObject
{
    public List<AttackAnimSO> AttackAnimList = new();
}
