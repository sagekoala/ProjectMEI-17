
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Anti-ghost lantern: held like a flashlight, Y turns the light on
public class GhostLantern : MonoBehaviour
{
    const float MinBeamAngle = 6f;
    const float MaxBeamAngle = 35f;
    const int MistPuffs = 8;

    static readonly Color BeamColor = new Color(0.5f, 1f, 0.6f, 1f);
    static readonly Color LensOn = new Color(0.6f, 1f, 0.65f);
    static readonly Color LensReady = new Color(0.25f, 0.55f, 0.3f);
    static readonly Color LensOff = new Color(0.15f, 0.25f, 0.17f);

    // cone layers: relative radius, alpha
    static readonly Vector2[] ConeLayers =
    {
        new Vector2(1f, 0.035f),
        new Vector2(0.6f, 0.045f),
        new Vector2(0.3f, 0.06f),
        new Vector2(0.1f, 0.09f),
    };

    private GhostSystem system;
    private Rigidbody body;
    private WizardLevitation levitation;
    private Vector3 baseScale;
    private Vector3 home;

    private Transform beamRoot;
    private Transform[] cones;
    private Transform[] puffs;
    private Renderer[] puffRenderers;
    private Light beamLight;
    private Renderer lensRenderer;
    private Transform flare;
    private Transform hitSpot;
    private Renderer hitSpotRenderer;
    private TextMeshPro label;
    private float nextHitPulse;
    private InputAction shineAction;

    const string GroundText =
        "<b>LINTERNA ANTI-FANTASMAS</b>\n<size=65%>Cógela con telequinesis</size>";
    const string HeldText = "Mantén <b>Y</b> para encender la luz";


    public Rigidbody Body => body;
    public bool IsHeld { get; private set; }
    public bool IsShining { get; private set; }

    public static GhostLantern Create(Vector3 position, GhostSystem system)
    {
        GameObject root = new GameObject("Spectral Lantern");
        root.transform.position = position;
        root.tag = "Levitatable";

        GhostLantern lantern = root.AddComponent<GhostLantern>();
        lantern.system = system;
        lantern.home = position;
        lantern.Build();
        return lantern;
    }

    void Build()
    {
        // Body: a dark cylinder along +Z (the beam direction).
        GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        casing.name = "Casing";
        Destroy(casing.GetComponent<Collider>());
        casing.transform.SetParent(transform, false);
        casing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        casing.transform.localScale = new Vector3(0.12f, 0.13f, 0.12f);
        Renderer casingRenderer = casing.GetComponent<Renderer>();
        if (GlowSphere.Material != null)
            casingRenderer.sharedMaterial = GlowSphere.Material;
        GlowSphere.SetColor(casingRenderer, new Color(0.16f, 0.12f, 0.2f));

        // Glowing lens at the front.
        GameObject lens = GlowSphere.Create("Lens", transform.position, 0.12f, LensOff);
        lens.transform.SetParent(transform, false);
        lens.transform.localPosition = new Vector3(0f, 0f, 0.13f);
        lens.transform.localScale = new Vector3(0.13f, 0.13f, 0.06f);
        lensRenderer = lens.GetComponent<Renderer>();

        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(0.14f, 0.14f, 0.3f);

        body = gameObject.AddComponent<Rigidbody>();
        body.mass = 1f;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;

        baseScale = transform.localScale;

        BuildBeam();
    }

    // beam is not a child so the lantern scale doesn't affect it
    void BuildBeam()
    {
        GameObject root = new GameObject("Lantern Beam");
        beamRoot = root.transform;

        Material translucent = Resources.Load<Material>("GlowTransparent");
        Mesh cone = BuildConeMesh(24);

        cones = new Transform[ConeLayers.Length];
        for (int i = 0; i < ConeLayers.Length; i++)
        {
            GameObject layer = new GameObject("Beam Layer");
            layer.transform.SetParent(beamRoot, false);
            layer.AddComponent<MeshFilter>().sharedMesh = cone;

            MeshRenderer renderer = layer.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            if (translucent != null)
                renderer.sharedMaterial = translucent;

            Color color = BeamColor;
            color.a = ConeLayers[i].y;
            GlowSphere.SetColor(renderer, color);

            cones[i] = layer.transform;
        }

        // Mist: soft puffs drifting out along the beam.
        puffs = new Transform[MistPuffs];
        puffRenderers = new Renderer[MistPuffs];
        for (int i = 0; i < MistPuffs; i++)
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = "Mist";
            Destroy(puff.GetComponent<Collider>());
            puff.transform.SetParent(beamRoot, false);

            Renderer renderer = puff.GetComponent<Renderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            if (translucent != null)
                renderer.sharedMaterial = translucent;

            puffs[i] = puff.transform;
            puffRenderers[i] = renderer;
        }

