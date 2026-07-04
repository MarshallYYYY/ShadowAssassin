using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/EquipmentDataBaseSO")]
public class EquipmentDataBaseSO : ScriptableObject
{
    public List<EquipmentSO> Equipments = new();
}