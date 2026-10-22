
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

    private Rigidbody heldObject;
    private bool originalGravity;
    private bool originalKinematic;

    void Update()
    {
        if (levitateAction == null || levitateAction.action == null)
            return;

        if (levitateAction.action.WasPressedThisFrame())
            TryLevitate();

        if (levitateAction.action.WasReleasedThisFrame())
            ReleaseObject();
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
    }

    void TryLevitate()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Rigidbody rb = hit.rigidbody;

            if (rb == null)
                return;

            // Don't grab kinematic or unrelated objects.
            if (rb.isKinematic)
                return;

            // Only grab objects tagged as levitatable.
            if (!rb.CompareTag("Levitatable"))
                return;

            heldObject = rb;

            originalGravity = rb.useGravity;
            originalKinematic = rb.isKinematic;

            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void ReleaseObject()
    {
        if (heldObject == null)
            return;

        heldObject.useGravity = originalGravity;
        heldObject.isKinematic = originalKinematic;
        heldObject = null;
    }

    void OnDisable()
    {
        ReleaseObject();
    }
}
