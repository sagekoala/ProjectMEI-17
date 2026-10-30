
using UnityEngine;

// A ghost built from glowing shapes: a translucent body, a tail and dark
// eyes, with a pale light. It floats towards the wizard's head, bobbing.
// The lantern's beam makes it flicker, slows it down and, after enough
// light, banishes it. If it reaches the wizard, a life is lost.
public class GhostEnemy : MonoBehaviour
{
    public float catchRadius = 0.6f;
    [Tooltip("Lowest height while it is still far away, so it floats over the goblins.")]
    public float cruiseHeight = 1.8f;

    static readonly Color BodyColor = new Color(0.75f, 0.95f, 1f, 0.55f);
    static readonly Color LitColor = new Color(1f, 1f, 0.8f, 0.85f);

    private GhostSystem system;
    private Transform head;
    private Renderer[] bodyRenderers;
    private Light glow;
    private Vector3 visualScale = Vector3.one;

    private float exposure;
    private bool litThisFrame;
    private float bobOffset;
    private bool gone;

    public float Radius => 0.35f;

    public static GhostEnemy Spawn(Vector3 position, GhostSystem system)
    {
        GameObject root = new GameObject("Ghost");
        root.transform.position = position;

        GhostEnemy ghost = root.AddComponent<GhostEnemy>();
        ghost.system = system;
        ghost.Build();

        MagicFeedback.Play(MagicFeedback.WhooshSound, position, 1f);
        if (GraveyardFeedback.Instance != null)
            GraveyardFeedback.Instance.GhostAppeared(position);

        return ghost;
    }

    void Build()
    {
        Material translucent = Resources.Load<Material>("GlowTransparent");

        GameObject body = MakePart("Body", PrimitiveType.Sphere,
            new Vector3(0f, 0f, 0f), new Vector3(0.55f, 0.65f, 0.55f), translucent, BodyColor);
        GameObject tail = MakePart("Tail", PrimitiveType.Sphere,
            new Vector3(0f, -0.38f, -0.08f), new Vector3(0.38f, 0.42f, 0.38f), translucent, BodyColor);

        bodyRenderers = new[] { body.GetComponent<Renderer>(), tail.GetComponent<Renderer>() };

        // Dark eyes on the front (+Z), which faces the wizard.
        foreach (float x in new[] { -0.1f, 0.1f })
        {
            GameObject eye = GlowSphere.Create("Eye", transform.position, 0.09f,
                new Color(0.05f, 0.02f, 0.08f));
            eye.transform.SetParent(transform, false);
            eye.transform.localPosition = new Vector3(x, 0.08f, 0.24f);
            eye.transform.localScale = new Vector3(0.08f, 0.11f, 0.05f);
        }

        GameObject lightObject = new GameObject("Ghost Light");
        lightObject.transform.SetParent(transform, false);
        glow = lightObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(0.6f, 0.9f, 1f);
        glow.range = 3f;
        glow.intensity = 1.5f;
        glow.shadows = LightShadows.None;

        bobOffset = Random.value * 10f;
    }

    GameObject MakePart(string name, PrimitiveType type, Vector3 localPosition,
        Vector3 localScale, Material material, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        Destroy(part.GetComponent<Collider>());
        part.transform.SetParent(transform, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;

        Renderer renderer = part.GetComponent<Renderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        if (material != null)
            renderer.sharedMaterial = material;
        GlowSphere.SetColor(renderer, color);

        return part;
    }

    // Called by the lantern every frame the ghost is inside its beam.
    public void Expose(float deltaTime)
    {
        if (gone)
            return;

        exposure += deltaTime;
        litThisFrame = true;

        if (exposure >= system.banishTime)
            Banish();
    }

    void Update()
    {
        if (gone)
            return;

        GraveyardGameManager game = GraveyardGameManager.Instance;
        if (game != null && !game.IsPlaying)
            return;

        if (head == null && Camera.main != null)
            head = Camera.main.transform;
        if (head == null)
            return;

        // The light slowly fades from the ghost when out of the beam.
        if (!litThisFrame)
            exposure = Mathf.Max(0f, exposure - Time.deltaTime * 0.5f);

        float lit = system.banishTime > 0f ? exposure / system.banishTime : 0f;

        Move(lit);
        UpdateLook(lit);

        litThisFrame = false;

        if (Vector3.Distance(transform.position, head.position) <= catchRadius)
            Catch();
    }

    void Move(float lit)
    {
        Vector3 target = head.position;

        // While far, it floats high over the goblins and comes down
        // as it gets close.
        float horizontal = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(target.x, 0f, target.z));
        if (horizontal > 3f)
            target.y = Mathf.Max(target.y, cruiseHeight + 0.6f);

        // In the light it slows down to a crawl.
        float speed = system.ghostSpeed * Mathf.Lerp(1f, 0.25f, Mathf.Clamp01(lit * 2f));

        Vector3 next = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        next.y += Mathf.Sin((Time.time + bobOffset) * 2f) * 0.15f * Time.deltaTime;
        transform.position = next;

        Vector3 look = head.position - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(look);
    }

    // Lit ghosts flicker, brighten and shrink.
    void UpdateLook(float lit)
    {
        Color color = Color.Lerp(BodyColor, LitColor, lit);
        if (lit > 0f)
            color.a *= 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 30f));

        foreach (Renderer renderer in bodyRenderers)
            GlowSphere.SetColor(renderer, color);

        transform.localScale = visualScale * Mathf.Lerp(1f, 0.7f, lit);
        glow.intensity = Mathf.Lerp(1.5f, 4f, lit);
    }

    void Banish()
    {
        gone = true;

        GameObject flash = GlowSphere.Create("Ghost Banished", transform.position, 1.4f,
            new Color(1f, 1f, 0.85f));
        Destroy(flash, 0.3f);

        MagicFeedback.Play(MagicFeedback.ChimeSound, transform.position, 1f);
        FloatingText.Spawn(transform.position + Vector3.up * 0.4f,
            $"¡Fantasma desterrado!\n+{GraveyardGameManager.Instance?.pointsPerGhost ?? 0} pts",
            new Color(1f, 1f, 0.7f));

        GraveyardGameManager.Instance?.GhostBanished();
        Leave();
    }

    void Catch()
    {
        gone = true;

        GraveyardGameManager game = GraveyardGameManager.Instance;
        game?.GhostCaughtPlayer();

        if (GraveyardFeedback.Instance != null)
            GraveyardFeedback.Instance.PlayerCaught(head.position);

        if (game != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            FloatingText message = FloatingText.Spawn(
                head.position + forward * 1.2f,
                $"¡El fantasma te atrapó!\nVidas: {game.GhostLivesLeft}",
                new Color(0.7f, 0.95f, 1f));
            message.duration = 2.5f;
            message.riseDistance = 0.15f;
        }

        Leave();
    }

    void Leave()
    {
        system.GhostGone();
        Destroy(gameObject);
    }
}
