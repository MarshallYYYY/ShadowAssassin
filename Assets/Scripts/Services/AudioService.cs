using System.Collections.Generic;
using UnityEngine;
using YooAsset;

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
    /// 播放背景音乐（同步加载，带缓存）
    /// </summary>
    /// <param name="assetName">YooAsset 中的资源名称（如 "VillageBGM"）</param>
    public void PlayBgm(string assetName)
    {
        AudioClip clip = LoadAudio(BgmPathPrefix + assetName);
        if (clip != null)
        {
            bgm.clip = clip;
            bgm.Play();
        }
    }

    /// <summary>
    /// 播放音效（同步加载，带缓存）
    /// </summary>
    /// <param name="assetName">YooAsset 中的资源名称（如 "Enemy_Attack"）</param>
    public void PlaySfx(string assetName)
    {
        AudioClip clip = LoadAudio(SfxPathPrefix + assetName);
        if (clip != null)
        {
            sfx.PlayOneShot(clip);
        }
    }

    public void StopBgm()
    {
        bgm.Stop();
    }
    #endregion

    #region 加载
    /// <summary>
    /// 同步加载 AudioClip（带缓存）
    /// </summary>
    private AudioClip LoadAudio(string fullPath)
    {
        if (audioCache.TryGetValue(fullPath, out AudioClip cachedClip))
            return cachedClip;

        ResourcePackage package = YooAssets.GetPackage(YooAssetConstants.PackageName);
        AssetHandle handle = package.LoadAssetSync<AudioClip>(fullPath);
        AudioClip clip = handle.AssetObject as AudioClip;

        if (clip != null)
        {
            audioCache[fullPath] = clip;
        }
        else
        {
            Debug.LogWarning($"[AudioService] 音频加载失败: {fullPath}");
        }

        return clip;
    }
    #endregion
}
