using System;
using UnityEngine;
using UnityEngine.Rendering;

// Switches the graveyard between day and a foggy, moonlit night.
// The day look is whatever the scene has when it starts.
public class NightAtmosphere : MonoBehaviour
{
    [Tooltip("The scene's directional light (sun / moon).")]
    public Light sun;

    [Header("Night")]
    public float moonIntensity = 0.35f;
    public Color moonColor = new Color(0.55f, 0.65f, 1f);
    public Color nightAmbient = new Color(0.07f, 0.08f, 0.14f);
    public Color nightFogColor = new Color(0.04f, 0.05f, 0.09f);
    public float nightFogDensity = 0.06f;

    [Tooltip("Exposure of the night sky (procedural skybox only).")]
    public float nightSkyExposure = 0.05f;

    public event Action Changed;

    public bool IsNight { get; private set; }

    // Day values, captured at startup.
    private float daySunIntensity;
    private Color daySunColor;
    private AmbientMode dayAmbientMode;
    private Color dayAmbientLight;
    private bool dayFog;
    private FogMode dayFogMode;
    private Color dayFogColor;
    private float dayFogDensity;
    private Material daySkybox;

    private Material nightSkybox;

    void Awake()
    {
        if (sun != null)
        {
            daySunIntensity = sun.intensity;
            daySunColor = sun.color;
        }

        dayAmbientMode = RenderSettings.ambientMode;
        dayAmbientLight = RenderSettings.ambientLight;
        dayFog = RenderSettings.fog;
        dayFogMode = RenderSettings.fogMode;
        dayFogColor = RenderSettings.fogColor;
        dayFogDensity = RenderSettings.fogDensity;
        daySkybox = RenderSettings.skybox;

        // A darker copy of the sky, so the shared asset is not modified.
        if (daySkybox != null && daySkybox.HasProperty("_Exposure"))
        {
            nightSkybox = new Material(daySkybox);
            nightSkybox.SetFloat("_Exposure", nightSkyExposure);
        }
    }

    void OnDestroy()
    {
        // Leave the scene settings as they were.
        Apply(false);

        if (nightSkybox != null)
            Destroy(nightSkybox);
    }

    public void SetNight(bool night)
    {
        Apply(night);
        Changed?.Invoke();
    }

    void Apply(bool night)
    {
        IsNight = night;

        if (sun != null)
        {
            sun.intensity = night ? moonIntensity : daySunIntensity;
            sun.color = night ? moonColor : daySunColor;
        }

        // Flat ambient at night: the skybox ambient would stay bright.
        RenderSettings.ambientMode = night ? AmbientMode.Flat : dayAmbientMode;
        RenderSettings.ambientLight = night ? nightAmbient : dayAmbientLight;

        RenderSettings.fog = night || dayFog;
        RenderSettings.fogMode = night ? FogMode.ExponentialSquared : dayFogMode;
        RenderSettings.fogColor = night ? nightFogColor : dayFogColor;
        RenderSettings.fogDensity = night ? nightFogDensity : dayFogDensity;

        if (nightSkybox != null)
            RenderSettings.skybox = night ? nightSkybox : daySkybox;

        DynamicGI.UpdateEnvironment();
    }
}
