using UnityEngine;

// The thrown teleport orb. Reports its first collision to the thrower.
public class TeleportOrbProjectile : MonoBehaviour
{
    public WizardTeleportOrb owner;

    private bool hasLanded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;
        hasLanded = true;

        if (owner != null)
            owner.OnOrbLanded(collision);

        Destroy(gameObject);
    }
}
