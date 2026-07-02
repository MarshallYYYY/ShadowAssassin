using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class OpenSceneTool : EditorWindow
{
    private const string YooAssetScene = nameof(YooAssetScene);
    private const string LoadMetadataScene = nameof(LoadMetadataScene);
    private const string StartScene = nameof(StartScene);
    private const string VillageScene = nameof(VillageScene);

    // &O 表示快捷键 Alt + O，如果你想改成 Ctrl + Shift + O，可以写成 %#O（% = Ctrl，# = Shift）。
    // 添加菜单项：Tools > 打开目标场景
    [MenuItem("Tools/Open FirstScene %L")]
    private static void OpenFirstScene()
    {
        OpenTargetScene(YooAssetScene);
    }
    private static void OpenTargetScene(string targetSceneName)
    {
        // 检查当前是否有未保存的修改
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 尝试打开场景（场景需要在 Build Settings 中，或在 Assets 目录下）
            string scenePath = GetScenePath(targetSceneName);

            if (!string.IsNullOrEmpty(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
                Debug.Log($"已打开场景: {targetSceneName}");
            }
            else
            {
                Debug.LogError($"未找到名为 {targetSceneName} 的场景，请检查场景名称是否正确。");
            }
        }
    }

    // 根据场景名称查找完整路径
    private static string GetScenePath(string sceneName)
    {
        // 获取项目中所有场景的 GUID
        string[] guids = AssetDatabase.FindAssets("t:Scene");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // 获取文件名（不含扩展名）
            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (name == sceneName)
            {
                return path;
            }
        }

        return null;
    }

    // ========== 可选：在窗口中添加按钮 ==========
    [MenuItem("Tools/场景打开器")]
    private static void ShowWindow()
    {
        GetWindow<OpenSceneTool>("场景打开器");
    }

    private void OnGUI()
    {
        // GUILayout.Label("快速打开场景", EditorStyles.boldLabel);

        // 显示当前目标场景
        // EditorGUILayout.LabelField("目标场景", targetSceneName);

        if (GUILayout.Button($"打开 {YooAssetScene} 场景"))
        {
            OpenTargetScene(YooAssetScene);
        }

        if (GUILayout.Button($"打开 {LoadMetadataScene} 场景"))
        {
            OpenTargetScene(LoadMetadataScene);
        }

        if (GUILayout.Button($"打开 {StartScene} 场景"))
        {
            OpenTargetScene(StartScene);
        }

        if (GUILayout.Button($"打开 {VillageScene} 场景"))
        {
            OpenTargetScene(VillageScene);
        }

        // 允许用户手动输入场景名（可选）
        // EditorGUILayout.Space();
        // string newName = EditorGUILayout.TextField("场景名", targetSceneName);
        // if (newName != targetSceneName && !string.IsNullOrEmpty(newName))
        // {
        //     targetSceneName = newName;
        // }
    }
}