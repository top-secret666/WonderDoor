using UnityEngine;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Не ломает меш: Spline Extrude остаётся включённым.
/// Каждый раз: Rebuild → масштабируем его UV (клетка по кругу) → тайлинг в Inspector сразу.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[DefaultExecutionOrder(100)]
public class ArcFloorUVLive : MonoBehaviour
{
    [Header("Тайлинг (крути — видно сразу)")]
    [Min(0.01f)] public float tilesAcross = 2f;
    [Min(0.01f)] public float tilesAlong = 8f;
    public bool swapUV;

    MeshFilter _mf;
    SplineExtrude _extrude;
    bool _applying;

    void OnEnable()
    {
        Cache();
        Apply();
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        EditorApplication.delayCall += () =>
        {
            if (this == null || !isActiveAndEnabled) return; // prefab assets / disabled objects have no extruded mesh
            Apply();
        };
#else
        Apply();
#endif
    }

    void Cache()
    {
        if (_mf == null) _mf = GetComponent<MeshFilter>();
        if (_extrude == null) _extrude = GetComponent<SplineExtrude>();
    }

    [ContextMenu("Apply / Restore Floor Now")]
    public void Apply()
    {
        if (_applying) return;
        _applying = true;
        try
        {
            Cache();

            if (_extrude == null)
            {
                Debug.LogError($"[ArcFloorUVLive] На «{name}» нет Spline Extrude — добавь компонент.", this);
                return;
            }

            // Тело пола = Extrude. Всегда включаем и пересобираем.
            if (!_extrude.enabled)
                _extrude.enabled = true;

            _extrude.Rebuild();

            var mesh = _mf != null ? _mf.sharedMesh : null;
            if (mesh == null || mesh.vertexCount == 0)
            {
                Debug.LogError("[ArcFloorUVLive] Меш пустой после Rebuild. Проверь Spline (есть ли точки) и Radius.", this);
                return;
            }

            var baseUv = mesh.uv;
            if (baseUv == null || baseUv.Length != mesh.vertexCount)
            {
                Debug.LogError("[ArcFloorUVLive] Extrude не дал UV.", this);
                return;
            }

            // V у Extrude = метры вдоль сплайна
            float vMax = 0.001f;
            for (int i = 0; i < baseUv.Length; i++)
                if (baseUv[i].y > vMax) vMax = baseUv[i].y;

            var uvs = new Vector2[baseUv.Length];
            float vScale = tilesAlong / vMax;

            for (int i = 0; i < uvs.Length; i++)
            {
                float u = baseUv[i].x * tilesAcross;
                float v = baseUv[i].y * vScale;
                if (swapUV) (u, v) = (v, u);
                uvs[i] = new Vector2(u, v);
            }

            mesh.uv = uvs;

            // Материал не множим тайлинг второй раз
            var mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = true;
                var block = new MaterialPropertyBlock();
                mr.GetPropertyBlock(block);
                block.SetVector("_BaseMap_ST", new Vector4(1f, 1f, 0f, 0f));
                block.SetVector("_MainTex_ST", new Vector4(1f, 1f, 0f, 0f));
                mr.SetPropertyBlock(block);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(_mf);
                EditorUtility.SetDirty(_extrude);
                SceneView.RepaintAll();
            }
#endif
        }
        finally
        {
            _applying = false;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ArcFloorUVLive))]
public class ArcFloorUVLiveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var live = (ArcFloorUVLive)target;

        EditorGUILayout.HelpBox(
            "Spline Extrude должен быть ВКЛЮЧЁН — это и есть тело пола.\n" +
            "Крути Tiles Across / Along — клетка по кругу, тайлинг сразу.\n" +
            "Если пол пропал — кнопка Restore ниже.",
            MessageType.Info);

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
            live.Apply();

        EditorGUILayout.Space(6);
        if (GUILayout.Button("RESTORE FLOOR + Apply UV", GUILayout.Height(32)))
        {
            live.Apply();
            EditorUtility.SetDirty(live);
        }
    }
}

public static class ArcFloorUVLiveMenu
{
    [MenuItem("WonderDoor/FIX Floor Texture Along Arc NOW")]
    static void FixNow()
    {
        SceneVisibilityManager.instance.EnableAllPicking();

        var go = Selection.activeGameObject;
        if (go == null || go.GetComponent<SplineExtrude>() == null)
            go = GameObject.Find("Floor");

        if (go == null)
        {
            EditorUtility.DisplayDialog("Floor", "Не найден объект Floor. Выдели его в Hierarchy.", "OK");
            return;
        }

        Selection.activeGameObject = go;

        // Вернуть Extrude
        var extrude = go.GetComponent<SplineExtrude>();
        if (extrude == null)
        {
            EditorUtility.DisplayDialog("Floor", "На Floor нет Spline Extrude — без него дуги не будет.", "OK");
            return;
        }

        Undo.RecordObject(extrude, "Enable Spline Extrude");
        extrude.enabled = true;
        // НЕ трогаем Scale / Radius — ширину и толщину пола задаёт только пользователь

        var live = go.GetComponent<ArcFloorUVLive>();
        if (live == null)
            live = Undo.AddComponent<ArcFloorUVLive>(go);

        live.Apply();

        EditorUtility.DisplayDialog(
            "UV по кругу",
            "Готово. Ширину/толщину скрипт больше не меняет.\n\n" +
            "Узкий пол: Transform Scale Y = 0.05, Radius как нужно.\n" +
            "Тайлинг: Tiles Across / Along в Arc Floor UV Live.",
            "OK");
    }
}
#endif
