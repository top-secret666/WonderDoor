using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Атмосфера: туман-дымка, пост-обработка (bloom, тонмаппинг, виньетка) и пылинки в воздухе вокруг камеры.
public class Atmosphere : MonoBehaviour
{
    public Color fogColor = new Color(0.62f, 0.66f, 0.74f);
    public float fogDensity = 0.035f;
    public Color ambient = new Color(0.28f, 0.29f, 0.33f);
    public float bloomIntensity = 0.9f, vignette = 0.32f;
    public int dustCount = 90;

    void Start()
    {
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor; RenderSettings.fogDensity = fogDensity;
        RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = ambient;

        var cam = GetComponentInChildren<Camera>();
        if (cam) { var d = cam.GetUniversalAdditionalCameraData(); d.renderPostProcessing = true; d.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing; cam.backgroundColor = fogColor; cam.clearFlags = CameraClearFlags.SolidColor; }

        var vgo = new GameObject("~GlobalVolume"); var vol = vgo.AddComponent<Volume>(); vol.isGlobal = true; vol.priority = 10;
        var p = ScriptableObject.CreateInstance<VolumeProfile>(); vol.profile = p;
        var bl = p.Add<Bloom>(true); bl.intensity.Override(bloomIntensity); bl.threshold.Override(0.9f); bl.scatter.Override(0.75f);
        var tm = p.Add<Tonemapping>(true); tm.mode.Override(TonemappingMode.ACES);
        var vg = p.Add<Vignette>(true); vg.intensity.Override(vignette); vg.smoothness.Override(0.45f);
        var ca = p.Add<ColorAdjustments>(true); ca.postExposure.Override(0.25f); ca.contrast.Override(12f); ca.saturation.Override(-8f);
        var fg = p.Add<FilmGrain>(true); fg.intensity.Override(0.15f);

        if (cam) MakeDust(cam.transform);
    }

    void MakeDust(Transform parent)
    {
        var go = new GameObject("~Dust"); go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>(); ps.Stop();
        var main = ps.main; main.loop = true; main.startLifetime = 9f; main.startSpeed = 0.03f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.025f); main.maxParticles = dustCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World; main.startColor = new Color(1f, 0.97f, 0.9f, 0.35f);
        var em = ps.emission; em.rateOverTime = dustCount / 9f;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(10f, 6f, 10f);
        var no = ps.noise; no.enabled = true; no.strength = 0.08f; no.frequency = 0.25f;
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.3f), new GradientAlphaKey(1, 0.7f), new GradientAlphaKey(0, 1) });
        col.color = g;
        var r = go.GetComponent<ParticleSystemRenderer>();
        var sh2 = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh2) { var m = new Material(sh2); m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 2); m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha); m.SetInt("_DstBlend", (int)BlendMode.One); m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = 3000; r.material = m; }
        ps.Play();
    }
}

