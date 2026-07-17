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
    [SerializeField] private Slider lookSlider;

    [Header("Button")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;

    private const float MinSensitivity = 0.01f;
    private const float MaxSensitivity = 3f;
    void Awake()
    {
        masterSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
        bgmSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
        sfxSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);

        lookSlider.onValueChanged.AddListener(OnLookSliderValueChanged);

        saveButton.onClick.AddListener(OnSaveButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }
    private void OnVolumeSliderValueChanged(float arg0)
    {
        AudioService.Instance.SetAudioSourceVolume(masterSlider.value, bgmSlider.value, sfxSlider.value);
    }
    private float lookSensitivity;
    private void OnLookSliderValueChanged(float arg0)
    {
        lookSensitivity = Mathf.Lerp(MinSensitivity, MaxSensitivity, arg0);
        Debug.Log(lookSensitivity);
    }

    void OnEnable()
    {
        // 父物体的启用也会调用子物体的 OnEnable() 函数
        SetSlidersValue();
    }
    private void OnSaveButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UIConfirm);
        PersistentService.Instance.SetGameConfig(masterSlider.value, bgmSlider.value, sfxSlider.value, lookSensitivity);
        // 实时音量已由 Slider onValueChanged 处理，无需重复调用
    }
    private void OnCancelButtonClicked()
    {
        AudioService.Instance.PlaySfx(AudioConstants.UICancel);
        SetSlidersValue();
    }
    /// <summary>
    /// 恢复 Slider 到已保存的数值
    /// </summary>
    private void SetSlidersValue()
    {
        PersistentService.Instance.GetVolume(out float masterVolume, out float bgmVolume, out float sfxVolume);
        masterSlider.value = masterVolume;
        bgmSlider.value = bgmVolume;
        sfxSlider.value = sfxVolume;

        lookSlider.value = Mathf.InverseLerp(MinSensitivity, MaxSensitivity, PersistentService.Instance.LookSensitivity);
    }
}