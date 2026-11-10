using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Перед Play и после открытия сцены: ровно один Player в коридоре.
/// Не спавнит дубль во время domain reload (из‑за этого и плодились капсулы).
/// </summary>
[InitializeOnLoad]
public static class EnsureSinglePlayer
{
    const string PrefabPath = "Assets/_Project/Prefabs/Player.prefab";
    const string ScenePath = "Assets/_Project/Scenes/01_WonderDoor.unity";
    static readonly Vector3 SpawnPos = new Vector3(62.15f, 0.3f, -20.25f);
    static readonly Quaternion SpawnRot = Quaternion.Euler(0f, -90f, 0f);

    static bool _pending;

    static EnsureSinglePlayer()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.playModeStateChanged += OnPlayMode;
        // Не зовём Fix сразу на domain reload — сцена ещё пустая → ложный «нет Player» → дубль
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (!scene.path.EndsWith("01_WonderDoor.unity")) return;
        ScheduleFix();
    }

    static void OnPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            Fix(forceSpawn: true, showDialog: false);
    }

    static void ScheduleFix()
    {
        if (_pending) return;
        _pending = true;
        EditorApplication.delayCall += () =>
        {
            _pending = false;
            // ещё один тик — иерархия точно загружена
            EditorApplication.delayCall += () => Fix(forceSpawn: true, showDialog: false);
        };
    }

    [MenuItem("WonderDoor/Fix Player (one in corridor)")]
    public static void FixMenu() => Fix(forceSpawn: true, showDialog: true);

    static void Fix(bool forceSpawn, bool showDialog)
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return;
        if (!string.IsNullOrEmpty(scene.path) && !scene.path.Replace('\\', '/').EndsWith("01_WonderDoor.unity"))
        {
            if (showDialog)
                EditorUtility.DisplayDialog("Player", "Открой сцену 01_WonderDoor.", "OK");
            return;
        }

        var players = Object.FindObjectsByType<PlayerController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Удаляем ВСЕ лишние, оставляем ближайшего к спавну
        if (players.Length > 1)
        {
            PlayerController keep = null;
            float best = float.MaxValue;
            foreach (var p in players)
            {
                if (p == null) continue;
                float d = Vector3.Distance(p.transform.position, SpawnPos);
                // Предпочитаем не-ноль (дубли часто в 0,0,0)
                Vector3 pos = p.transform.position;
                bool nearOrigin = pos.sqrMagnitude < 1f;
                float score = nearOrigin ? d + 1000f : d;
                if (score < best)
                {
                    best = score;
                    keep = p;
                }
            }

            int removed = 0;
            foreach (var p in players)
            {
                if (p == null || p == keep) continue;
                Undo.DestroyObjectImmediate(p.gameObject);
                removed++;
            }

            if (keep != null)
            {
                // Если единственный остался в нуле — вернём в коридор
                if (keep.transform.position.sqrMagnitude < 4f)
                    keep.transform.SetPositionAndRotation(SpawnPos, SpawnRot);
                EnsureCamera(keep);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[EnsureSinglePlayer] Удалено дублей: {removed}. Остался @ {(keep ? keep.transform.position.ToString() : "?")}");
            if (showDialog)
                EditorUtility.DisplayDialog("Player", $"Лишние удалены ({removed}). Один Player в коридоре.", "OK");
            return;
        }

        if (players.Length == 1)
        {
            var p = players[0];
            // Сдвинут в ноль / под пол — вернуть на спавн
            if (p.transform.position.sqrMagnitude < 4f || p.transform.position.y < -5f)
            {
                Undo.RecordObject(p.transform, "Reset Player Spawn");
                p.transform.SetPositionAndRotation(SpawnPos, SpawnRot);
                EditorSceneManager.MarkSceneDirty(scene);
            }
            EnsureCamera(p);
            if (showDialog)
                EditorUtility.DisplayDialog("Player", "Ок: один Player на месте.", "OK");
            return;
        }

        // players.Length == 0
        if (!forceSpawn) return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            if (showDialog)
                EditorUtility.DisplayDialog("Player", $"Нет префаба:\n{PrefabPath}", "OK");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        Undo.RegisterCreatedObjectUndo(instance, "Spawn Player");
        instance.name = "Player";
        instance.transform.SetPositionAndRotation(SpawnPos, SpawnRot);
        var pc = instance.GetComponent<PlayerController>();
        if (pc) EnsureCamera(pc);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[EnsureSinglePlayer] Player создан в коридоре.");
        if (showDialog)
            EditorUtility.DisplayDialog("Player", "Player отсутствовал — поставлен в коридор (62, 0.3, -20).", "OK");
    }

    static void EnsureCamera(PlayerController pc)
    {
        if (pc == null) return;
        var cam = pc.GetComponentInChildren<Camera>(true);
        if (cam == null) return;
        cam.enabled = true;
        cam.gameObject.tag = "MainCamera";
        if (pc.cameraPivot == null)
            pc.cameraPivot = cam.transform;
        var listener = cam.GetComponent<AudioListener>();
        if (listener) listener.enabled = true;

        // Выключить чужие камеры/слушатели
        foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c == cam) continue;
            if (c.CompareTag("MainCamera") || c.GetComponentInParent<PlayerController>() != null)
            {
                c.enabled = false;
                var al = c.GetComponent<AudioListener>();
                if (al) al.enabled = false;
            }
        }
    }
}
