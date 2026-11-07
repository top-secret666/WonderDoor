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

        // light layer 1: only this room's renderers receive the sun (URP Rendering Layers)
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.renderingLayerMask |= 2u;
        var go = new GameObject("~Sun"); go.transform.SetParent(transform, false);
        go.transform.rotation = rot;
        var l = go.AddComponent<Light>(); sun = l; area = b; area.Expand(new Vector3(1f, 4f, 1f));
        l.type = LightType.Directional; l.color = sunColor; l.intensity = intensity;
        l.renderingLayerMask = 2; l.shadows = LightShadows.Soft; l.shadowStrength = 0.95f;
        var d = go.AddComponent<UniversalAdditionalLightData>(); d.renderingLayers = 2u;
    }

    // generated meshes (meadow grass) can appear later, so keep tagging them for the sun
    float nextTag;
    void Update()
    {
        if (Time.time < nextTag) return; nextTag = Time.time + 0.5f;
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.renderingLayerMask |= 2u;
    }
}
