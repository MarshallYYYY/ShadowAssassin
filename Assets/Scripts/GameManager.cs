using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GameManager))]
[RequireComponent(typeof(PersistentService))]
[RequireComponent(typeof(SceneLoadService))]
[RequireComponent(typeof(AudioService))]
[RequireComponent(typeof(StaticDataService))]
// 确保 PersistenceService 中 GameConfig 的新建/读取 在StartSceneUIManager.Awake()之前，
// 因为StartSceneUIManager.Awake()中的逻辑（继续游戏按钮、加载游戏按钮的可交互设置）会用到 GameConfig 的数据。
[DefaultExecutionOrder(-100)] // 数值越小，Awake 越早
public class GameManager : MonoBehaviour
{
    #region 单例
    private static GameManager instance;
    public static GameManager Instance { get => instance; }
    #endregion
    [SerializeField] private Canvas sceneLoadCanvas;
    private ThirdPersonControl inputActions;
    public ThirdPersonControl InputActions { get => inputActions; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Debug.LogError($"单例：{gameObject.GetInstanceID()}");
            DontDestroyOnLoad(gameObject);

            Init();
            // StartCoroutine(Init());
        }
        // 从其他场景返回 StartScene 时，销毁 StartScene 中的 GameManager 游戏物体，
        // 保留最初的 GameManager 游戏物体
        // else if (instance != null && instance != this)
        else
        {
            // 如果已存在实例，销毁自己（保留第一个）
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        GetComponent<PersistentService>().Init();
        GetComponent<SceneLoadService>().Init();
        GetComponent<AudioService>().Init();
        GetComponent<StaticDataService>().Init();

        if (sceneLoadCanvas != null)
        {
            // 显示在其他Canvas上方
            sceneLoadCanvas.sortingOrder = 1;
        }
        inputActions = new();
        // 默认启用 UI
        SwitchToUIMode();
        AudioService.Instance.PlayBgm(AudioConstants.BgmStartScene);
    }
    public void SwitchToUIMode()
    {
        inputActions.Player.Disable();
        inputActions.UI.Enable();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void SwitchToPlayMode()
    {
        inputActions.UI.Disable();
        inputActions.Player.Enable();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}