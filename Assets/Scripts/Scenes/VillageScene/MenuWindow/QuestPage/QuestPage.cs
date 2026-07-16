using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestPage : MonoBehaviour
{
    [SerializeField] private GameObject questItemUIPrefab;
    [SerializeField] private GameObject scrollView;
    void Awake()
    {
        // 清理滚动容器中原本的物体
        RectTransform scrollContent = scrollView.GetComponent<ScrollRect>().content;
        for (int i = scrollContent.childCount - 1; i >= 0; i--)
        {
            Destroy(scrollContent.GetChild(i).gameObject);
        }

        List<Quest> quests = PersistentService.Instance.AllQuests;
        for (int i = 0; i < quests.Count; i++)
        {
            Quest quest = quests[i];
            QuestSO questSO = StaticDataService.Instance.GetQuestSO(quest.QuestCode);

            Transform questItemUITransform = Instantiate(questItemUIPrefab.transform, scrollContent);
            QuestItemUI questItemUI = questItemUITransform.GetComponent<QuestItemUI>();

            questItemUI.Init(quest, questSO);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent);
    }
}