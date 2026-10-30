
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

// Spell type, unlocked by delivering pumpkins
[System.Serializable]
public class SpellType
{
    public string name = "Rayo";
    public Color color = Color.cyan;
    [Tooltip("Damage per hit. Goblins have 6 health.")]
    public float damage = 1f;
    public float speedMultiplier = 1f;
    public float sizeMultiplier = 1f;
    [Tooltip("If > 0, explodes and damages every goblin in this radius (m).")]
    public float splashRadius = 0f;
    [Tooltip("Pumpkins to deliver in the round to unlock it (0 = from the start).")]
    public int deliveriesToUnlock = 0;
    [Tooltip("Short description for the grimoire.")]
    public string description = "Daño 1";
}

public class WizardSpellCaster : MonoBehaviour
{
    [Header("Quest 3 Input")]
    public InputActionReference castAction;

    [Header("Spell Settings")]
    public float spellSpeed = 12f;
    public float spellLifetime = 5f;
    public float spellSize = 0.12f;

    [Header("Spells (unlocked by delivering pumpkins)")]
    public SpellType[] spells =
    {
        new SpellType
        {
            name = "Rayo", color = Color.cyan,
            damage = 1f, deliveriesToUnlock = 0,
            description = "daño 1",
        },
        new SpellType
        {
            name = "Rayo arcano", color = new Color(0.65f, 0.25f, 1f),
            damage = 1f, speedMultiplier = 1.6f, deliveriesToUnlock = 1,
            description = "daño 1 · rápido",
        },
        new SpellType
        {
            name = "Explosivo", color = new Color(0.45f, 1f, 0.2f),
            damage = 1.5f, splashRadius = 1.5f, deliveriesToUnlock = 3,
            description = "daño 1,5 · área",
        },
        new SpellType
        {
            name = "Bola de fuego", color = new Color(1f, 0.4f, 0.1f),
            damage = 2f, sizeMultiplier = 1.8f, deliveriesToUnlock = 5,
            description = "daño x2",
        },
    };

    [Header("Spell Trail")]
    [Tooltip("Material for the comet trail (an unlit, transparent material).")]
    public Material trailMaterial;
    public float trailTime = 0.35f;

    [Tooltip("Light carried by the spell, lighting up what it flies past.")]
    public float spellLightRange = 2.5f;
    public float spellLightIntensity = 2f;

    [Header("Ready Indicator")]
    [Tooltip("Orb on the hand, grows back while recharging")]
    public float indicatorSize = 0.04f;
    public float indicatorDistance = 0.06f;

    [Header("Spell Cooldown")]
    public float spellCooldown = 2f;

    [Header("Feedback")]
    [Tooltip("Haptics of the casting controller.")]
    public HapticImpulsePlayer haptics;

    private float nextCastTime = 0f;

    private int currentColorIndex = 0;
    private int lastKnownDeliveries;

    private Transform readyIndicator;
    private Renderer readyIndicatorRenderer;

    public int CurrentColorIndex => currentColorIndex;

    // Time of the last spell cast (negative if none yet).
    public float LastCastTime { get; private set; } = -1f;

    public SpellType CurrentSpell =>
        spells != null && spells.Length > 0
            ? spells[currentColorIndex % spells.Length]
            : null;

    public Color CurrentColor =>
        CurrentSpell != null ? CurrentSpell.color : Color.cyan;

    // Pumpkins delivered this round (all spells unlocked without a game).
    public int Deliveries =>
        GraveyardGameManager.Instance != null
            ? GraveyardGameManager.Instance.PumpkinsDelivered
            : int.MaxValue;

    public bool IsUnlocked(int index) =>
        spells != null && index >= 0 && index < spells.Length &&
        Deliveries >= spells[index].deliveriesToUnlock;

    // 0 right after casting, 1 when the next spell is ready.
    public float CooldownProgress =>
        spellCooldown <= 0f
            ? 1f
            : Mathf.Clamp01(1f - (nextCastTime - Time.time) / spellCooldown);

    void Awake()
    {
        CreateReadyIndicator();
    }

    void Start()
    {
        lastKnownDeliveries = Deliveries;
    }

