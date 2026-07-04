using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BlindBoxWindow : MonoBehaviour
{
    #region UI物体
    [Header("UI物体")]
    [SerializeField] private Button backButton;
    [SerializeField] private Transform blindBoxGroupTransform;
    [SerializeField] private Button open1BlindBoxButton;
    [SerializeField] private Button open10BlindBoxButton;
    #endregion
    #region 预制体
    [Header("预制体")]
    [SerializeField] private Image blindBoxPrefab;
    #endregion
    #region TipPanel
    [Header("TipPanel")]
    [SerializeField] private GameObject tipPanel;
    [SerializeField] private Text tipText;
    [SerializeField] private Button confirmButton;
    #endregion
    void Awake()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        open1BlindBoxButton.onClick.AddListener(OnOpen1BlindBoxButtonClicked);
        open10BlindBoxButton.onClick.AddListener(OnOpen10BlindBoxButtonClicked);

        tipPanel.SetActive(false);
        confirmButton.onClick.AddListener(() => tipPanel.SetActive(false));
    }
    void OnEnable()
    {
        ClearBlindBoxs();
    }
    #region Button Events
    private void OnBackButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnOpen1BlindBoxButtonClicked()
    {
        StartCoroutine(OpenBlindBoxs(1));
    }

    private void OnOpen10BlindBoxButtonClicked()
    {
        StartCoroutine(OpenBlindBoxs(10));
    }
    #region 打开指定数量的盲盒：清空上一轮的盲盒、生成随机盲盒物品、播放开盲盒动画
    /// <summary>
    /// 打开指定数量的盲盒
    /// </summary>
    /// <param name="count"></param>
    private IEnumerator OpenBlindBoxs(int count)
    {
        DOTween.KillAll(true); // 终止上一轮残留的所有 tween

        int goldCoin = PersistentService.Instance.GetGoldCoin();
        int totalPrice = count switch
        {
            1 => 100,
            10 => 900,
            _ => 0,
        };
        if (goldCoin < totalPrice)
        {
            ShowTipPanel("金币好像不够了呢");
            yield break;
        }

        open1BlindBoxButton.interactable = false;
        open10BlindBoxButton.interactable = false;
        backButton.interactable = false;
        ClearBlindBoxs();


        // 生成所有物品数据
        var items = new List<InventoryItem>();
        for (int i = 0; i < count; i++)
        {
            InventoryItem item = GetBlindBoxItem();
            items.Add(item);
            // 添加物品到背包
            PersistentService.Instance.AddInventoryItem(item);
        }

        yield return PlayOpenBlindBoxAnim(items, count);

        // 扣钱
        PersistentService.Instance.SetGoldCoin(goldCoin - totalPrice);
        PersistentService.Instance.AddQuestProgress(QuestCodeConstants.BlindBox);
        ShowTipPanel($"成功开启{count}个盲盒！");

        open1BlindBoxButton.interactable = true;
        open10BlindBoxButton.interactable = true;
        backButton.interactable = true;
        yield return null;
    }
    /// <summary>
    /// 清除 BlindBoxGroup 下的所有 BlindBox 物体
    /// </summary>
    private void ClearBlindBoxs()
    {
        for (int i = blindBoxGroupTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(blindBoxGroupTransform.GetChild(i).gameObject);
        }
    }
    /// <summary>
    /// 打开盲盒，并返回该物品
    /// </summary>
    /// <returns></returns>
    private InventoryItem GetBlindBoxItem()
    {
        List<InventoryItemSO> itemSOs = StaticDataService.Instance.GetAllInventoryItemSO();
        int index = Random.Range(0, itemSOs.Count);
        InventoryItemSO itemSO = itemSOs[index];
        InventoryItem item = new()
        {
            Uid = System.Guid.NewGuid().ToString(),
            Id = itemSO.Id,
        };
        return item;
    }
    private IEnumerator PlayOpenBlindBoxAnim(List<InventoryItem> items, int count)
    {
        // 暂存每个盲盒的 cover 和 itemImg 引用，用于后续逐个揭示
        var boxes = new List<(Image cover, Image itemImg, InventoryItemSO itemSO)>();

        // 生成盲盒UI
        foreach (var item in items)
        {
            InventoryItemSO itemSO = StaticDataService.Instance.GetInventoryItemSO(item.Id);

            Image box = Instantiate(blindBoxPrefab, blindBoxGroupTransform);
            Image cover = box;
            Image itemImg = box.transform.GetChild(0).GetComponent<Image>();

            cover.color = Color.white;
            cover.enabled = true;
            itemImg.sprite = itemSO.Sprite;
            itemImg.color = new Color(1f, 1f, 1f, 0f);
            itemImg.gameObject.SetActive(true);
            box.gameObject.SetActive(true);

            boxes.Add((cover, itemImg, itemSO));
        }

        if (count == 1)
        {
            // 单抽：晃动 + 揭示一气呵成
            var (cover, itemImg, _) = boxes[0];
            Sequence seq = DOTween.Sequence();
            seq.Append(cover.rectTransform.DOShakeRotation(2f, 25f, 8, 90f));
            seq.Join(cover.rectTransform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 2f, 6, 1f));
            // 快速淡出
            seq.Append(cover.DOFade(0f, 0.3f));
            // 缓慢淡入
            seq.Join(itemImg.DOFade(1f, 1f));
            seq.OnComplete(() => { if (cover != null) cover.enabled = false; });

            yield return seq.WaitForCompletion();
        }
        else
        {
            // 十连抽：所有盲盒同时晃动 → 逐个揭示
            // Phase 1: 同时晃动所有盲盒
            foreach (var (cover, _, _) in boxes)
            {
                cover.rectTransform.DOShakeRotation(2f, 25f, 8, 90f);
                cover.rectTransform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 2f, 6, 1f);
            }

            // 等待晃动结束
            yield return new WaitForSeconds(2f);

            // Phase 2: 逐个揭示
            foreach (var (cover, itemImg, _) in boxes)
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(cover.DOFade(0f, 0.3f));
                seq.Join(itemImg.DOFade(1f, 1f));
                seq.OnComplete(() => { if (cover != null) cover.enabled = false; });

                yield return new WaitForSeconds(0.3f);
            }
        }
    }
    private void ShowTipPanel(string content)
    {
        tipText.text = content;
        tipPanel.SetActive(true);
    }
    #endregion
    #endregion
}