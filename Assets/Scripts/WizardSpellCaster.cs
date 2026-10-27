
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardSpellCaster : MonoBehaviour
{
    [Header("Quest 3 Input")]
    public InputActionReference castAction;

    [Header("Spell Settings")]
    public float spellSpeed = 12f;
    public float spellLifetime = 5f;
    public float spellSize = 0.12f;
    public Color spellColor = Color.cyan;

    void Update()
    {
        if (castAction == null || castAction.action == null)
            return;

        if (castAction.action.WasPressedThisFrame())
        {
            CastSpell();
        }
    }

    void CastSpell()
    {
        Vector3 direction = transform.forward;
        Vector3 spawnPosition =
            transform.position + direction * 0.25f;

        // Create the magic orb
        GameObject spell = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        spell.name = "Magic Orb";
        spell.transform.position = spawnPosition;
        spell.transform.localScale =
            Vector3.one * spellSize;

        // Set the orb color
        Renderer renderer = spell.GetComponent<Renderer>();
        renderer.material.color = spellColor;

        // NEW: Add impact detection
        MagicProjectile projectile =
            spell.AddComponent<MagicProjectile>();
        projectile.impactColor = spellColor;

        // Add physics
        Rigidbody rb = spell.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        rb.linearVelocity = direction * spellSpeed;

        // Remove the spell if it never hits anything
        Destroy(spell, spellLifetime);
    }
}
