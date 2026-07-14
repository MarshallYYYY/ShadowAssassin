using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using System;

public class AudioService : BaseService<AudioService>
{
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource sfx;

    /// <summary>
    /// 已加载的 AudioClip 缓存，避免重复加载
    /// </summary>
    private readonly Dictionary<string, AudioClip> audioCache = new();

    private const string BgmPathPrefix = "Assets/Audios/BGM/";
    private const string SfxPathPrefix = "Assets/Audios/SFX/";

    public override void Init()
    {
        base.Init();

        PersistentService.Instance.GetVolume(out float masterVolume, out float bgmVolume, out float sfxVolume);
        bgm.volume = masterVolume * bgmVolume;
        sfx.volume = masterVolume * sfxVolume;
    }

    public void SetAudioSourceVolume(float masterVolume, float bgmVolume, float floatVolume)
    {
        bgm.volume = masterVolume * bgmVolume;
        sfx.volume = masterVolume * floatVolume;
    }

    #region 播放/停止 BGM/SFX 音频
    /// <summary>
    /// 播放背景音乐（异步加载，带缓存）
    /// </summary>
    /// <param name="assetName">YooAsset 中的资源名称（如 "VillageBGM"）</param>
    public void PlayBgm(string assetName)
    {
        PlayAudio(BgmPathPrefix + assetName, clip =>
        {
            bgm.clip = clip;
            bgm.Play();
        });
    }

    /// <summary>
    /// 播放音效（异步加载，带缓存）
    /// </summary>
    /// <param name="assetName">YooAsset 中的资源名称（如 "Enemy_Attack"）</param>
    public void PlaySfx(string assetName)
    {
        PlayAudio(SfxPathPrefix + assetName, clip => sfx.PlayOneShot(clip));
    }

    public void StopBgm()
    {
        bgm.Stop();
    }
    #endregion

    #region 异步加载
    /// <summary>
    /// 播放音频（带缓存，已缓存则同步回调，未缓存则异步加载）
    /// </summary>
    private void PlayAudio(string fullPath, Action<AudioClip> onLoaded)
    {
        // 已缓存：直接回调，无需协程
        if (audioCache.TryGetValue(fullPath, out AudioClip cachedClip))
        {
            onLoaded?.Invoke(cachedClip);
            return;
        }

        StartCoroutine(PlayAudioCoroutine(fullPath, onLoaded));
    }

    private IEnumerator PlayAudioCoroutine(string fullPath, Action<AudioClip> onLoaded)
    {
        ResourcePackage package = YooAssets.GetPackage(YooAssetConstants.PackageName);
        AssetHandle handle = package.LoadAssetAsync<AudioClip>(fullPath);
        yield return handle;

        AudioClip clip = handle.AssetObject as AudioClip;
        if (clip != null)
        {
            audioCache[fullPath] = clip;
            onLoaded?.Invoke(clip);
        }
        else
        {
            Debug.LogWarning($"[AudioService] 音频加载失败: {fullPath}");
        }
    }
    #endregion
}
