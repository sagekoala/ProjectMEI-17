
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinSpawner : MonoBehaviour
{
    [Header("Pumpkin Setup")]
    public GameObject pumpkinPrefab;
    public Transform[] spawnPoints;
    public int maxActivePumpkins = 3;

    [Header("Respawn Settings")]
    public float respawnDelay = 6f;

    private readonly List<GameObject> activePumpkins = new();
    private readonly List<int> occupiedPoints = new();

    void Start()
    {
        if (pumpkinPrefab == null || spawnPoints == null)
            return;

        for (int i = 0; i < Mathf.Min(maxActivePumpkins, spawnPoints.Length); i++)
            SpawnPumpkin();
    }

    void SpawnPumpkin()
    {
        if (GraveyardGameManager.Instance != null && GraveyardGameManager.Instance.GameOver)
            return;

        List<int> availablePoints = new();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null && !occupiedPoints.Contains(i))
                availablePoints.Add(i);
        }

        if (availablePoints.Count == 0)
            return;

        int selectedIndex = availablePoints[Random.Range(0, availablePoints.Count)];
        Transform spawnPoint = spawnPoints[selectedIndex];

        GameObject pumpkin = Instantiate(
            pumpkinPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        activePumpkins.Add(pumpkin);
        occupiedPoints.Add(selectedIndex);

        StartCoroutine(WatchPumpkin(pumpkin, selectedIndex));
    }

    IEnumerator WatchPumpkin(GameObject pumpkin, int spawnIndex)
    {
        yield return new WaitUntil(() => pumpkin == null);

        activePumpkins.Remove(pumpkin);
        occupiedPoints.Remove(spawnIndex);

        yield return new WaitForSeconds(respawnDelay);

        SpawnPumpkin();
    }
}
