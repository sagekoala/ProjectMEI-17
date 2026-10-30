
using UnityEngine;

public class GoblinEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.8f;
    public float attackDistance = 0.6f;

    private PumpkinItem target;

    [Header("Eating")]
    [Tooltip("Seconds to eat a pumpkin of normal size. Bigger pumpkins " +
             "take longer (2x size = twice as long), smaller ones less: " +
             "time for the wizard to save them.")]
    public float eatTimePerSize = 1.5f;

    [Tooltip("A pumpkin levitated higher than this above the goblin is " +
             "out of reach: it waits underneath.")]
    public float reachHeight = 1.2f;

    private PumpkinItem eating;
    private float eatTimer;

    [Header("Combat")]
    public int maxHealth = 6;

    private int currentHealth;
    private GoblinHealthBar healthBar;

    void Update()
    {
        if (GraveyardGameManager.Instance != null &&
            !GraveyardGameManager.Instance.IsPlaying)
            return;

        FindTarget();

        if (target == null)
            return;

        Vector3 destination = target.transform.position;
        destination.y = transform.position.y;

        Vector3 direction = destination - transform.position;

        if (direction.magnitude <= attackDistance)
        {
            float height = target.transform.position.y - transform.position.y;

            if (height > reachHeight)
            {
                // Out of reach (levitated): wait underneath.
                eating = null;
                return;
            }

            if (eating != target)
            {
                // Starts eating: warn the player.
                eating = target;
                eatTimer = 0f;
                FloatingText.Spawn(
                    target.transform.position + Vector3.up * 0.4f,
                    "¡ÑAM!", new Color(1f, 0.3f, 0.25f));
                MagicFeedback.Play(MagicFeedback.GrowlSound, transform.position, 0.8f);
            }

            eatTimer += Time.deltaTime;

            if (eatTimer < eatTimePerSize * target.SizeMultiplier)
            {
                // Chewing: stays in place, facing the pumpkin.
                if (direction.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(direction);
                return;
            }

            eating = null;

            if (GraveyardGameManager.Instance != null)
                GraveyardGameManager.Instance.PumpkinDestroyed();

            if (GraveyardFeedback.Instance != null)
                GraveyardFeedback.Instance.PumpkinLost(target.transform.position);

            Destroy(target.gameObject);
            target = null;
            return;
        }

        // The pumpkin got away (e.g. levitated): stop eating.
        eating = null;

        // Move horizontally toward the nearest pumpkin.
        transform.position +=
            direction.normalized * moveSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Awake()
    {
        currentHealth = maxHealth;
        healthBar = gameObject.AddComponent<GoblinHealthBar>();
    }

    void Start()
    {
        // A growl from where it appears tells the player where to look.
        if (GraveyardFeedback.Instance != null)
            GraveyardFeedback.Instance.GoblinAppeared(transform.position);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Show the hit: red flash, shorter health bar and a thump.
        healthBar.SetHealth((float)currentHealth / maxHealth);
        healthBar.Flash();
        MagicFeedback.Play(MagicFeedback.HitSound, transform.position, 0.8f);

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