        // A bright glow at the mouth of the lantern.
        GameObject flareObject = GlowSphere.Create("Flare", beamRoot.position, 0.1f,
            new Color(0.8f, 1f, 0.8f));
        flareObject.transform.SetParent(beamRoot, false);
        flare = flareObject.transform;

        // Where the beam hits something: a glowing green splash.
        GameObject spot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spot.name = "Beam Hit";
        Destroy(spot.GetComponent<Collider>());
        spot.transform.SetParent(beamRoot, false);
        hitSpotRenderer = spot.GetComponent<Renderer>();
        hitSpotRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        hitSpotRenderer.receiveShadows = false;
        if (translucent != null)
            hitSpotRenderer.sharedMaterial = translucent;
        Color spotColor = BeamColor;
        spotColor.a = 0.12f;
        GlowSphere.SetColor(hitSpotRenderer, spotColor);
        hitSpot = spot.transform;

        GameObject lightObject = new GameObject("Lantern Light");
        lightObject.transform.SetParent(beamRoot, false);
        beamLight = lightObject.AddComponent<Light>();
        beamLight.type = LightType.Spot;
        beamLight.color = new Color(0.4f, 1f, 0.45f);
        beamLight.intensity = 10f;
        beamLight.shadows = LightShadows.None;
        // ForcePixel so URP doesn't drop it (4 lights per object limit)
        beamLight.renderMode = LightRenderMode.ForcePixel;
        beamLight.innerSpotAngle = 1f;

        root.SetActive(false);

