using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;

/// <summary>
/// UV «как на референсе»: линии клетки = по радиусу (сегменты) + по дуге (концентрично).
/// Меню: WonderDoor → Apply Arc Floor UVs
/// </summary>
public class ArcFloorUV : ScriptableWizard
{
    [Tooltip("Клеток по ширине пола (от внутреннего края к внешнему)")]
    public float tilesAcross = 2f;

    [Tooltip("Если вкл — вдоль дуги ровно 1 клетка на каждый сегмент (Segments Per Unit)")]
    public bool oneCellPerSegment = true;

    [Tooltip("Клеток вдоль дуги (если One Cell Per Segment выкл)")]
    public float tilesAlong = 12f;

    [Tooltip("Поменять U/V")]
    public bool swapUV;

    [Tooltip("Сдвиг центра окружности (локально). Если клетка «плывёт» — подвигай X/Z")]
    public Vector3 centerOffset;

    [Tooltip("Выключить Spline Extrude после UV, чтобы он не затёр развёртку")]
    public bool disableSplineExtrude = true;

    // Старый wizard — для Live тайлинга используй:
    // WonderDoor → FIX Floor Texture Along Arc NOW (компонент ArcFloorUVLive)

    [MenuItem("WonderDoor/Apply Arc Floor UVs (legacy polar)")]
    static void Open()
    {
        var w = DisplayWizard<ArcFloorUV>("Arc Floor UVs — legacy", "Apply");
        w.tilesAcross = 2f;
        w.oneCellPerSegment = true;
        w.disableSplineExtrude = true;
    }

    void OnWizardCreate() => ApplyToSelection();

    [MenuItem("WonderDoor/Apply Arc Floor UVs (legacy polar)", true)]
    static bool Validate() => Selection.activeGameObject != null;

    public void ApplyToSelection()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Arc Floor UVs", "Выдели Floor в Hierarchy.", "OK");
            return;
        }

        // UV Editor / Auto больше не нужны — закрывать вручную.
        if (disableSplineExtrude)
        {
            var extrude = go.GetComponent<SplineExtrude>();
            if (extrude != null)
            {
                Undo.RecordObject(extrude, "Disable Spline Extrude");
                extrude.enabled = false;
                EditorUtility.SetDirty(extrude);
            }
        }

        var pb = go.GetComponent<ProBuilderMesh>();
        if (pb != null)
        {
            ApplyProBuilder(pb);
            return;
        }

        var mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            EditorUtility.DisplayDialog("Arc Floor UVs", "Нужен MeshFilter / ProBuilder на объекте.", "OK");
            return;
        }

        ApplyMeshFilter(mf);
    }

    void ApplyProBuilder(ProBuilderMesh pb)
    {
        Undo.RecordObject(pb, "Arc Floor UVs");

        var positions = pb.positions;
        int segments;
        var uvs = BuildUVs(positions, out segments);

        var list = new List<Vector4>(uvs.Length);
        for (int i = 0; i < uvs.Length; i++)
            list.Add(uvs[i]);

        pb.SetUVs(0, list);
        pb.ToMesh();
        pb.Refresh(RefreshMask.UV);
        EditorUtility.SetDirty(pb);

        Debug.Log($"[ArcFloorUV] Готово «{pb.name}»: сегментов ~{segments}, " +
                  $"клеток вдоль={(oneCellPerSegment ? segments : tilesAlong)}, поперёк={tilesAcross}.");
    }

    void ApplyMeshFilter(MeshFilter mf)
    {
        var src = mf.sharedMesh;
        // Копия, чтобы не портить ассет Spline Extrude
        var mesh = Object.Instantiate(src);
        mesh.name = src.name + "_ArcUV";

        Undo.RecordObject(mf, "Arc Floor UVs");
        int segments;
        mesh.uv = BuildUVs(mesh.vertices, out segments);
        mf.sharedMesh = mesh;
        EditorUtility.SetDirty(mf);

        Debug.Log($"[ArcFloorUV] Готово «{mf.name}»: сегментов ~{segments}.");
    }

    Vector2[] BuildUVs(IList<Vector3> positions, out int segmentCount)
    {
        segmentCount = 1;
        int n = positions.Count;
        if (n == 0) return new Vector2[0];

        var bounds = new Bounds(positions[0], Vector3.zero);
        for (int i = 1; i < n; i++)
            bounds.Encapsulate(positions[i]);

        // Центр подковы ≈ центр bounds (для C/U обычно ок)
        Vector2 center = new Vector2(bounds.center.x, bounds.center.z)
                         + new Vector2(centerOffset.x, centerOffset.z);

        float rMin = float.MaxValue, rMax = float.MinValue;
        var angles = new float[n];
        var radii = new float[n];

        for (int i = 0; i < n; i++)
        {
            float dx = positions[i].x - center.x;
            float dz = positions[i].z - center.y;
            radii[i] = Mathf.Sqrt(dx * dx + dz * dz);
            angles[i] = Mathf.Atan2(dz, dx);
            if (radii[i] < rMin) rMin = radii[i];
            if (radii[i] > rMax) rMax = radii[i];
        }

        // Разрыв C-формы
        var sorted = (float[])angles.Clone();
        System.Array.Sort(sorted);
        float bestGap = 0f;
        float gapAfter = sorted[0];
        for (int i = 0; i < sorted.Length - 1; i++)
        {
            float gap = sorted[i + 1] - sorted[i];
            if (gap > bestGap)
            {
                bestGap = gap;
                gapAfter = sorted[i + 1];
            }
        }

        float wrapGap = (sorted[0] + Mathf.PI * 2f) - sorted[sorted.Length - 1];
        if (wrapGap > bestGap)
        {
            bestGap = wrapGap;
            gapAfter = sorted[0];
        }

        float aStart = gapAfter;
        float arcSpan = Mathf.Max(0.0001f, Mathf.PI * 2f - bestGap);
        float width = Mathf.Max(0.0001f, rMax - rMin);

        // Число сегментов = сколько разных углов (узкие клинья Spline Extrude)
        var unique = new List<float>();
        const float eps = 0.02f; // ~1°
        for (int i = 0; i < n; i++)
        {
            float a = angles[i] - aStart;
            while (a < 0f) a += Mathf.PI * 2f;
            while (a >= Mathf.PI * 2f) a -= Mathf.PI * 2f;
            if (a > arcSpan + 0.001f) continue;

            bool found = false;
            for (int u = 0; u < unique.Count; u++)
            {
                if (Mathf.Abs(unique[u] - a) < eps)
                {
                    found = true;
                    break;
                }
            }
            if (!found) unique.Add(a);
        }

        segmentCount = Mathf.Max(1, unique.Count - 1); // интервалов между лучами
        float along = oneCellPerSegment ? segmentCount : tilesAlong;

        var uvs = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            float a = angles[i] - aStart;
            while (a < 0f) a += Mathf.PI * 2f;
            while (a >= Mathf.PI * 2f) a -= Mathf.PI * 2f;

            // U — концентрические дуги (поперёк пола)
            // V — вдоль сегментов (радиальные линии = границы клеток)
            float u = ((radii[i] - rMin) / width) * tilesAcross;
            float v = (a / arcSpan) * along;

            if (swapUV) (u, v) = (v, u);
            uvs[i] = new Vector2(u, v);
        }

        return uvs;
    }
}
