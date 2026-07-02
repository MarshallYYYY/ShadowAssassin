using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/QuestDataBaseSO"))]
public class QuestDataBaseSO : ScriptableObject
{
    public List<QuestSO> Quests = new();
}
[Serializable]
public class QuestSO
{
    public string QuestName;
    public string QuestCode;
    public int Id;
    public int RequiredCount;
    public int Exp;
    public int GoldCoin;
}
