using UnityEngine;
using UnityEditor;
using System.IO;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor;

public class HandleDllTool
{
    private const string MenuPath = "HybridCLR/编译并复制 HotUpdate.dll";
    private const string SourceDllPath = "HybridCLRData/HotUpdateDlls/StandaloneWindows64/HotUpdate.dll";
    private const string TargetFolder = "Assets/Dlls";
    private const string TargetFileName = "HotUpdate.dll.bytes";

    // 旧文件列表（需要删除的）
    private static readonly string[] oldFiles = new string[]
    {
        "Assets/Dlls/HotUpdate.dll.bytes",
        "Assets/Dlls/HotUpdate.dll.bytes.meta"
    };

    /// <summary>
    /// 菜单栏入口，点击执行更新操作
    /// </summary>
    [MenuItem(MenuPath)]
    public static void CompileAndCopyHotUpdateDll()
    {
        // 调用 HybridCLR 的 API，执行编译：
        // 这会根据当前激活的 BuildTarget 编译热更新 DLL，
        // 并将结果输出到 HybridCLRData/HotUpdateDlls/{platform}/ 目录下 [citation:1][citation:3][citation:6]
        CompileDllCommand.CompileDllActiveBuildTarget();
        // CompileDllCommand.CompileDll(BuildTarget.StandaloneWindows64);
        Debug.Log($"[{nameof(HandleDllTool)}] HybridCLR 编译完成。");

        // 检查源文件是否存在
        if (!File.Exists(SourceDllPath))
        {
            EditorUtility.DisplayDialog(
                "错误",
                $"源文件不存在！\n请检查路径：{SourceDllPath}",
                "确定"
            );
            Debug.LogError($"[{nameof(HandleDllTool)}] 源文件不存在：{SourceDllPath}");
            return;
        }

        try
        {
            // 1. 删除旧文件
            DeleteOldFiles();

            // 2. 确保目标文件夹存在
            EnsureTargetFolderExists();

            // 3. 复制并重命名
            CopyAndRenameDll();

            // 4. 刷新 AssetDatabase
            AssetDatabase.Refresh();

            // 5. 提示成功
            // EditorUtility.DisplayDialog(
            //     "成功",
            //     $"HotUpdate.dll 已编译并复制成功！\n目标位置：{TargetFolder}/{TargetFileName}",
            //     "确定"
            // );

            Debug.Log($"[{nameof(HandleDllTool)}] 更新成功！源：{SourceDllPath} -> 目标：{TargetFolder}/{TargetFileName}");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog(
                "错误",
                $"更新失败：{ex.Message}",
                "确定"
            );
            Debug.LogError($"[{nameof(HandleDllTool)}] 更新失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 删除旧文件
    /// </summary>
    private static void DeleteOldFiles()
    {
        foreach (string filePath in oldFiles)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[{nameof(HandleDllTool)}] 已删除：{filePath}");
            }
            else
            {
                Debug.Log($"[{nameof(HandleDllTool)}] 文件不存在，跳过删除：{filePath}");
            }
        }
    }

    /// <summary>
    /// 确保目标文件夹存在
    /// </summary>
    private static void EnsureTargetFolderExists()
    {
        if (!Directory.Exists(TargetFolder))
        {
            Directory.CreateDirectory(TargetFolder);
            Debug.Log($"[{nameof(HandleDllTool)}] 创建文件夹：{TargetFolder}");
        }
    }

    /// <summary>
    /// 复制 Dll 并重命名
    /// </summary>
    private static void CopyAndRenameDll()
    {
        string targetFullPath = Path.Combine(TargetFolder, TargetFileName);

        // 执行复制
        File.Copy(SourceDllPath, targetFullPath, overwrite: true);
        Debug.Log($"[{nameof(HandleDllTool)}] 已复制：{SourceDllPath} -> {targetFullPath}");
    }

    /// <summary>
    /// 菜单项是否可用（可选配置）
    /// 如果源文件不存在，菜单项显示为灰色不可点击。
    /// </summary>
    [MenuItem(MenuPath, true)]
    public static bool ValidateUpdateHotUpdateDll()
    {
        return File.Exists(SourceDllPath);
    }
}