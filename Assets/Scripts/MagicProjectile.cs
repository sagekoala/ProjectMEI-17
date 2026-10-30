
using System.Collections.Generic;
using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float impactSize = 0.3f;
    public float impactDuration = 0.2f;

    [Tooltip("Spell color, reused for the impact flash.")]
    public Color color = Color.cyan;

    public float damage = 1f;
    [Tooltip("If > 0, damages every goblin in this radius (m).")]
    public float splashRadius = 0f;

    private bool hasHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit)
            return;

        hasHit = true;

        Vector3 impactPosition = collision.GetContact(0).point;

        if (splashRadius > 0f)
        {
            // Explosive spell: every goblin in the radius is hit once.
            var hit = new HashSet<GoblinEnemy>();
            foreach (Collider nearby in Physics.OverlapSphere(impactPosition, splashRadius))
            {
                GoblinEnemy enemy = nearby.GetComponentInParent<GoblinEnemy>();
                if (enemy != null && hit.Add(enemy))
                    enemy.TakeDamage(damage);
            }
        }
        else
        {
            GoblinEnemy goblin =
                collision.collider.GetComponentInParent<GoblinEnemy>();

            if (goblin != null)
                goblin.TakeDamage(damage);
        }

        // A short flash in the spell's color at the collision point;
        // explosive spells show the size of their blast.
        float flashSize = splashRadius > 0f ? splashRadius * 1.2f : impactSize;
        GameObject flash = GlowSphere.Create(
            "Magic Impact", impactPosition, flashSize, color
        );

        Destroy(flash, splashRadius > 0f ? impactDuration * 1.5f : impactDuration);

        MagicFeedback.Play(
            splashRadius > 0f ? MagicFeedback.ExplosionSound : MagicFeedback.ImpactSound,
            impactPosition, splashRadius > 0f ? 0.8f : 0.6f);

        // Remove the projectile after impact.
        Destroy(gameObject);
    }
}
