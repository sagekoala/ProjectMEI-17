
using UnityEngine;

// Spectral lantern. It is picked up with telekinesis and then held like a
// flashlight: in front of the hand, pointing where the hand points, so it
// is aimed by turning the wrist. Holding Y makes it shine a misty green
// beam; a ghost inside the beam is banished after a moment of light.
// Its size changes the beam's radius: bigger = wider (easier to hit),
// smaller = narrower. The length stays the same.
public class GhostLantern : MonoBehaviour
{
    const float MinBeamAngle = 6f;
    const float MaxBeamAngle = 35f;
    const int MistPuffs = 8;

    static readonly Color BeamColor = new Color(0.45f, 1f, 0.55f, 1f);
    static readonly Color LensOn = new Color(0.6f, 1f, 0.65f);
    static readonly Color LensReady = new Color(0.25f, 0.55f, 0.3f);
    static readonly Color LensOff = new Color(0.15f, 0.25f, 0.17f);

    // Layered cones: (relative radius, alpha). Denser in the middle.
    static readonly Vector2[] ConeLayers =
    {
        new Vector2(1f, 0.05f),
        new Vector2(0.65f, 0.07f),
        new Vector2(0.3f, 0.10f),
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

    private bool wasHeld;
    private float grabTime;
    private bool hintShown;

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

    // The beam is its own object (not a child), so the lantern's scale
    // doesn't stretch it twice.
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

        GameObject lightObject = new GameObject("Lantern Light");
        lightObject.transform.SetParent(beamRoot, false);
        beamLight = lightObject.AddComponent<Light>();
        beamLight.type = LightType.Spot;
        beamLight.color = new Color(0.5f, 1f, 0.55f);
        beamLight.intensity = 4f;
        beamLight.shadows = LightShadows.None;

        root.SetActive(false);
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

    void Start()
    {
        levitation = FindAnyObjectByType<WizardLevitation>();
    }

    void OnDestroy()
    {
        if (beamRoot != null)
            Destroy(beamRoot.gameObject);
    }

    // 1 = normal size.
    public float SizeMultiplier =>
        baseScale.x > 0f ? transform.localScale.x / baseScale.x : 1f;

    public float BeamRange => system.baseBeamRange;

    // Only the width depends on the size.
    public float BeamAngle => Mathf.Clamp(
        system.baseBeamAngle * SizeMultiplier, MinBeamAngle, MaxBeamAngle);

    bool ShineButtonPressed =>
        levitation != null &&
        levitation.rotateAction != null &&
        levitation.rotateAction.action != null &&
        levitation.rotateAction.action.IsPressed();

    void LateUpdate()
    {
        IsHeld = levitation != null && levitation.HeldObject == body;
        IsShining = IsHeld && ShineButtonPressed;

        if (IsHeld && !wasHeld)
        {
            grabTime = Time.time;
            hintShown = false;
        }
        wasHeld = IsHeld;

        // A reminder if the lantern is held but not lit.
        if (IsHeld && !IsShining && !hintShown && Time.time - grabTime > 0.6f)
        {
            hintShown = true;
            FloatingText hint = FloatingText.Spawn(
                transform.position + Vector3.up * 0.2f,
                "Mantén Y para encender la luz",
                new Color(0.6f, 1f, 0.65f));
            hint.duration = 2.5f;
            hint.riseDistance = 0.1f;
        }

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

        beamLight.range = range * 1.2f;
        beamLight.spotAngle = angle * 2f;

        LightGhost(origin, range, angle);
    }

    // Puffs drift from the lantern to the end of the beam, growing with
    // the cone and fading in and out.
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
            color.a = 0.10f * Mathf.Sin(Mathf.PI * t);
            GlowSphere.SetColor(puffRenderers[i], color);
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
            ghost.Expose(Time.deltaTime);
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
