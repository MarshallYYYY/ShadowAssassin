using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioService : BaseService<AudioService>
{
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource sfx;
    public override void Init()
    {
        base.Init();

        PersistentService.Instance.GetVolume(out float masterVolume, out float bgmVolume, out float sfxVolume);
        bgm.volume = masterVolume * bgmVolume;
        sfx.volume = masterVolume * sfxVolume;
    }
    public void SetAudioSourceVolume(float masterVolume, float bgmVolume, float sfxVolume)
    {
        bgm.volume = masterVolume * bgmVolume;
        sfx.volume = masterVolume * sfxVolume;
    }
    public void PlayBgm()
    {
        bgm.Play();
    }
    public void PlaySfx()
    {

    }
}
