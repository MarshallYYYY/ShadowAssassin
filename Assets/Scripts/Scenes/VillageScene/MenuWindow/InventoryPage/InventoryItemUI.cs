using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectedImage;
    #region Data
    private InventoryItem inventoryItem;
    private InventoryItemSO inventoryItemSO;
    // TODO：移除上层UI物体的引用，使用事件？
    private InventoryPage inventoryPage;
    private bool isSelected = false;
    #endregion
    void OnDestroy()
    {
        selectedFadeTween?.Kill();
    }
    public void Refresh(InventoryItem inventoryItem, InventoryPage inventoryPage)
    {
        // 数据初始化
        this.inventoryItem = inventoryItem;
        inventoryItemSO = StaticDataService.Instance.GetInventoryItemSO(inventoryItem.Id);
        this.inventoryPage = inventoryPage;

        iconImage.sprite = inventoryItemSO.Sprite;
        selectedImage.gameObject.SetActive(false);
    }
    /// <summary>
    /// 空槽位：只显示背景框，不显示物品图标和选中效果
    /// </summary>
    public void SetEmpty()
    {
        iconImage.gameObject.SetActive(false);
        selectedImage.gameObject.SetActive(false);
    }
    #region IPointerXXXHandler
    private Tween selectedFadeTween;
    private const float FadeDuration = 0.5f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected)
            return;
        selectedFadeTween?.Kill();                              // 终止上一次动画
        selectedImage.gameObject.SetActive(true);
        selectedImage.color = new Color(1f, 1f, 1f, a: 0f); // 从透明开始
        selectedFadeTween = selectedImage.DOFade(endValue: 1f, duration: FadeDuration); // 1 秒淡入
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIButtonClick);
        if (inventoryPage.CurrentSelectedItemUid == inventoryItem.Uid)
            return;
        isSelected = true;
        // 根据点击设置最新的uid -> 进而刷新详情界面
        inventoryPage.CurrentSelectedItemUid = inventoryItem.Uid;
        // 终止淡入/淡出
        selectedFadeTween?.Kill();
        selectedImage.gameObject.SetActive(true);
        // 瞬间满不透明
        selectedImage.DOFade(1f, 0f);
        // 除了已被点击的当前物体，其他物体的 selectedImage 都关闭显示
        int index = transform.GetSiblingIndex();
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if (i != index)
            {
                InventoryItemUI itemUI = transform.parent.GetChild(i).GetComponent<InventoryItemUI>();
                itemUI.selectedImage.gameObject.SetActive(false);
                itemUI.isSelected = false;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 当前物品没有被点击的时候才会执行淡出动画
        if (isSelected)
            return;
        selectedFadeTween?.Kill();
        selectedFadeTween = selectedImage.DOFade(0f, FadeDuration)
            .OnComplete(() => selectedImage.gameObject.SetActive(false));  // 淡出完毕后隐藏
    }
    #endregion
}