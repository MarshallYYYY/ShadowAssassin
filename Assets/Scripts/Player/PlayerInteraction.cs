using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject[] npcs = new GameObject[4];
    [SerializeField] private DialogueWindow dialogueWindow;
    /// <summary>
    /// 当前应该对话的 NPC 索引
    /// </summary>
    private int currentNpcIndex = 0;
    /// <summary>
    /// 本轮正在对话的 NPC 索引
    /// </summary>
    private int talkingNpcIndex = -1;
    void Start()
    {
        // 从持久化的任务进度同步 currentNpcIndex，防止加载存档后归零
        int talkQuestProgress = PersistentService.Instance.GetQuestProgress(QuestCodeConstants.Talk);
        currentNpcIndex = talkQuestProgress;

        for (int i = 0; i < npcs.Length; i++)
        {
            // 只启用第一个 NPC 的 BoxCollider，其他的禁用
            // npcs[i].GetComponent<BoxCollider>().enabled = i == 0;
            npcs[i].GetComponent<BoxCollider>().enabled = i < talkQuestProgress + 1;
        }

        dialogueWindow.OnDialogueEnd += ActiveNextNpcCollider;
        dialogueWindow.gameObject.SetActive(false);
    }
    void OnTriggerStay(Collider other)
    {
        // 保留？：防止在DungeonScene中报空引用错误
        if (dialogueWindow?.gameObject.activeSelf is true)
            return;
        if (other.CompareTag(Constants.NpcTag))
        {
            // 找到碰撞的是第几个 NPC，记录本轮对话索引
            for (int i = 0; i < npcs.Length; i++)
            {
                if (other.gameObject == npcs[i])
                {
                    talkingNpcIndex = i;
                    break;
                }
            }
            // 获取该 NPC 的对话数据
            DialogueSO dialogueSO = StaticDataService.Instance.GetDialogueSO(other.gameObject.name);
            // 将对话数据传递给对话窗口
            dialogueWindow.GetComponent<DialogueWindow>().SetDialogueContent(dialogueSO);
            // 显示对话窗口
            dialogueWindow.gameObject.SetActive(true);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (dialogueWindow?.gameObject.activeSelf is true)
            dialogueWindow.gameObject.SetActive(false);
    }
    /// <summary>
    /// 当前对话结束后，启用下一个 NPC 的 BoxCollider
    /// </summary>
    private void ActiveNextNpcCollider()
    {
        // 仅当本轮对话的 NPC 正好是"应该对话的那个"时，才推进
        // 例子：King 0：talkingNpcIndex = 0，currentNpcIndex = 0
        if (talkingNpcIndex == currentNpcIndex)
        {
            PersistentService.Instance.AddQuestProgress(QuestCodeConstants.Talk);
            // 例子：King 0：talkingNpcIndex = 0，currentNpcIndex = 0
            currentNpcIndex++;
            // 例子：King 0：talkingNpcIndex = 0，currentNpcIndex = 1

            // 启用下一个
            if (currentNpcIndex < npcs.Length)
                npcs[currentNpcIndex].GetComponent<BoxCollider>().enabled = true;
        }
        // 重置
        talkingNpcIndex = -1;
    }
}