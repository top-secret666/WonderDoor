using UnityEngine;
using UnityEngine.Rendering.Universal;

// Солнечный свет из окон: направленный свет поперёк комнаты, под углом сверху, с мягкими тенями.
public class WindowSunlight : MonoBehaviour
{
    public Color sunColor = new Color(1f, 0.88f, 0.68f);
    public float intensity = 3f;
    [Range(5, 85)] public float elevation = 35f;
    public float yawOffset = 20f;
    public bool fromOtherSide = false;
    public int sunLayer = 8;

    Light sun; Bounds area;

    void Start()
    {
        var rs = GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return;
        Bounds b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        Vector3 across = b.size.x >= b.size.z ? Vector3.forward : Vector3.right; // поперёк длинной оси
        if (fromOtherSide) across = -across;
        Quaternion rot = Quaternion.LookRotation(across) * Quaternion.Euler(elevation, yawOffset, 0f);

        foreach (var t in GetComponentsInChildren<Transform>(true)) t.gameObject.layer = sunLayer;
        var go = new GameObject("~Sun"); go.transform.SetParent(transform, false);
        go.transform.rotation = rot;
        var l = go.AddComponent<Light>(); sun = l; area = b; area.Expand(new Vector3(1f, 4f, 1f));
        l.type = LightType.Directional; l.color = sunColor; l.intensity = intensity;
        l.cullingMask = 1 << sunLayer; l.shadows = LightShadows.Soft; l.shadowStrength = 0.95f;
        var d = go.AddComponent<UniversalAdditionalLightData>();
    }

    // солнце включено только когда камера внутри этой комнаты
    void Update()
    {
        if (!sun) return;
        var cam = Camera.main;
        sun.enabled = cam && area.Contains(cam.transform.position);
    }
}
