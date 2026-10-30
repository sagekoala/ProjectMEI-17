
using UnityEngine;

public class GoblinEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.8f;
    public float attackDistance = 0.6f;

    private PumpkinItem target;

    [Header("Reach")]
    [Tooltip("A pumpkin levitated higher than this above the goblin is " +
             "out of reach: it waits underneath.")]
    public float reachHeight = 1.2f;

    [Header("Combat")]
    public int maxHealth = 6;

    // Float: some spells do 1.5 damage.
    private float currentHealth;
    private bool dead;
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

            // Out of reach (levitated high): wait underneath.
            if (height > reachHeight)
                return;

            if (GraveyardGameManager.Instance != null)
                GraveyardGameManager.Instance.PumpkinDestroyed();

            if (GraveyardFeedback.Instance != null)
                GraveyardFeedback.Instance.PumpkinLost(target.transform.position);

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
        healthBar = gameObject.AddComponent<GoblinHealthBar>();
    }

    void Start()
    {
        // A growl from where it appears tells the player where to look.
        if (GraveyardFeedback.Instance != null)
            GraveyardFeedback.Instance.GoblinAppeared(transform.position);
    }

    public void TakeDamage(float damage)
    {
        if (dead)
            return;

        currentHealth -= damage;

        // Show the hit: red flash, shorter health bar and a thump.
        healthBar.SetHealth(Mathf.Clamp01(currentHealth / maxHealth));
        healthBar.Flash();
        MagicFeedback.Play(MagicFeedback.HitSound, transform.position, 0.8f);

        Debug.Log($"Goblin hit! Health: {currentHealth}");

        if (currentHealth <= 0f)
        {
            dead = true;

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
        float closestDistance = float.MaxValue;
        PumpkinItem closest = null;

        foreach (PumpkinItem pumpkin in PumpkinItem.All)
        {
            if (pumpkin == null)
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
