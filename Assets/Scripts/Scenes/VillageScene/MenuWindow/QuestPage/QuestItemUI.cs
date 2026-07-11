using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestItemUI : MonoBehaviour
{
    #region UI
    [SerializeField] private Text questNameText;
    [SerializeField] private Text progressText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text goldCoinText;
    [SerializeField] private Text expText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Text claimText;
    #endregion
    #region Data
    private Quest quest;
    private QuestSO questSO;
    #endregion
    #region 公共方法
    public void Init(Quest quest, QuestSO questSO)
    {
        this.quest = quest;
        this.questSO = questSO;

        // 下面这四个是不变的数据
        questNameText.text = questSO.QuestName;
        progressSlider.value = 0;
        progressSlider.maxValue = questSO.RequiredCount;
        goldCoinText.text = questSO.GoldCoin.ToString();
        expText.text = questSO.Exp.ToString();

        claimButton.onClick.AddListener(OnClaimButtonClicked);

        RefreshProgress();
    }

    private void OnClaimButtonClicked()
    {
        quest.IsClaimed = true;
        claimButton.interactable = false;
        claimText.text = "已领取";
    }
    #endregion

    #region UI刷新
    void OnEnable()
    {
        RefreshProgress();
    }
    /// <summary>
    /// 刷新进度
    /// </summary>
    private void RefreshProgress()
    {
        // 防止在Editor中设计的时候将这个UI默认打开，从而执行OnEnable ---> 调用quest，导致报空
        if (quest is not null)
        {
            progressText.text = $"{quest.ProgressCount} / {questSO.RequiredCount}";
            progressSlider.value = quest.ProgressCount;
        }

        if (progressSlider.value == progressSlider.maxValue)
        {
            if (quest.IsClaimed is false)
            {
                claimButton.interactable = true;
                claimText.text = "领取";
            }
            else
            {
                claimButton.interactable = false;
                claimText.text = "已领取";
            }
        }
        else
        {
            claimButton.interactable = false;
            claimText.text = "领取";
        }
    }
    #endregion
}
