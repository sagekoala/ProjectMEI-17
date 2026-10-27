
using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public float impactSize = 0.3f;
    public float impactDuration = 0.2f;
    public Color impactColor = Color.cyan;

    private bool hasHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Vector3 impactPosition =
            collision.GetContact(0).point;

        // Create a temporary magical flash.
        GameObject flash = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        flash.name = "Magic Impact";
        flash.transform.position = impactPosition;
        flash.transform.localScale =
            Vector3.one * impactSize;

        // The flash is visual only.
        Destroy(flash.GetComponent<Collider>());

        Renderer renderer = flash.GetComponent<Renderer>();
        renderer.material.color = impactColor;

        Destroy(flash, impactDuration);

        // Remove the original projectile.
        Destroy(gameObject);
    }
}
