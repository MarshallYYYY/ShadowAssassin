using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于对象池的残影管理器。
/// 挂载在角色身上，预创建固定数量的残影快照，循环复用。
/// 使用方法：afterImage.StartEffect() 开始 / afterImage.StopEffect() 停止
/// </summary>
public class AfterImage3D2 : MonoBehaviour
{
    [Header("池设置")]
    [SerializeField] private int poolSize = 10;
    // [SerializeField] private float snapshotInterval = 0.05f; // 多少秒拍一片
    [SerializeField] private float fadeDuration = 0.5f;      // 淡出时长

    private SkinnedMeshRenderer[] sourceRenderers;
    private readonly List<Snapshot> pool = new();
    private int poolIndex;
    // private float snapshotTimer;
    private bool isActive;

    private class Snapshot
    {
        public GameObject go;
        public MeshFilter[] meshFilters;
        public MeshRenderer[] meshRenderers;
        public MaterialPropertyBlock mpb;
        public float alpha = 1f;
        public float remainingLife;
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
                go = new GameObject($"AfterImage_{i}"),
                meshFilters = new MeshFilter[sourceRenderers.Length],
                meshRenderers = new MeshRenderer[sourceRenderers.Length],
                mpb = new MaterialPropertyBlock(),
            };

            for (int j = 0; j < sourceRenderers.Length; j++)
            {
                var src = sourceRenderers[j];
                var sub = new GameObject($"Part_{j}");
                sub.transform.SetParent(snap.go.transform, false);

                var mf = sub.AddComponent<MeshFilter>();
                mf.mesh = new Mesh();
                mf.mesh.MarkDynamic(); // 标记动态 → GPU 上传更快

                var mr = sub.AddComponent<MeshRenderer>();
                mr.sharedMaterial = src.sharedMaterial;

                snap.meshFilters[j] = mf;
                snap.meshRenderers[j] = mr;
            }

            snap.go.SetActive(false);
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
        isActive = false;
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
            if (!snap.go.activeSelf) continue;

            snap.remainingLife -= dt;
            if (snap.remainingLife <= 0f)
            {
                snap.go.SetActive(false);
                snap.alpha = 1f;
                continue;
            }

            snap.alpha -= fadeSpeed * dt;
            snap.alpha = Mathf.Max(snap.alpha, 0f);

            snap.mpb.SetColor("_Color", new Color(1f, 1f, 1f, snap.alpha));
            snap.mpb.SetColor("_BaseColor", new Color(1f, 1f, 1f, snap.alpha));
            for (int j = 0; j < snap.meshRenderers.Length; j++)
                snap.meshRenderers[j].SetPropertyBlock(snap.mpb);
        }
    }

    private void TakeSnapshot()
    {
        var snap = pool[poolIndex];
        poolIndex = (poolIndex + 1) % poolSize;

        snap.go.SetActive(true);
        snap.alpha = 1f;
        snap.remainingLife = fadeDuration + 0.15f;

        for (int i = 0; i < sourceRenderers.Length; i++)
        {
            var src = sourceRenderers[i];
            src.BakeMesh(snap.meshFilters[i].mesh); // 每 0.05 秒一次

            Transform t = src.transform;
            snap.meshFilters[i].transform.SetPositionAndRotation(t.position, t.rotation);
        }

        snap.mpb.SetColor("_Color", Color.white);
        snap.mpb.SetColor("_BaseColor", Color.white);
        for (int i = 0; i < snap.meshRenderers.Length; i++)
            snap.meshRenderers[i].SetPropertyBlock(snap.mpb);
    }

    void OnDestroy()
    {
        foreach (var snap in pool)
        {
            if (snap.go != null)
            {
                foreach (var mf in snap.meshFilters)
                    if (mf.mesh != null) Destroy(mf.mesh);
                Destroy(snap.go);
            }
        }
        pool.Clear();
    }
}