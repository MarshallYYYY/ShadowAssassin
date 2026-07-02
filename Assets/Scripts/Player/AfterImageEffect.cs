using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于对象池的残影管理器。
/// 挂载在角色身上，预创建固定数量的残影快照，循环复用。
/// 使用方法：afterImage.StartEffect() 开始 / afterImage.StopEffect() 停止
/// </summary>
public class AfterImageEffect : MonoBehaviour
{
    [Header("池设置")]
    [SerializeField] private int poolSize = 10;
    // [SerializeField] private float snapshotInterval = 0.05f; // 多少秒拍一片
    [SerializeField] private float fadeDuration = 0.5f;      // 淡出时长

    private SkinnedMeshRenderer[] sourceRenderers;
    private readonly List<Snapshot> pool = new();
    private int poolIndex;
    // private float snapshotTimer;
    // private bool isActive;

    private class Snapshot
    {
        public GameObject Go;
        public MeshFilter[] MeshFilters;
        public MeshRenderer[] MeshRenderers;
        public MaterialPropertyBlock MaterialPropertyBlock;
        public float Alpha = 0.1f;
        public float RemainingLife;
    }

    void Awake()
    {
        sourceRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        BuildPool();
    }

    #region 池构建（仅 Awake 一次）
    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var snap = new Snapshot
            {
                Go = new GameObject($"AfterImage_{i}"),
                MeshFilters = new MeshFilter[sourceRenderers.Length],
                MeshRenderers = new MeshRenderer[sourceRenderers.Length],
                MaterialPropertyBlock = new MaterialPropertyBlock(),
            };

            for (int j = 0; j < sourceRenderers.Length; j++)
            {
                var src = sourceRenderers[j];
                var sub = new GameObject($"Part_{j}");
                sub.transform.SetParent(snap.Go.transform, false);

                var mf = sub.AddComponent<MeshFilter>();
                mf.mesh = new Mesh();
                mf.mesh.MarkDynamic(); // 标记动态 → GPU 上传更快

                var mr = sub.AddComponent<MeshRenderer>();
                mr.sharedMaterial = src.sharedMaterial;

                snap.MeshFilters[j] = mf;
                snap.MeshRenderers[j] = mr;
            }

            snap.Go.SetActive(false);
            pool.Add(snap);
        }
    }
    #endregion

    #region 公共接口
    public void StartEffect()
    {
        // isActive = true;
        // snapshotTimer = 0f;
        TakeSnapshot();
    }
    public void StopEffect()
    {
        // isActive = false;
    }
    #endregion

    void Update()
    {
        // if (isActive)
        // {
        //     snapshotTimer += Time.deltaTime;
        //     if (snapshotTimer >= snapshotInterval)
        //     {
        //         snapshotTimer -= snapshotInterval;
        //         TakeSnapshot();
        //     }
        // }

        // 淡出所有存活的快照
        float dt = Time.deltaTime;
        float fadeSpeed = 1f / fadeDuration;
        for (int i = 0; i < pool.Count; i++)
        {
            var snap = pool[i];
            if (!snap.Go.activeSelf) continue;

            snap.RemainingLife -= dt;
            if (snap.RemainingLife <= 0f)
            {
                snap.Go.SetActive(false);
                snap.Alpha = 1f;
                continue;
            }

            snap.Alpha -= fadeSpeed * dt;
            snap.Alpha = Mathf.Max(snap.Alpha, 0f);

            snap.MaterialPropertyBlock.SetColor("_Color", new Color(1f, 1f, 1f, snap.Alpha));
            snap.MaterialPropertyBlock.SetColor("_BaseColor", new Color(1f, 1f, 1f, snap.Alpha));
            for (int j = 0; j < snap.MeshRenderers.Length; j++)
                snap.MeshRenderers[j].SetPropertyBlock(snap.MaterialPropertyBlock);
        }
    }

    private void TakeSnapshot()
    {
        var snap = pool[poolIndex];
        poolIndex = (poolIndex + 1) % poolSize;

        snap.Go.SetActive(true);
        snap.Alpha = 1f;
        snap.RemainingLife = fadeDuration + 0.15f;

        for (int i = 0; i < sourceRenderers.Length; i++)
        {
            var src = sourceRenderers[i];
            src.BakeMesh(snap.MeshFilters[i].mesh); // 每 0.05 秒一次

            Transform t = src.transform;
            snap.MeshFilters[i].transform.SetPositionAndRotation(t.position, t.rotation);
        }

        snap.MaterialPropertyBlock.SetColor("_Color", Color.white);
        snap.MaterialPropertyBlock.SetColor("_BaseColor", Color.white);
        for (int i = 0; i < snap.MeshRenderers.Length; i++)
            snap.MeshRenderers[i].SetPropertyBlock(snap.MaterialPropertyBlock);
    }

    void OnDestroy()
    {
        foreach (var snap in pool)
        {
            if (snap.Go != null)
            {
                foreach (var mf in snap.MeshFilters)
                    if (mf.mesh != null) Destroy(mf.mesh);
                Destroy(snap.Go);
            }
        }
        pool.Clear();
    }
}