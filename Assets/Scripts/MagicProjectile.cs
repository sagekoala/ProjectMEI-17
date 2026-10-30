
using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float impactSize = 0.3f;
    public float impactDuration = 0.2f;

    [Tooltip("Spell color, reused for the impact flash.")]
    public Color color = Color.cyan;

    private bool hasHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit)
            return;

        hasHit = true;

        Vector3 impactPosition = collision.GetContact(0).point;

        GoblinEnemy goblin =
            collision.collider.GetComponentInParent<GoblinEnemy>();

        if (goblin != null)
        {
            goblin.TakeDamage(1);
        }

        // A short flash in the spell's color at the collision point.
        GameObject flash = GlowSphere.Create(
            "Magic Impact", impactPosition, impactSize, color
        );

        Destroy(flash, impactDuration);

        MagicFeedback.Play(MagicFeedback.ImpactSound, impactPosition, 0.6f);

        // Remove the projectile after impact.
        Destroy(gameObject);
    }
}
