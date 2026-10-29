
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

    private float elapsedTime;
    private bool secondGoblinStarted;

    void Start()
    {
        StartCoroutine(GoblinSlot());
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
            StartCoroutine(GoblinSlot());
        }
    }

    IEnumerator GoblinSlot()
    {
        // No goblins before the round starts (e.g. during the tutorial).
        if (GraveyardGameManager.Instance != null)
        {
            yield return new WaitUntil(() =>
                GraveyardGameManager.Instance == null ||
                GraveyardGameManager.Instance.RoundStarted
            );
        }

        while (true)
        {
            if (GraveyardGameManager.Instance != null &&
                GraveyardGameManager.Instance.GameOver)
                yield break;

            if (goblinPrefab == null ||
                spawnPoints == null ||
                spawnPoints.Length == 0)
                yield break;

            Transform point = spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

            if (point == null)
                yield break;

            GameObject goblin = Instantiate(
                goblinPrefab,
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

            yield return new WaitForSeconds(respawnDelay);
        }
    }
}
