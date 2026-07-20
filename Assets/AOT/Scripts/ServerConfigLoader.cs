using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 服务器地址配置加载器：从 StreamingAssets/ServerConfig.json 读取服务器地址
/// 构建后可直接修改游戏目录中的该文件切换服务器，无需重新编译
/// </summary>
public static class ServerConfigLoader
{
    public static string DefaultHostServerURL { get; private set; }
    public static string FallBackHostServerURL { get; private set; }

    static ServerConfigLoader()
    {
        Load();
    }

    private static void Load()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "ServerConfig.json");
        string fallbackDefault = "http://127.0.0.1/ShadowAssassin/PC";
        string fallbackFallback = "http://127.0.0.1/ShadowAssassin/PC";

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                ServerConfig config = JsonUtility.FromJson<ServerConfig>(json);
                DefaultHostServerURL = string.IsNullOrEmpty(config.DefaultHostServerURL) ? fallbackDefault : config.DefaultHostServerURL;
                FallBackHostServerURL = string.IsNullOrEmpty(config.FallBackHostServerURL) ? fallbackFallback : config.FallBackHostServerURL;
                Debug.Log($"[ServerConfig] 加载成功: {DefaultHostServerURL}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ServerConfig] 解析失败，使用默认值: {e.Message}");
                DefaultHostServerURL = fallbackDefault;
                FallBackHostServerURL = fallbackFallback;
            }
        }
        else
        {
            Debug.LogWarning("[ServerConfig] ServerConfig.json 不存在，使用默认值");
            DefaultHostServerURL = fallbackDefault;
            FallBackHostServerURL = fallbackFallback;
        }
    }

    [Serializable]
    private class ServerConfig
    {
        public string DefaultHostServerURL;
        public string FallBackHostServerURL;
    }
}