    void OnEnable()
    {
        if (readyIndicator != null)
            readyIndicator.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        // e.g. menu open
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
        CheckUnlocks();

        if (castAction == null || castAction.action == null)
            return;

        if (castAction.action.WasPressedThisFrame())
        {
            CastSpell();
        }
    }

    // Cycles through the unlocked spells.
    public void ChangeSpellColor()
    {
        if (spells == null || spells.Length == 0)
            return;

        for (int step = 1; step <= spells.Length; step++)
        {
            int next = (currentColorIndex + step) % spells.Length;
            if (IsUnlocked(next))
            {
                currentColorIndex = next;
                return;
            }
        }
    }

    // used by the grimoire
    public void SetSpellColor(int index)
    {
        if (!IsUnlocked(index))
            return;

        currentColorIndex = index;
    }

    // spell unlocked: message, sound, haptic
    void CheckUnlocks()
    {
        int deliveries = Deliveries;
        if (deliveries == lastKnownDeliveries || spells == null)
            return;

        for (int i = 0; i < spells.Length; i++)
        {
            int needed = spells[i].deliveriesToUnlock;
            if (needed > lastKnownDeliveries && needed <= deliveries)
            {
                // equip it
                currentColorIndex = i;
                AnnounceUnlock(spells[i]);
            }
        }

        lastKnownDeliveries = deliveries;
    }

    void AnnounceUnlock(SpellType spell)
    {
        Transform head = Camera.main != null ? Camera.main.transform : transform;

        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f)
            forward = head.forward;
        forward.Normalize();

        FloatingText message = FloatingText.Spawn(
            head.position + forward * 1.5f + Vector3.up * 0.15f,
            $"¡Nuevo hechizo desbloqueado!\n{spell.name}",
            spell.color
        );
        message.duration = 3.5f;
        message.riseDistance = 0.15f;

        MagicFeedback.Play(MagicFeedback.ChimeSound, head.position, 1f);
        MagicFeedback.Pulse(haptics, 0.6f, 0.25f);
    }

    // Creates a projectile in the direction of the controller.
    void CastSpell()
    {
        if (Time.time < nextCastTime)
        {
            // Not ready yet: a light tick, so the press isn't ignored silently.
            MagicFeedback.Pulse(haptics, 0.08f, 0.03f);
            return;
        }

        nextCastTime = Time.time + spellCooldown;
        LastCastTime = Time.time;

        Vector3 direction = transform.forward;

        Vector3 spawnPosition =
            transform.position + direction * 0.25f;

        // spell orb (keeps the collider)
        SpellType type = CurrentSpell ?? new SpellType();
        Color color = type.color;
        float size = spellSize * type.sizeMultiplier;

        GameObject spell = GlowSphere.Create(
            "Magic Orb", spawnPosition, size, color, keepCollider: true
        );

        AddTrail(spell, color, size);
        AddLight(spell, color);

        // Handle projectile collisions, damage and impact effects.
        MagicProjectile projectile = spell.AddComponent<MagicProjectile>();
        projectile.color = color;
        projectile.damage = type.damage;
        projectile.splashRadius = type.splashRadius;

        Rigidbody rb = spell.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        rb.linearVelocity = direction * spellSpeed * type.speedMultiplier;

        MagicFeedback.Play(MagicFeedback.CastSound, spawnPosition, 0.6f);
        MagicFeedback.Pulse(haptics, 0.3f, 0.08f);

        // Remove projectiles that do not collide with anything.
        Destroy(spell, spellLifetime);
    }

    // A comet tail: as wide as the orb at the head, thin at the end.
    void AddTrail(GameObject spell, Color color, float size)
    {
        if (trailMaterial == null)
            return;

        TrailRenderer trail = spell.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.minVertexDistance = 0.05f;
        trail.widthMultiplier = size;
        trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;

        // copy the material so the asset isn't changed
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
        GameObject orb = GlowSphere.Create(
            "Spell Ready Indicator", transform.position, indicatorSize, CurrentColor
        );

        readyIndicatorRenderer = orb.GetComponent<Renderer>();

        // not parented, the hand is scaled
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

        GlowSphere.SetColor(readyIndicatorRenderer, CurrentColor);
    }
}
