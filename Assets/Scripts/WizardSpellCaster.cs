
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardSpellCaster : MonoBehaviour
{
    [Header("Quest 3 Input")]
    public InputActionReference castAction;

    [Header("Spell Settings")]
    public float spellSpeed = 12f;
    public float spellLifetime = 5f;
    public float spellSize = 0.12f;

    [Header("Spell Appearance")]
    public Color[] spellColors =
    {
        Color.cyan,
        new Color(0.65f, 0.25f, 1f),
        Color.green,
        Color.red
    };

    [Header("Spell Trail")]
    [Tooltip("Material for the comet trail (an unlit, transparent material).")]
    public Material trailMaterial;
    public float trailTime = 0.35f;

    [Tooltip("Light carried by the spell, lighting up what it flies past.")]
    public float spellLightRange = 2.5f;
    public float spellLightIntensity = 2f;

    [Header("Ready Indicator")]
    [Tooltip("A small orb in front of the hand showing the selected color. " +
             "It grows back while the spell recharges.")]
    public float indicatorSize = 0.04f;
    public float indicatorDistance = 0.06f;

    [Header("Spell Cooldown")]
    public float spellCooldown = 2f;

    private float nextCastTime = 0f;

    private int currentColorIndex = 0;

    private Transform readyIndicator;
    private Renderer readyIndicatorRenderer;

    public int CurrentColorIndex => currentColorIndex;

    // Time of the last spell cast (negative if none yet).
    public float LastCastTime { get; private set; } = -1f;

    public Color CurrentColor =>
        spellColors != null && spellColors.Length > 0
            ? spellColors[currentColorIndex % spellColors.Length]
            : Color.cyan;

    // 0 right after casting, 1 when the next spell is ready.
    public float CooldownProgress =>
        spellCooldown <= 0f
            ? 1f
            : Mathf.Clamp01(1f - (nextCastTime - Time.time) / spellCooldown);

    void Awake()
    {
        CreateReadyIndicator();
    }

    void OnEnable()
    {
        if (readyIndicator != null)
            readyIndicator.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        // e.g. while the menu is open.
        if (readyIndicator != null)
            readyIndicator.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (readyIndicator != null)
            Destroy(readyIndicator.gameObject);
    }

    void Update()
    {
        UpdateReadyIndicator();

        if (castAction == null || castAction.action == null)
            return;

        if (castAction.action.WasPressedThisFrame())
        {
            CastSpell();
        }
    }

    // Cycles through the available spell colors.
    public void ChangeSpellColor()
    {
        if (spellColors == null || spellColors.Length == 0)
            return;

        currentColorIndex =
            (currentColorIndex + 1) % spellColors.Length;
    }

    // Selects a spell color directly (used by the grimoire).
    public void SetSpellColor(int index)
    {
        if (spellColors == null || index < 0 || index >= spellColors.Length)
            return;

        currentColorIndex = index;
    }

    // Creates a projectile in the direction of the controller.
    void CastSpell()
    {
        if (Time.time < nextCastTime)
            return;

        nextCastTime = Time.time + spellCooldown;
        LastCastTime = Time.time;

        Vector3 direction = transform.forward;

        Vector3 spawnPosition =
            transform.position + direction * 0.25f;

        GameObject spell = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        spell.name = "Magic Orb";
        spell.transform.position = spawnPosition;
        spell.transform.localScale =
            Vector3.one * spellSize;

        // Apply the currently selected spell color.
        Color color = CurrentColor;
        Renderer renderer = spell.GetComponent<Renderer>();
        renderer.material.color = color;

        AddTrail(spell, color);
        AddLight(spell, color);

        // Handle projectile collisions and impact effects.
        spell.AddComponent<MagicProjectile>();

        Rigidbody rb = spell.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        rb.linearVelocity = direction * spellSpeed;

        // Remove projectiles that do not collide with anything.
        Destroy(spell, spellLifetime);
    }

    // A comet tail: as wide as the orb at the head, thin at the end.
    void AddTrail(GameObject spell, Color color)
    {
        if (trailMaterial == null)
            return;

        TrailRenderer trail = spell.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.minVertexDistance = 0.05f;
        trail.widthMultiplier = spellSize;
        trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;

        // Own copy of the material, tinted with the spell color, so the
        // shared asset is not modified.
        Material material = new Material(trailMaterial);
        Color trailColor = color;
        trailColor.a = 0.6f;
        material.SetColor("_BaseColor", trailColor);
        trail.sharedMaterial = material;

        // The copy goes away with the spell.
        Destroy(material, spellLifetime);
        trail.startColor = color;
        trail.endColor = new Color(color.r, color.g, color.b, 0f);
    }

    void AddLight(GameObject spell, Color color)
    {
        Light light = spell.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = spellLightRange;
        light.intensity = spellLightIntensity;
        light.shadows = LightShadows.None;
    }

    void CreateReadyIndicator()
    {
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "Spell Ready Indicator";

        // Visual only.
        Destroy(orb.GetComponent<Collider>());

        readyIndicatorRenderer = orb.GetComponent<Renderer>();
        readyIndicatorRenderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        // Not parented to the hand: the hand is scaled, so the orb is
        // placed in world space every frame instead.
        readyIndicator = orb.transform;
    }

    void UpdateReadyIndicator()
    {
        if (readyIndicator == null)
            return;

        readyIndicator.position =
            transform.position + transform.forward * indicatorDistance;

        // Grows back while the spell recharges.
        readyIndicator.localScale =
            Vector3.one * (indicatorSize * CooldownProgress);

        readyIndicatorRenderer.material.color = CurrentColor;
    }
}
