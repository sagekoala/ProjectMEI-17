
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

        SpawnFlash(
            impactPosition, impactSize, impactColor, impactDuration
        );

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
