using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// Custom navigation: throw a teleport orb that flies in an arc. If it
// lands on teleportable ground, the player appears there; otherwise it
// flashes red and nothing happens.
public class WizardTeleportOrb : MonoBehaviour
{
    [Header("Teleportation")]
    public TeleportationProvider teleportationProvider;

    [Header("Input")]
    [Tooltip("Input System binding path of the button that throws the orb.")]
    public string throwBinding = "<XRController>{RightHand}/secondaryButton";

    [Header("Orb Settings")]
    public float throwSpeed = 9f;
    public float orbSize = 0.15f;
    public float orbLifetime = 4f;
    public Color orbColor = new Color(1f, 0.85f, 0.3f);
    public Color invalidColor = new Color(1f, 0.2f, 0.2f);

    [Tooltip("Minimum dot product between the surface normal and world up for a surface to count as floor.")]
    public float minFloorDot = 0.7f;

    [Header("Impact Flash")]
    public float flashSize = 0.6f;
    public float flashDuration = 0.3f;

    [Header("Feedback")]
    [Tooltip("Haptics of the throwing controller.")]
    public HapticImpulsePlayer haptics;

    private InputAction throwAction;
    private GameObject activeOrb;

    void Awake()
    {
        throwAction = new InputAction(
            "Throw Teleport Orb", InputActionType.Button, throwBinding
        );

        if (teleportationProvider == null)
        {
            Debug.LogError(
                "WizardTeleportOrb has no TeleportationProvider; " +
                "orbs will not teleport.", this);
        }
    }

    void OnEnable()
    {
        throwAction.Enable();
    }

    void OnDisable()
    {
        throwAction.Disable();
    }

    void OnDestroy()
    {
        throwAction.Dispose();
    }

    void Update()
    {
        // Only one orb in flight at a time.
        if (throwAction.WasPressedThisFrame() && activeOrb == null)
            ThrowOrb();
    }

    void ThrowOrb()
    {
        Vector3 direction = transform.forward;

        GameObject orb = GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );

        orb.name = "Teleport Orb";
        orb.transform.position =
            transform.position + direction * 0.25f;
        orb.transform.localScale =
            Vector3.one * orbSize;

        Renderer renderer = orb.GetComponent<Renderer>();
        renderer.material.color = orbColor;

        TeleportOrbProjectile projectile =
            orb.AddComponent<TeleportOrbProjectile>();
        projectile.owner = this;

        // Gravity makes it fly in an arc, like a thrown stone.
        Rigidbody rb = orb.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;
        rb.linearVelocity = direction * throwSpeed;

        activeOrb = orb;

        MagicFeedback.Play(MagicFeedback.WhooshSound, orb.transform.position, 0.6f);
        MagicFeedback.Pulse(haptics, 0.25f, 0.1f);

        // Remove the orb if it never lands.
        Destroy(orb, orbLifetime);
    }

    // Called by the orb when it hits something.
    public void OnOrbLanded(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        bool isFloor =
            Vector3.Dot(contact.normal, Vector3.up) >= minFloorDot;

        bool isTeleportable =
            collision.collider.GetComponentInParent<TeleportationArea>() != null;

        bool canTeleport =
            isFloor && isTeleportable && teleportationProvider != null;

        MagicProjectile.SpawnFlash(
            contact.point,
            flashSize,
            canTeleport ? orbColor : invalidColor,
            flashDuration
        );

        if (!canTeleport)
        {
            MagicFeedback.Play(MagicFeedback.FailSound, contact.point, 0.8f);
            MagicFeedback.Pulse(haptics, 0.15f, 0.2f);
            return;
        }

        MagicFeedback.Play(MagicFeedback.ChimeSound, contact.point, 0.8f);
        MagicFeedback.Pulse(haptics, 0.5f, 0.15f);

        teleportationProvider.QueueTeleportRequest(new TeleportRequest
        {
            destinationPosition = contact.point,
            destinationRotation = Quaternion.identity,
            matchOrientation = MatchOrientation.None,
            requestTime = Time.time,
        });
    }
}
