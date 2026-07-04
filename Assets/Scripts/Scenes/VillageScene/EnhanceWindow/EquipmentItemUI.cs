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
    #endregion

    int index = -1;
    void Awake()
    {
        index = transform.GetSiblingIndex();
        itemButton.onClick.AddListener(OnItemButtonClicked);
    }
    private void OnItemButtonClicked()
    {
        // 除了已被点击的当前物体，其他物体的 selectedImage 都关闭显示
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            EquipmentItemUI itemUI = transform.parent.GetChild(i).GetComponent<EquipmentItemUI>();
            if (i != index)
            {
                itemUI.selectedImage.gameObject.SetActive(false);
            }
            else
            {
                itemUI.selectedImage.gameObject.SetActive(true);
                EquipmentSelected?.Invoke(gameObject.name);
            }
        }
    }
    #region 公共接口（外部可订阅/调用）
    public event Action<string> EquipmentSelected;
    public void Refresh()
    {
        // 首次进入时设置index
        if (index == -1)
        {
            index = transform.GetSiblingIndex();
        }
        // 刷新后只选中第一个装备
        if (index == 0)
        {
            OnItemButtonClicked();
        }
    }
    #endregion
}
