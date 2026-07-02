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
        // 2026-7-2 01:35:08：打包后到exe运行没有跟NPC的Trigger触发，怀疑是npcs没有成功赋值
        // if (npcs.Length != 4)
        // {
        //     for (int i = 0; i < 4; i++)
        //     {
        //         Transform npcsTransoform = GameObject.Find("NPCs").transform;
        //         npcs[i] = npcsTransoform.GetChild(i).gameObject;
        //     }
        // }
        for (int i = 0; i < npcs.Length; i++)
        {
            // 只启用第一个 NPC 的 BoxCollider，其他的禁用
            npcs[i].GetComponent<BoxCollider>().enabled = i == 0;
        }

        dialogueWindow.OnDialogueEnd += ActiveNextNpcCollider;
        dialogueWindow.Init();
        dialogueWindow.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
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
    /// <summary>
    /// 当前对话结束后，启用下一个 NPC 的 BoxCollider
    /// </summary>
    private void ActiveNextNpcCollider()
    {
        // 仅当本轮对话的 NPC 正好是"应该对话的那个"时，才推进
        if (talkingNpcIndex == currentNpcIndex)
        {
            currentNpcIndex++;
            // 启用下一个
            if (currentNpcIndex < npcs.Length)
                npcs[currentNpcIndex].GetComponent<BoxCollider>().enabled = true;
        }
        // 重置
        talkingNpcIndex = -1;
    }
}