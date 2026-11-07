using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// Meadow: fluffy grass tufts and flowers on top of this floor.
/// The mesh is built automatically and not saved (it is not stored in the scene).
[ExecuteAlways]
[DisallowMultipleComponent]
public class MeadowField : MonoBehaviour
{
    public Material material;

    [Header("Density (per square metre)")]
    [Range(0f, 40f)] public float grassPerM2 = 14f;
    [Range(0f, 10f)] public float flowersPerM2 = 1.2f;
    [Tooltip("Share of lavender flowers (0..1)")]
    [Range(0f, 1f)] public float lavenderShare = 0.2f;
    public int seed = 12345;
    [Tooltip("Safety cap on tufts so the editor does not lag")]
    public int maxTufts = 40000;

    [Header("Sizes (metres)")]
    public Vector2 grassHeight = new Vector2(0.35f, 0.75f);
    public Vector2 flowerHeight = new Vector2(0.25f, 0.5f);
    [Tooltip("Margin from walls")]
    public float edgePadding = 0.15f;

    [Header("Clumping")]
    [Tooltip("Size of grass clumps; larger means bigger patches")]
    public float clumpScale = 1.6f;
    [Range(0f, 1f)] public float clumpStrength = 0.6f;

    const string ChildName = "~MeadowGenerated";
    GameObject _child;
    Mesh _mesh;

    void OnEnable()
    {
#if UNITY_EDITOR
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)) return;
        UnityEditor.EditorApplication.delayCall += SafeRebuild;
#endif
        if (Application.isPlaying) Rebuild();
    }

    void OnDisable() { Clear(); }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)) return;
        UnityEditor.EditorApplication.delayCall += SafeRebuild;
    }
#endif

    void SafeRebuild()
    {
        if (this == null || !isActiveAndEnabled) return;
        Rebuild();
    }

    void Clear()
    {
        if (_child != null)
        {
            if (Application.isPlaying) Destroy(_child); else DestroyImmediate(_child);
        }
        if (_mesh != null)
        {
            if (Application.isPlaying) Destroy(_mesh); else DestroyImmediate(_mesh);
        }
        _child = null; _mesh = null;
        var old = transform.Find(ChildName);
        if (old != null) DestroyImmediate(old.gameObject);
    }

    [Tooltip("Off: grass only on this object's own floor (child pieces stay bare). On: on child pieces too")]
    public bool includeChildren = false;

    bool GetFloorBounds(out Bounds b)
    {
        b = new Bounds();
        bool has = false;
        var list = includeChildren ? GetComponentsInChildren<MeshRenderer>() : GetComponents<MeshRenderer>();
        foreach (var r in list)
        {
            if (r.gameObject.name == ChildName) continue;
            if (r.bounds.size.sqrMagnitude < 1e-6f) continue;
            if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
        }
        return has;
    }

    [ContextMenu("Rebuild Meadow")]
    public void Rebuild()
    {
        Clear();
        if (material == null) return;
        if (!GetFloorBounds(out var bounds)) return;

        _child = new GameObject(ChildName);
        _child.hideFlags = HideFlags.DontSave;
        _child.transform.SetParent(transform, false);
        var t = _child.transform;

        var rnd = new System.Random(seed);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        float minX = bounds.min.x + edgePadding, maxX = bounds.max.x - edgePadding;
        float minZ = bounds.min.z + edgePadding, maxZ = bounds.max.z - edgePadding;
        float y = bounds.max.y - 0.01f;
        float area = Mathf.Max(0, maxX - minX) * Mathf.Max(0, maxZ - minZ);

        var v = new List<Vector3>(); var n = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();

        Rect[] grassCells = { new Rect(0, .5f, .5f, .5f), new Rect(.5f, .5f, .5f, .5f), new Rect(0, 0, .5f, .5f) };
        Rect[] whiteFlowers = { new Rect(.5f, .25f, .25f, .25f), new Rect(.75f, .25f, .25f, .25f), new Rect(.75f, 0, .25f, .25f) };
        Rect lavender = new Rect(.5f, 0, .25f, .25f);

        float ox = R(0, 1000), oz = R(0, 1000);

        void Tuft(Vector3 p, float h, float w, Rect cell, int planes, float lean)
        {
            float rot = R(0, Mathf.PI);
            Vector3 leanDir = new Vector3(R(-1, 1), 0, R(-1, 1)).normalized * lean * h;
            for (int i = 0; i < planes; i++)
            {
                float a = rot + i * Mathf.PI / planes;
                Vector3 d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * (w * 0.5f);
                Vector3 up = Vector3.up * h + leanDir;
                int s = v.Count;
                v.Add(t.InverseTransformPoint(p - d));
                v.Add(t.InverseTransformPoint(p + d));
                v.Add(t.InverseTransformPoint(p + d + up));
                v.Add(t.InverseTransformPoint(p - d + up));
                Vector3 nn = t.InverseTransformDirection(Vector3.up).normalized;
                n.Add(nn); n.Add(nn); n.Add(nn); n.Add(nn);
                uv.Add(new Vector2(cell.xMin, cell.yMin)); uv.Add(new Vector2(cell.xMax, cell.yMin));
                uv.Add(new Vector2(cell.xMax, cell.yMax)); uv.Add(new Vector2(cell.xMin, cell.yMax));
                tri.Add(s); tri.Add(s + 2); tri.Add(s + 1);
                tri.Add(s); tri.Add(s + 3); tri.Add(s + 2);
            }
        }

        int grassN = Mathf.Min(maxTufts, Mathf.RoundToInt(area * grassPerM2));
        int tries = 0;
        for (int i = 0; i < grassN && tries < grassN * 4; tries++)
        {
            float x = R(minX, maxX), z = R(minZ, maxZ);
            float noise = Mathf.PerlinNoise(ox + x / clumpScale, oz + z / clumpScale);
            if (R(0, 1) > Mathf.Lerp(1f, noise * 1.6f, clumpStrength)) continue;
            i++;
            float h = R(grassHeight.x, grassHeight.y) * Mathf.Lerp(0.7f, 1.25f, noise);
            Tuft(new Vector3(x, y, z), h, h * R(1.1f, 1.6f), grassCells[rnd.Next(grassCells.Length)], 3, R(0, 0.15f));
        }

        int flowerN = Mathf.Min(maxTufts / 4, Mathf.RoundToInt(area * flowersPerM2));
        for (int i = 0; i < flowerN; i++)
        {
            float x = R(minX, maxX), z = R(minZ, maxZ);
            float h = R(flowerHeight.x, flowerHeight.y);
            Rect cell = R(0, 1) < lavenderShare ? lavender : whiteFlowers[rnd.Next(whiteFlowers.Length)];
            Tuft(new Vector3(x, y, z), h, h, cell, 2, R(0, 0.1f));
        }

        _mesh = new Mesh { name = "MeadowMesh", hideFlags = HideFlags.DontSave };
        _mesh.indexFormat = IndexFormat.UInt32;
        _mesh.SetVertices(v); _mesh.SetNormals(n); _mesh.SetUVs(0, uv); _mesh.SetTriangles(tri, 0);
        _mesh.RecalculateBounds();

        _child.AddComponent<MeshFilter>().sharedMesh = _mesh;
        var mr = _child.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.shadowCastingMode = ShadowCastingMode.Off;
    }
}
