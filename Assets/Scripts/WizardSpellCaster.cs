
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

    [Header("Spell Appearance")]
    public Color[] spellColors =
    {
        Color.cyan,
        new Color(0.65f, 0.25f, 1f),
        Color.green,
        Color.red
    };

    private int currentColorIndex = 0;

    void Update()
    {
        if (castAction == null || castAction.action == null)
            return;

        if (castAction.action.WasPressedThisFrame())
        {
            CastSpell();
        }
    }

    // Cycles through the available spell colors.
    public void ChangeSpellColor()
    {
        if (spellColors == null || spellColors.Length == 0)
            return;

        currentColorIndex =
            (currentColorIndex + 1) % spellColors.Length;
    }

    // Creates a projectile in the direction of the controller.
    void CastSpell()
    {
        Vector3 direction = transform.forward;

        Vector3 spawnPosition =
            transform.position + direction * 0.25f;

        GameObject spell = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        spell.name = "Magic Orb";
        spell.transform.position = spawnPosition;
        spell.transform.localScale =
            Vector3.one * spellSize;

        // Apply the currently selected spell color.
        Renderer renderer = spell.GetComponent<Renderer>();

        if (spellColors != null && spellColors.Length > 0)
        {
            renderer.material.color =
                spellColors[currentColorIndex % spellColors.Length];
        }
        else
        {
            renderer.material.color = Color.cyan;
        }

        // Handle projectile collisions and impact effects.
        spell.AddComponent<MagicProjectile>();

        Rigidbody rb = spell.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        rb.linearVelocity = direction * spellSpeed;

        // Remove projectiles that do not collide with anything.
        Destroy(spell, spellLifetime);
    }
}
