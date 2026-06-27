using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage3D : MonoBehaviour
{
    private readonly List<GameObject> clones = new();
    [SerializeField] private float lifeTime = 2f;
    void Awake()
    {
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = renderers[i];
            GameObject go = new();
            go.AddComponent<MeshRenderer>().material = renderer.material;
            Mesh mesh = new();
            renderer.BakeMesh(mesh);
            go.AddComponent<MeshFilter>().mesh = mesh;
            clones.Add(go);

            go.transform.SetPositionAndRotation(renderer.transform.position, renderer.transform.rotation);
        }
    }
    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            // Destroy(this);
        }
    }
    void OnDestroy()
    {
        foreach (GameObject go in clones)
        {
            Destroy(go);
        }
        clones.Clear();
    }
}
