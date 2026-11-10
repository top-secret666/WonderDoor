using UnityEditor;

/// <summary>Алиас меню → тот же Fix, что EnsureSinglePlayer.</summary>
public static class PlacePlayerInScene
{
    [MenuItem("WonderDoor/Place Player In Scene")]
    public static void Place() => EnsureSinglePlayer.FixMenu();
}
