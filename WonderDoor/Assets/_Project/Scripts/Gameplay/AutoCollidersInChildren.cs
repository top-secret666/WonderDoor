using UnityEngine;

// Adds MeshColliders to every child mesh that has no collider (spline floors/walls), so the player can't fall through.
public class AutoCollidersInChildren : MonoBehaviour
{
    void Start()
    {
        foreach (var mf in GetComponentsInChildren<MeshFilter>(true))
        {
            if (!mf.sharedMesh || mf.GetComponent<Collider>() != null) continue;
            if (mf.GetComponentInParent<Light>() != null || mf.GetComponentInChildren<Light>() != null) continue;
            if (mf.name.Contains("Lamp") || mf.transform.parent && mf.transform.parent.name.Contains("Lamp")) continue;
            mf.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
        }
    }
}
