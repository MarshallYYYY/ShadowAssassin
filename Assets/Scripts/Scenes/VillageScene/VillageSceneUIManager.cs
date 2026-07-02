using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VillageSceneUIManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.LogWarning($"当前使用的是存档 {PersistentService.Instance.GetLastSelectedIndex()}");
        GameManager.Instance.SwitchToPlayMode();

        InitMenu();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 如果Menu打开并处于UI模式，则切换到Player模式，然后关闭Menu
            if (menuWindow.activeSelf && GameManager.Instance.InputActions.UI.enabled)
            {
                GameManager.Instance.SwitchToPlayMode();
                menuWindow.SetActive(false);
            }
            // 如果Menu关闭并处于Player模式，则切换到UI模式，然后显示Menu
            else if (menuWindow.activeSelf is false && GameManager.Instance.InputActions.Player.enabled)
            {
                GameManager.Instance.SwitchToUIMode();
                menuWindow.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            PersistentService.Instance.SetGoldCoin(99999);
        }
    }
    #region MenuWindow
    [SerializeField] private GameObject menuWindow;
    /// <summary>
    /// TabBar/XXXButton/Image
    /// </summary>
    private readonly List<GameObject> tabButtonSelectedImages = new();
    /// <summary>
    /// TabPages/XXX
    /// </summary>
    private readonly List<GameObject> pages = new();
    private void InitMenu()
    {
        menuWindow.SetActive(false);

        Transform tabBar = menuWindow.transform.Find("TabBar");
        List<Button> tabBarButtons = new();
        for (int i = 0; i < tabBar.childCount; i++)
        {
            Button button = tabBar.GetChild(i).GetComponent<Button>();
            // 关键：拷贝循环变量，避免闭包问题
            int index = i;
            button.onClick.AddListener(() => OnTabBarButtonClicked(index));
            tabBarButtons.Add(button);

            GameObject image = button.transform.Find("SelectedImage").gameObject;
            tabButtonSelectedImages.Add(image);
            image.SetActive(i == 0);
        }
        Transform tabPages = menuWindow.transform.Find("TabPages");
        for (int i = 0; i < tabPages.childCount; i++)
        {
            GameObject page = tabPages.GetChild(i).gameObject;
            pages.Add(page);
            // 只让第一个标签页（角色信息）默认显示
            page.SetActive(i == 0);
        }
        /* 2026-7-1 01:53:33
        让CharacterInformationPage物体的子物体（CharacterShowImage）上挂载的脚本RotateModel的Init()先执行一下，
        记录最开始的CharacterShowCamera与Player的偏移。
        */
        pages[0].transform.Find("CharacterShowImage").GetComponent<RotateModel>().Init();
    }
    private void OnTabBarButtonClicked(int index)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            tabButtonSelectedImages[i].SetActive(i == index);
            pages[i].SetActive(i == index);
        }
    }
    #endregion
}