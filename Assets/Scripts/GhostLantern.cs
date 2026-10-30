
using UnityEngine;

// Spectral lantern: picked up with telekinesis like any other object.
// While it is levitated it shines a visible cone of light along its
// forward axis, so it is aimed by rotating it (Y + wrist). A ghost inside
// the cone is banished after a moment of light.
// Its size changes the beam: bigger = longer but narrower (reaches far,
// harder to aim); smaller = shorter but wider (easy to aim, needs the
// ghost close).
public class GhostLantern : MonoBehaviour
{
    const float MinBeamAngle = 8f;
    const float MaxBeamAngle = 40f;

    static readonly Color BeamColor = new Color(0.75f, 1f, 0.65f, 0.16f);
    static readonly Color LensOn = new Color(0.85f, 1f, 0.7f);
    static readonly Color LensOff = new Color(0.3f, 0.4f, 0.3f);

    private GhostSystem system;
    private Rigidbody body;
    private WizardLevitation levitation;
    private Vector3 baseScale;
    private Vector3 home;

    private Transform beam;
    private Renderer beamRenderer;
    private Light beamLight;
    private Renderer lensRenderer;

    public Rigidbody Body => body;
    public bool IsShining { get; private set; }

    private bool wasHeld;

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

    // A cone of light, kept as its own object (not a child) so the
    // lantern's scale doesn't stretch it twice.
    void BuildBeam()
    {
        GameObject beamObject = new GameObject("Lantern Beam");
        beam = beamObject.transform;

        beamObject.AddComponent<MeshFilter>().sharedMesh = BuildConeMesh(24);
        beamRenderer = beamObject.AddComponent<MeshRenderer>();
        beamRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beamRenderer.receiveShadows = false;

        Material translucent = Resources.Load<Material>("GlowTransparent");
        if (translucent != null)
            beamRenderer.sharedMaterial = translucent;
        GlowSphere.SetColor(beamRenderer, BeamColor);

        GameObject lightObject = new GameObject("Lantern Light");
        lightObject.transform.SetParent(beam, false);
        beamLight = lightObject.AddComponent<Light>();
        beamLight.type = LightType.Spot;
        beamLight.color = new Color(0.85f, 1f, 0.75f);
        beamLight.intensity = 3f;
        beamLight.shadows = LightShadows.None;

        beamObject.SetActive(false);
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
        if (beam != null)
            Destroy(beam.gameObject);
    }

    // 1 = normal size.
    public float SizeMultiplier =>
        baseScale.x > 0f ? transform.localScale.x / baseScale.x : 1f;

    public float BeamRange => system.baseBeamRange * SizeMultiplier;

    public float BeamAngle => Mathf.Clamp(
        system.baseBeamAngle / SizeMultiplier, MinBeamAngle, MaxBeamAngle);

    void LateUpdate()
    {
        bool held = levitation != null && levitation.HeldObject == body;
        IsShining = held;

        // When picked up it starts pointing where the hand points; from
        // then on it keeps its direction and is aimed with Y + wrist.
        if (held && !wasHeld)
        {
            Quaternion aim = Quaternion.LookRotation(levitation.transform.forward, Vector3.up);
            body.rotation = aim;
            transform.rotation = aim;
        }
        wasHeld = held;

        beam.gameObject.SetActive(held);
        GlowSphere.SetColor(lensRenderer, held ? LensOn : LensOff);

        if (!held)
        {
            ReturnHomeIfLost();
            return;
        }

        float range = BeamRange;
        float angle = BeamAngle;
        float radius = range * Mathf.Tan(angle * Mathf.Deg2Rad);

        Vector3 origin = transform.position + transform.forward * 0.15f * SizeMultiplier;
        beam.SetPositionAndRotation(origin, transform.rotation);
        beam.localScale = new Vector3(radius, radius, range);

        beamLight.range = range * 1.2f;
        beamLight.spotAngle = angle * 2f;

        LightGhost(origin, range, angle);
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
