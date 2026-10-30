
using UnityEngine;

// The ghost and the spectral lantern. The lantern lies near the start
// (pick it up with telekinesis); once the round starts, a ghost appears
// on the far side of the cemetery and floats towards the wizard. Lighting
// it with the lantern banishes it; if it touches the wizard, a life is
// lost. Either way it comes back after a while, from the far side.
// Added by GraveyardGameManager if it is not in the scene.
public class GhostSystem : MonoBehaviour
{
    public static GhostSystem Instance { get; private set; }

    [Header("Ghost")]
    [Tooltip("Seconds after the round starts before the first ghost.")]
    public float firstGhostDelay = 20f;
    [Tooltip("Seconds before a new ghost after one is banished or catches you.")]
    public float respawnDelay = 8f;
    public float ghostSpeed = 0.8f;
    public float spawnHeight = 3f;
    [Tooltip("Seconds of light needed to banish the ghost.")]
    public float banishTime = 1.5f;

    [Header("Lantern")]
    public Vector3 lanternPosition = new Vector3(1.3f, 1.2f, -11.6f);
    [Tooltip("Beam length (the same at any size).")]
    public float baseBeamRange = 6f;
    [Tooltip("Beam half-angle at normal size. It widens as the lantern grows.")]
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
