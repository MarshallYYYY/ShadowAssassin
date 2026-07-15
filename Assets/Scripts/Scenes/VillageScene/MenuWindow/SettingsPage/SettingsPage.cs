using System;
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
        masterSlider.onValueChanged.AddListener(OnSliderValueChanged);
        bgmSlider.onValueChanged.AddListener(OnSliderValueChanged);
        sfxSlider.onValueChanged.AddListener(OnSliderValueChanged);

        saveButton.onClick.AddListener(OnSaveButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnSliderValueChanged(float arg0)
    {
        AudioService.Instance.SetAudioSourceVolume(masterSlider.value, bgmSlider.value, sfxSlider.value);
    }

    void OnEnable()
    {
        // 父物体的启用也会调用子物体的 OnEnable() 函数
        SetSlidersValue();
    }
    private void OnSaveButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIConfirm);
        PersistentService.Instance.SetGameConfigVolume(masterSlider.value, bgmSlider.value, sfxSlider.value);
        // 实时音量已由 Slider onValueChanged 处理，无需重复调用
    }
    private void OnCancelButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UICancel);
        SetSlidersValue();
    }
    /// <summary>
    /// 恢复 Slider 到已保存的音量值
    /// </summary>
    private void SetSlidersValue()
    {
        PersistentService.Instance.GetVolume(out float masterVolume, out float bgmVolume, out float sfxVolume);
        masterSlider.value = masterVolume;
        bgmSlider.value = bgmVolume;
        sfxSlider.value = sfxVolume;
    }
}
