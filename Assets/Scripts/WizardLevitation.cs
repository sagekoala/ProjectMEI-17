
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardLevitation : MonoBehaviour
{
    [Header("Quest 3 Input")]
    public InputActionReference levitateAction;
    public InputActionReference rotateAction;
    public InputActionReference enlargeAction;
    public InputActionReference shrinkAction;

    [Header("Levitation Settings")]
    public float maxDistance = 8f;
    public float holdDistance = 1.5f;
    public float moveSpeed = 12f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 90f;

    [Header("Scaling Settings")]
    public float scaleSpeed = 0.5f;
    public float minScaleMultiplier = 0.5f;
    public float maxScaleMultiplier = 2f;

    private Rigidbody heldObject;
    public Rigidbody HeldObject => heldObject;
    private bool originalGravity;
    private bool originalKinematic;
    private RigidbodyConstraints originalConstraints;
    private Vector3 initialScale;

    // Scale each object had the first time it was levitated. The scale
    // limits are relative to this, so regrabbing an object can't push it
    // past minScaleMultiplier / maxScaleMultiplier.
    private readonly Dictionary<Rigidbody, Vector3> originalScales =
        new Dictionary<Rigidbody, Vector3>();

    void Update()
    {
        if (levitateAction != null && levitateAction.action != null)
        {
            if (levitateAction.action.WasPressedThisFrame())
                TryLevitate();

            if (levitateAction.action.WasReleasedThisFrame())
                ReleaseObject();
        }

        if (heldObject != null)
            UpdateScale();
    }

    void FixedUpdate()
    {
        if (heldObject == null)
            return;

        Vector3 targetPosition =
            transform.position + transform.forward * holdDistance;

        Vector3 direction = targetPosition - heldObject.position;

        heldObject.linearVelocity = direction * moveSpeed;
        heldObject.angularVelocity = Vector3.zero;

        if (rotateAction != null &&
            rotateAction.action != null &&
            rotateAction.action.IsPressed())
        {
            Quaternion rotation = Quaternion.Euler(
                0f,
                rotationSpeed * Time.fixedDeltaTime,
                0f
            );

            heldObject.MoveRotation(heldObject.rotation * rotation);
        }
    }

    void UpdateScale()
    {
        bool enlarging = enlargeAction != null &&
                         enlargeAction.action != null &&
                         enlargeAction.action.IsPressed();

        bool shrinking = shrinkAction != null &&
                         shrinkAction.action != null &&
                         shrinkAction.action.IsPressed();

        if (enlarging == shrinking)
            return;

        float direction = enlarging ? 1f : -1f;

        float currentMultiplier =
            heldObject.transform.localScale.x / initialScale.x;

        float targetMultiplier = Mathf.Clamp(
            currentMultiplier + direction * scaleSpeed * Time.deltaTime,
            minScaleMultiplier,
            maxScaleMultiplier
        );

        // Preserve the object's proportions while changing its size.
        heldObject.transform.localScale =
            initialScale * targetMultiplier;
    }

    void TryLevitate()
    {
        if (heldObject != null)
            return;

        Ray ray = new Ray(transform.position, transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            return;

        Rigidbody rb = hit.rigidbody;

        if (rb == null || rb.isKinematic)
            return;

        if (!rb.CompareTag("Levitatable"))
            return;

        heldObject = rb;

        originalGravity = rb.useGravity;
        originalKinematic = rb.isKinematic;
        originalConstraints = rb.constraints;
        initialScale = GetOriginalScale(rb);

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Prevent collisions from causing uncontrolled rotation.
        rb.constraints |= RigidbodyConstraints.FreezeRotation;
    }

    Vector3 GetOriginalScale(Rigidbody rb)
    {
        // Forget objects that were destroyed (e.g. pumpkins delivered or
        // broken by goblins), so the dictionary doesn't keep growing.
        if (originalScales.Count > 32)
        {
            var destroyed = new List<Rigidbody>();
            foreach (Rigidbody key in originalScales.Keys)
            {
                if (key == null)
                    destroyed.Add(key);
            }
            foreach (Rigidbody key in destroyed)
                originalScales.Remove(key);
        }

        if (!originalScales.TryGetValue(rb, out Vector3 scale))
        {
            scale = rb.transform.localScale;
            originalScales[rb] = scale;
        }

        return scale;
    }

    void ReleaseObject()
    {
        if (heldObject == null)
            return;

        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;

        heldObject.useGravity = originalGravity;
        heldObject.isKinematic = originalKinematic;
        heldObject.constraints = originalConstraints;

        heldObject = null;
    }

    void OnDisable()
    {
        ReleaseObject();
    }
}
