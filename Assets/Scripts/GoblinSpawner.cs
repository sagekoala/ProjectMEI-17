
using System.Collections;
using UnityEngine;

public class GoblinSpawner : MonoBehaviour
{
    [Header("Goblin Setup")]
    public GameObject goblinPrefab;
    public Transform[] spawnPoints;

    [Header("Difficulty")]
    public float secondGoblinDelay = 60f;

    [Header("Respawning")]
    public float respawnDelay = 7.5f;

    [Header("Final Wave")]
    [Tooltip("Enemy of the final wave (e.g. the fast, fragile goblin archer).")]
    public GameObject finalWavePrefab;

    [Tooltip("The final wave starts when this many seconds of the round are left.")]
    public float finalWaveTimeLeft = 60f;

    [Tooltip("Final wave enemies alive at the same time.")]
    public int finalWaveSlots = 3;

    public float finalWaveRespawnDelay = 3f;

    // The final wave has begun (it then lasts until the round ends).
    public bool FinalWaveStarted { get; private set; }

    private float elapsedTime;
    private bool secondGoblinStarted;

    void Start()
    {
        StartCoroutine(GoblinSlot(goblinPrefab, respawnDelay, 0f));
    }

    void Update()
    {
        if (GraveyardGameManager.Instance != null &&
            !GraveyardGameManager.Instance.IsPlaying)
            return;

        elapsedTime += Time.deltaTime;

        if (!secondGoblinStarted &&
            elapsedTime >= secondGoblinDelay)
        {
            secondGoblinStarted = true;
            StartCoroutine(GoblinSlot(goblinPrefab, respawnDelay, 0f));
        }

        if (!FinalWaveStarted &&
            finalWavePrefab != null &&
            GraveyardGameManager.Instance != null &&
            GraveyardGameManager.Instance.RemainingTime <= finalWaveTimeLeft)
        {
            StartFinalWave();
        }
    }

    // The regular goblins keep coming; the final wave adds its enemies
    // on top, a little apart so they don't all appear at once.
    void StartFinalWave()
    {
        FinalWaveStarted = true;

        if (GraveyardFeedback.Instance != null)
            GraveyardFeedback.Instance.AnnounceFinalWave();

        for (int i = 0; i < finalWaveSlots; i++)
        {
            StartCoroutine(GoblinSlot(
                finalWavePrefab, finalWaveRespawnDelay, 1f + i * 0.8f
            ));
        }
    }

    // Keeps one enemy of this kind alive: spawns it, waits until it dies,
    // waits the respawn delay and spawns the next one.
    IEnumerator GoblinSlot(GameObject prefab, float delay, float firstDelay)
    {
        // No goblins before the round starts (e.g. during the tutorial).
        if (GraveyardGameManager.Instance != null)
        {
            yield return new WaitUntil(() =>
                GraveyardGameManager.Instance == null ||
                GraveyardGameManager.Instance.RoundStarted
            );
        }

        if (firstDelay > 0f)
            yield return new WaitForSeconds(firstDelay);

        while (true)
        {
            if (GraveyardGameManager.Instance != null &&
                GraveyardGameManager.Instance.GameOver)
                yield break;

            if (prefab == null ||
                spawnPoints == null ||
                spawnPoints.Length == 0)
                yield break;

            Transform point = spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

            if (point == null)
                yield break;

            GameObject goblin = Instantiate(
                prefab,
                point.position,
                point.rotation
            );

            yield return new WaitUntil(() =>
                goblin == null ||
                (GraveyardGameManager.Instance != null &&
                 GraveyardGameManager.Instance.GameOver)
            );

            if (GraveyardGameManager.Instance != null &&
                GraveyardGameManager.Instance.GameOver)
                yield break;

            yield return new WaitForSeconds(delay);
        }
    }
}
