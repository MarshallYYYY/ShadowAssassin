using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentItemUI : MonoBehaviour
{
    #region UI
    [SerializeField] private Button itemButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectedImage;
    [SerializeField] private Transform starGroupTransform;
    #endregion

    #region Data
    private Equipment equipment;
    #endregion
    #region 公共接口（外部可订阅/调用）
    public event Action<Equipment> EquipmentSelected;
    public void Init(Equipment equipment)
    {
        this.equipment = equipment;
        itemButton.onClick.AddListener(() => OnItemButtonClicked(transform.GetSiblingIndex()));
        iconImage.sprite = StaticDataService.Instance.GetEquipmentSO(equipment.EquipmentName).Sprite;
    }
    /// <summary>
    /// 更新当前装备的StarGroup，并按需选择是否选中第一个装备。
    /// 因为Refresh()是由EnhanceWindow调用的，所以Refresh()的执行顺序在Awake()前面。
    /// </summary>
    /// <param name="isOnEnable">true：更新StarGroup，并设置默认值，也就是选中第一个装备；false：只更新StarGroup</param>
    public void Refresh(bool isOnEnable)
    {
        // 更新自己的星星（所有 itemUI 都需要）
        for (int i = 0; i < starGroupTransform.childCount; i++)
        {
            starGroupTransform.GetChild(i).gameObject.SetActive(i < equipment.EnhanceLevel);
        }
        if (isOnEnable)
        {
            // 只让第一个触发选中
            // 这个if判断有必要存在，目的是防止if块里的逻辑重复执行6次
            if (transform.GetSiblingIndex() == 0)
                OnItemButtonClicked(0);
        }
    }
    #endregion
    private void OnItemButtonClicked(int index)
    {
        // 除了已被点击的当前物体，其他物体的 selectedImage 都关闭显示
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            EquipmentItemUI itemUI = transform.parent.GetChild(i).GetComponent<EquipmentItemUI>();
            itemUI.selectedImage.gameObject.SetActive(i == index);
        }
        EquipmentSelected?.Invoke(equipment);
    }
}