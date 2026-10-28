
using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float impactSize = 0.3f;
    public float impactDuration = 0.2f;

    private bool hasHit = false;
    private Renderer projectileRenderer;

    private void Awake()
    {
        projectileRenderer = GetComponent<Renderer>();
    }

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

        // Preserve the projectile color for the impact effect.
        Color impactColor = Color.cyan;

        if (projectileRenderer != null)
        {
            impactColor = projectileRenderer.material.color;
        }

        // Create a temporary visual effect at the collision point.
        GameObject flash = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        flash.name = "Magic Impact";
        flash.transform.position = impactPosition;
        flash.transform.localScale = Vector3.one * impactSize;

        // The impact effect should not interact with physics.
        Destroy(flash.GetComponent<Collider>());

        Renderer flashRenderer = flash.GetComponent<Renderer>();
        flashRenderer.material.color = impactColor;

        Destroy(flash, impactDuration);

        // Remove the projectile after impact.
        Destroy(gameObject);
    }
}
