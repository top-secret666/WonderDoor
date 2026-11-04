using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Снимает блокировку клика/перемещения в Scene (иконка «рука» в Hierarchy).
/// Меню: WonderDoor → Unlock All Scene Picking
/// </summary>
public static class UnlockScenePicking
{
    [MenuItem("WonderDoor/Unlock All Scene Picking")]
    public static void Unlock()
    {
        SceneVisibilityManager.instance.EnableAllPicking();
        SceneVisibilityManager.instance.ShowAll();
        Debug.Log("[UnlockScenePicking] Picking и Visibility включены для всех объектов сцены.");
        EditorUtility.DisplayDialog(
            "Unlock Scene Picking",
            "Готово.\nТеперь Floor / Spline снова можно кликать в Scene и двигать.\n\nЕсли нет — кликни объект в Hierarchy, потом Move (W).",
            "OK");
    }
}
