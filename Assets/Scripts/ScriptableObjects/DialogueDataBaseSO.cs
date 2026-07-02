using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("Scriptable Objects/DialogueDataBaseSO"))]
public class DialogueDataBaseSO : ScriptableObject
{
    public List<DialogueSO> Dialogues = new();
}

[Serializable]
public class DialogueSO
{
    public string NpcName;
    public string NpcGameObjectName;
    public List<string> Sentences;
}
