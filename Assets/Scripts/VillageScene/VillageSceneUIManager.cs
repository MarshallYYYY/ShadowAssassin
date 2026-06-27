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
        InitMenu();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menu.SetActive(!menu.activeSelf);
        }
    }
    #region Menu
    [SerializeField] private Button openMenuButton;
    [SerializeField] private GameObject menu;
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
        openMenuButton.onClick.AddListener(() => menu.SetActive(true));

        menu.SetActive(false);

        Transform tabBar = menu.transform.Find("TabBar");
        List<Button> tabBarButtons = new();
        for (int i = 0; i < tabBar.childCount; i++)
        {
            Button button = tabBar.GetChild(i).GetComponent<Button>();
            int index = i; // 关键：拷贝循环变量，避免闭包问题
            button.onClick.AddListener(() => OnTabBarButtonClicked(index));
            tabBarButtons.Add(button);

            GameObject image = button.transform.Find("SelectedImage").gameObject;
            tabButtonSelectedImages.Add(image);
            image.SetActive(i == 0);
        }

        Transform tabPages = menu.transform.Find("TabPages");
        for (int i = 0; i < tabPages.childCount; i++)
        {
            GameObject page = tabPages.GetChild(i).gameObject;
            pages.Add(page);
            // 只让第一个标签页（角色信息）默认显示
            page.SetActive(i == 0);
        }
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
