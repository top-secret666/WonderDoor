using UnityEngine;

// Добавляет MeshCollider по сгенерированному мешу (сплайн-пол), чтобы игрок не проваливался.
[RequireComponent(typeof(MeshFilter))]
public class AutoMeshCollider : MonoBehaviour
{
    void Start()
    {
        var mf = GetComponent<MeshFilter>(); if (!mf.sharedMesh) return;
        var mc = GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = null; mc.sharedMesh = mf.sharedMesh;
    }
}
