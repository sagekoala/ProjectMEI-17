
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardLevitation : MonoBehaviour
{
    [Header("Quest 3 Input")]
    public InputActionReference levitateAction;

    [Header("Levitation Settings")]
    public float maxDistance = 8f;
    public float holdDistance = 1.5f;
    public float moveSpeed = 12f;

    [Header("Rotation")]
    [Tooltip("The held object follows the twist of this hand.")]
    public bool rotateWithHand = true;
    public float rotateSpeed = 10f;

    [Header("Two-Hand Scaling")]
    [Tooltip("The other hand. Hold its grip and move the hands apart or together to scale the held object.")]
    public Transform otherHand;

    [Tooltip("Input System binding path of the button that scales.")]
    public string scaleBinding = "<XRController>{RightHand}/{GripButton}";

    [Tooltip("Scale limits, relative to the object's original size.")]
    public float minScaleFactor = 0.25f;
    public float maxScaleFactor = 4f;

    private Rigidbody heldObject;
    private bool originalGravity;
    private bool originalKinematic;

    private Quaternion grabHandRotation;
    private Quaternion grabObjectRotation;

    private InputAction scaleAction;
    private bool isScaling;
    private float scaleStartHandDistance;
    private Vector3 scaleStartObjectScale;

    // Size of each object the first time it was levitated, so
    // repeated scaling stays within the limits.
    private readonly Dictionary<Rigidbody, Vector3> originalScales =
        new Dictionary<Rigidbody, Vector3>();

    // The object currently being levitated, or null.
    public Rigidbody HeldObject => heldObject;

    void Awake()
    {
        scaleAction = new InputAction(
            "Scale Levitated Object", InputActionType.Button, scaleBinding
        );
    }

    void OnEnable()
    {
        scaleAction.Enable();
    }

    void OnDestroy()
    {
        scaleAction.Dispose();
    }

    void Update()
    {
        if (levitateAction == null || levitateAction.action == null)
            return;

        if (levitateAction.action.WasPressedThisFrame())
            TryLevitate();

        if (levitateAction.action.WasReleasedThisFrame())
            ReleaseObject();

        UpdateScaling();
    }

    void FixedUpdate()
    {
        if (heldObject == null)
            return;

        Vector3 targetPosition =
            transform.position + transform.forward * holdDistance;

        Vector3 direction =
            targetPosition - heldObject.position;

        heldObject.linearVelocity =
            direction * moveSpeed;

        if (rotateWithHand)
            RotateWithHand();
    }

    // Applies the hand's rotation since the grab to the held object,
    // through angular velocity so it still collides normally.
    void RotateWithHand()
    {
        Quaternion handDelta =
            transform.rotation * Quaternion.Inverse(grabHandRotation);

        Quaternion targetRotation = handDelta * grabObjectRotation;

        Quaternion remaining =
            targetRotation * Quaternion.Inverse(heldObject.rotation);

        remaining.ToAngleAxis(out float angle, out Vector3 axis);

        // Take the short way round.
        if (angle > 180f)
            angle -= 360f;

        if (Mathf.Abs(angle) < 0.01f || !float.IsFinite(axis.x))
        {
            heldObject.angularVelocity = Vector3.zero;
            return;
        }

        heldObject.angularVelocity =
            axis * (angle * Mathf.Deg2Rad * rotateSpeed);
    }

    void UpdateScaling()
    {
        bool wantsToScale =
            heldObject != null &&
            otherHand != null &&
            scaleAction.IsPressed();

        if (!wantsToScale)
        {
            isScaling = false;
            return;
        }

        float handDistance =
            Vector3.Distance(transform.position, otherHand.position);

        if (!isScaling)
        {
            // Gesture starts: remember the current hand distance and size.
            isScaling = true;
            scaleStartHandDistance = Mathf.Max(handDistance, 0.01f);
            scaleStartObjectScale = heldObject.transform.localScale;
            return;
        }

        Vector3 originalScale = originalScales[heldObject];
        if (originalScale.x <= 0f)
            return;

        float gestureFactor = handDistance / scaleStartHandDistance;

        // Uniform scale, clamped relative to the original size.
        float totalFactor = Mathf.Clamp(
            scaleStartObjectScale.x * gestureFactor / originalScale.x,
            minScaleFactor,
            maxScaleFactor
        );

        heldObject.transform.localScale = originalScale * totalFactor;
    }

    // Returns the levitatable object this hand points at, or null.
    public Rigidbody FindLevitatable(out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        Ray ray = new Ray(transform.position, transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            return null;

        Rigidbody rb = hit.rigidbody;

        if (rb == null)
            return null;

        // Don't grab kinematic or unrelated objects.
        if (rb.isKinematic)
            return null;

        // Only grab objects tagged as levitatable.
        if (!rb.CompareTag("Levitatable"))
            return null;

        hitPoint = hit.point;
        return rb;
    }

    void TryLevitate()
    {
        Rigidbody rb = FindLevitatable(out _);

        if (rb != null)
        {
            heldObject = rb;

            originalGravity = rb.useGravity;
            originalKinematic = rb.isKinematic;

            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            grabHandRotation = transform.rotation;
            grabObjectRotation = rb.rotation;

            if (!originalScales.ContainsKey(rb))
                originalScales.Add(rb, rb.transform.localScale);
        }
    }

    void ReleaseObject()
    {
        if (heldObject == null)
            return;

        heldObject.useGravity = originalGravity;
        heldObject.isKinematic = originalKinematic;
        heldObject.angularVelocity = Vector3.zero;
        heldObject = null;
        isScaling = false;
    }

    void OnDisable()
    {
        ReleaseObject();
        scaleAction.Disable();
    }
}
