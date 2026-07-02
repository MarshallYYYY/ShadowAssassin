using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SaveSlot))]
public class SaveSlot : MonoBehaviour
{
    #region 没有存档文件
    [Header("没有存档文件")]
    [SerializeField] private GameObject text;
    #endregion

    #region 没有存档文件
    [Header("有存档文件")]
    [SerializeField] private GameObject content;
    [SerializeField] private Text levelText;
    [SerializeField] private Text totalPlayTimeText;
    [SerializeField] private Text lastPlayTimeText;
    [SerializeField] private Button contentButton;
    [SerializeField] private Button deleteButton;
    #endregion

    int index = 0;
    #region 事件：解耦与 StartSceneUIManager 的强关联
    public event Action<int> ContentButtonClicked;
    public event Action<int> DeleteButtonClicked;
    #endregion
    void Start()
    {
        index = transform.GetSiblingIndex() + 1;
        RefreshUI();
        contentButton.onClick.AddListener(() => ContentButtonClicked?.Invoke(index));
        deleteButton.onClick.AddListener(() => DeleteButtonClicked?.Invoke(index));
    }
    public void RefreshUI()
    {
        // 根据目标存档是否存在，显示不同的UI
        bool isExists = PersistentService.Instance.IsExistsSaveFile(index);
        // 如果目标存档存在，则关闭Text，显示Content
        text.SetActive(!isExists);
        content.SetActive(isExists);

        // 如果内容显示（即存档存在），就显示 等级、总计游玩时长 和 最后游玩时间
        if (content.activeSelf)
        {
            PersistentService.Instance.GetSaveSlotBasicInfo(
                index, out int level, out float totalPlayTime, out DateTime lastPlayTime);
            levelText.text = level.ToString();
            totalPlayTimeText.text = TimeSpan.FromSeconds(totalPlayTime).ToString(@"hh\:mm\:ss");
            lastPlayTimeText.text = lastPlayTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
