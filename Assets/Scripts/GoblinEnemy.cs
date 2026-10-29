
using UnityEngine;

public class GoblinEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.8f;
    public float attackDistance = 0.6f;

    private PumpkinItem target;

    [Header("Combat")]
    public int maxHealth = 6;

    private int currentHealth;

    void Update()
    {
        if (GraveyardGameManager.Instance != null &&
            GraveyardGameManager.Instance.GameOver)
            return;

        FindTarget();

        if (target == null)
            return;

        Vector3 destination = target.transform.position;
        destination.y = transform.position.y;

        Vector3 direction = destination - transform.position;

        if (direction.magnitude <= attackDistance)
        {
            if (GraveyardGameManager.Instance != null)
                GraveyardGameManager.Instance.PumpkinDestroyed();

            Destroy(target.gameObject);
            target = null;
            return;
        }

        // Move horizontally toward the nearest pumpkin.
        transform.position +=
            direction.normalized * moveSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log($"Goblin hit! Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            if (GraveyardGameManager.Instance != null)
                GraveyardGameManager.Instance.GoblinDefeated();

            Debug.Log("Goblin defeated!");
            Explode();
            Destroy(gameObject);
        }
    }

    // Bursts into chunks of its own material, from the middle of its body.
    void Explode()
    {
        Renderer body = GetComponentInChildren<Renderer>();

        Vector3 center = body != null
            ? body.bounds.center
            : transform.position + Vector3.up * 0.5f;

        GoblinExplosion.Explode(
            center, body != null ? body.sharedMaterial : null
        );
    }

    void FindTarget()
    {
        PumpkinItem[] pumpkins = FindObjectsByType<PumpkinItem>(
            FindObjectsSortMode.None
        );

        float closestDistance = float.MaxValue;
        PumpkinItem closest = null;

        foreach (PumpkinItem pumpkin in pumpkins)
        {
            if (!pumpkin.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                pumpkin.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = pumpkin;
            }
        }

        target = closest;
    }
}
