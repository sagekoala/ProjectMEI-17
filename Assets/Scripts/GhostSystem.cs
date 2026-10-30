
using UnityEngine;

// Spawns the lantern and the ghost
public class GhostSystem : MonoBehaviour
{
    public static GhostSystem Instance { get; private set; }

    [Header("Ghost")]
    [Tooltip("First ghost delay (s)")]
    public float firstGhostDelay = 20f;
    [Tooltip("Respawn delay (s)")]
    public float respawnDelay = 8f;
    public float ghostSpeed = 0.8f;
    public float spawnHeight = 3f;
    [Tooltip("Light time to kill it (s)")]
    public float banishTime = 2f;

    [Header("Lantern")]
    public Vector3 lanternPosition = new Vector3(1.3f, 1.2f, -11.6f);
    [Tooltip("Beam length")]
    public float baseBeamRange = 8f;
    [Tooltip("Beam half-angle at size 1 (wider when bigger)")]
    public float baseBeamAngle = 15f;

    public GhostLantern Lantern { get; private set; }
    public GhostEnemy Ghost { get; private set; }

    private GraveyardGameManager game;
    private PlayAreaBounds bounds;
    private float nextSpawnTime = -1f;

    void Awake()
    {
        Instance = this;
        Lantern = GhostLantern.Create(lanternPosition, this);
    }

    void Start()
    {
        game = GraveyardGameManager.Instance;
        bounds = FindAnyObjectByType<PlayAreaBounds>();

        // generate the wail now, not when the first ghost appears (hitch)
        _ = MagicFeedback.GhostWailLoop;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (game == null || !game.IsPlaying)
            return;

        if (nextSpawnTime < 0f)
            nextSpawnTime = Time.time + firstGhostDelay;

        if (Ghost == null && Time.time >= nextSpawnTime)
            Ghost = GhostEnemy.Spawn(FarSidePosition(), this);
    }

    // Called by the ghost when it leaves (banished or after catching).
    public void GhostGone()
    {
        Ghost = null;
        nextSpawnTime = Time.time + respawnDelay;
    }

    // The corner of the cemetery farthest from the wizard, up in the air.
    Vector3 FarSidePosition()
    {
        Vector3 player = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        if (bounds == null)
            return player + new Vector3(8f, spawnHeight, 8f);

        Vector3 half = bounds.size * 0.5f - Vector3.one * 1f;
        Vector3 best = bounds.center;
        float bestDistance = -1f;

        foreach (float sx in new[] { -1f, 1f })
        {
            foreach (float sz in new[] { -1f, 1f })
            {
                Vector3 corner = bounds.center + new Vector3(sx * half.x, 0f, sz * half.z);
                float distance = Vector3.Distance(
                    new Vector3(corner.x, 0f, corner.z),
                    new Vector3(player.x, 0f, player.z));

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = corner;
                }
            }
        }

        best.y = spawnHeight;
        return best;
    }
}