        BuildLabel();
    }

    // label: name on the ground, Y hint in the hand
    void BuildLabel()
    {
        GameObject labelObject = new GameObject("Lantern Label");
        labelObject.transform.localScale = Vector3.one * 0.06f;

        label = labelObject.AddComponent<TextMeshPro>();
        label.text = HeldText;
        label.fontSize = 12f;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.color = new Color(0.65f, 1f, 0.7f);
        label.rectTransform.sizeDelta = new Vector2(30f, 4f);

        labelObject.SetActive(false);
    }

    // Apex at the origin, open base of radius 1 at z = 1.
    static Mesh BuildConeMesh(int segments)
    {
        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 1f);

            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        Mesh mesh = new Mesh { name = "Lantern Beam Cone" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    void Awake()
    {
        // own Y binding (+ keyboard Y for testing)
        shineAction = new InputAction("Lantern Shine", InputActionType.Button);
        shineAction.AddBinding("<XRController>{LeftHand}/{SecondaryButton}");
        shineAction.AddBinding("<XRController>{LeftHand}/secondaryButton");
        shineAction.AddBinding("<Keyboard>/y");
    }

    void OnEnable()
    {
        shineAction.Enable();
    }

    void OnDisable()
    {
        shineAction.Disable();
    }

    void Start()
    {
        levitation = FindAnyObjectByType<WizardLevitation>();
        if (levitation == null)
            Debug.LogWarning("GhostLantern: no WizardLevitation found.", this);
    }

    void OnDestroy()
    {
        shineAction?.Dispose();

        if (beamRoot != null)
            Destroy(beamRoot.gameObject);
        if (label != null)
            Destroy(label.gameObject);
    }

    // 1 = normal size.
    public float SizeMultiplier =>
        baseScale.x > 0f ? transform.localScale.x / baseScale.x : 1f;

    public float BeamRange => system.baseBeamRange;

    // Only the width depends on the size.
    public float BeamAngle => Mathf.Clamp(
        system.baseBeamAngle * SizeMultiplier, MinBeamAngle, MaxBeamAngle);

    // Y, through the lantern's own action or the telekinesis Y action.
    bool ShineButtonPressed =>
        shineAction.IsPressed() ||
        (levitation != null &&
         levitation.rotateAction != null &&
         levitation.rotateAction.action != null &&
         levitation.rotateAction.action.IsPressed());

    void LateUpdate()
    {
        if (levitation == null)
            levitation = FindAnyObjectByType<WizardLevitation>();

        IsHeld = levitation != null && levitation.HeldObject == body;
        bool pressed = ShineButtonPressed;
        bool shining = IsHeld && pressed;

        IsShining = shining;

        UpdateLabel();

        beamRoot.gameObject.SetActive(IsShining);
        GlowSphere.SetColor(lensRenderer,
            IsShining ? LensOn : IsHeld ? LensReady : LensOff);

        if (!IsHeld)
        {
            ReturnHomeIfLost();
            return;
        }

        if (!IsShining)
            return;

        float range = BeamRange;
        float angle = BeamAngle;
        float radius = range * Mathf.Tan(angle * Mathf.Deg2Rad);

        Vector3 origin = transform.position + transform.forward * 0.15f * SizeMultiplier;
        beamRoot.SetPositionAndRotation(origin, transform.rotation);

        for (int i = 0; i < cones.Length; i++)
        {
            float r = radius * ConeLayers[i].x;
            cones[i].localScale = new Vector3(r, r, range);
        }

        UpdateMist(range, radius);
        UpdateHitSpot(origin, range, radius);
        flare.localScale = Vector3.one * (0.06f + 0.015f * Mathf.Sin(Time.time * 12f)) * SizeMultiplier;

        beamLight.range = range * 1.6f;
        beamLight.spotAngle = Mathf.Min(angle * 2.4f, 120f);
        beamLight.innerSpotAngle = angle * 1.2f;

        LightGhost(origin, range, angle);
    }

    // mist puffs moving along the beam
    void UpdateMist(float range, float radius)
    {
        for (int i = 0; i < puffs.Length; i++)
        {
            float t = Mathf.Repeat(Time.time * 0.45f + (float)i / puffs.Length, 1f);
            float wobble = Mathf.Sin(Time.time * 1.7f + i * 2.1f);

            puffs[i].localPosition = new Vector3(
                wobble * radius * 0.25f * t,
                Mathf.Cos(Time.time * 1.3f + i) * radius * 0.2f * t,
                range * t);

            float size = Mathf.Max(0.05f, radius * t * 1.3f);
            puffs[i].localScale = new Vector3(size, size, size * 1.6f);

            Color color = BeamColor;
            color.a = 0.07f * Mathf.Sin(Mathf.PI * t);
            GlowSphere.SetColor(puffRenderers[i], color);
        }
    }

    // light spot where the beam hits
    void UpdateHitSpot(Vector3 origin, float range, float radius)
    {
        float distance = range;
        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, range,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            distance = hit.distance;

        float size = Mathf.Max(0.12f, radius * (distance / range) * 1.6f);
        hitSpot.localPosition = new Vector3(0f, 0f, distance);
        hitSpot.localScale = new Vector3(size, size, size * 0.35f);
    }

    void UpdateLabel()
    {
        bool show = !IsShining;
        label.gameObject.SetActive(show);
        if (!show)
            return;

        if (IsHeld)
        {
            label.text = HeldText;
            label.transform.localScale = Vector3.one * 0.06f;
            label.transform.position =
                transform.position + Vector3.up * (0.16f + 0.05f * SizeMultiplier);
        }
        else
        {
            // Bigger and floating gently, readable from a few meters away.
            label.text = GroundText;
            label.transform.localScale = Vector3.one * 0.1f;
            label.transform.position = transform.position +
                Vector3.up * (0.45f + 0.04f * Mathf.Sin(Time.time * 2f));
        }

        if (Camera.main != null)
        {
            Vector3 away = label.transform.position - Camera.main.transform.position;
            if (away.sqrMagnitude > 0.0001f)
                label.transform.rotation = Quaternion.LookRotation(away, Vector3.up);
        }
    }

    void LightGhost(Vector3 origin, float range, float angle)
    {
        GhostEnemy ghost = system.Ghost;
        if (ghost == null)
            return;

        Vector3 toGhost = ghost.transform.position - origin;
        float distance = toGhost.magnitude;
        if (distance > range + ghost.Radius || distance < 0.01f)
            return;

        // The ghost's size counts, so grazing it with the edge works too.
        float allowance = Mathf.Atan2(ghost.Radius, distance) * Mathf.Rad2Deg;
        if (Vector3.Angle(transform.forward, toGhost) <= angle + allowance)
        {
            ghost.Expose(Time.deltaTime);

            // A soft buzz in the hand while the light is on the ghost.
            if (Time.time >= nextHitPulse && GraveyardFeedback.Instance != null)
            {
                nextHitPulse = Time.time + 0.15f;
                MagicFeedback.Pulse(GraveyardFeedback.Instance.leftHaptics, 0.2f, 0.06f);
            }
        }
    }

    // If it falls out of the world or over the walls, it comes back.
    void ReturnHomeIfLost()
    {
        if (transform.position.y > -3f && transform.position.y < 30f)
            return;

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = home;
        transform.position = home;
    }
}
