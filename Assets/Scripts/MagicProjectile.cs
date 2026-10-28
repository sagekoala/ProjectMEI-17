
using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float impactSize = 0.3f;
    public float impactDuration = 0.2f;
    public Color impactColor = Color.cyan;

    [Tooltip("Impulse applied to the physics object that is hit.")]
    public float pushImpulse = 3f;

    // Set by the caster; used to push hit objects.
    public Vector3 travelDirection = Vector3.forward;

    private bool hasHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        Vector3 impactPosition =
            collision.GetContact(0).point;

        // Spells knock physics objects (e.g. levitatable graves) back.
        Rigidbody body = collision.rigidbody;
        if (body != null && !body.isKinematic)
        {
            body.AddForceAtPosition(
                travelDirection * pushImpulse,
                impactPosition,
                ForceMode.Impulse
            );
        }

        Hit(collision.collider, impactPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Fly through plain triggers; only spell targets stop the spell.
        if (other.GetComponentInParent<ISpellTarget>() == null)
            return;

        Hit(other, transform.position);
    }

    void Hit(Collider other, Vector3 impactPosition)
    {
        hasHit = true;

        ISpellTarget target = other.GetComponentInParent<ISpellTarget>();
        if (target != null)
            target.OnSpellHit(impactPosition, impactColor);

        SpawnFlash(
            impactPosition, impactSize, impactColor, impactDuration
        );
        MagicFeedback.Play(MagicFeedback.ImpactSound, impactPosition, 0.7f);

        // Remove the original projectile.
        Destroy(gameObject);
    }

    // Creates a temporary magical flash.
    public static void SpawnFlash(
        Vector3 position, float size, Color color, float duration)
    {
        GameObject flash = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        flash.name = "Magic Impact";
        flash.transform.position = position;
        flash.transform.localScale =
            Vector3.one * size;

        // The flash is visual only.
        Destroy(flash.GetComponent<Collider>());

        Renderer renderer = flash.GetComponent<Renderer>();
        renderer.material.color = color;

        Destroy(flash, duration);
    }
}
