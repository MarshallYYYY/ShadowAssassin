using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPage : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Button")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;

    void Awake()
    {
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }
    void OnEnable()
    {
        // 父物体的启用也会调用子物体的 OnEnable() 函数
        OnCancelButtonClicked();
    }
    private void OnSaveButtonClicked()
    {
        PersistentService.Instance.SetGameConfigVolume(masterSlider.value, bgmSlider.value, sfxSlider.value);
        AudioService.Instance.SetAudioSourceVolume(masterSlider.value, bgmSlider.value, sfxSlider.value);
    }
    private void OnCancelButtonClicked()
    {
        PersistentService.Instance.GetVolume(out float masterVolume, out float bgmVolume, out float sfxVolume);
        masterSlider.value = masterVolume;
        bgmSlider.value = bgmVolume;
        sfxSlider.value = sfxVolume;
    }
}
