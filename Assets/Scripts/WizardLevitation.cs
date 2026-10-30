
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardLevitation : MonoBehaviour
{
    [Header("Quest 3 Input")]
    public InputActionReference levitateAction;
    [Tooltip("Hold to rotate with the wrist")]
    public InputActionReference rotateAction;
    public InputActionReference enlargeAction;
    public InputActionReference shrinkAction;

    [Header("Levitation Settings")]
    public float maxDistance = 8f;
    [Tooltip("Hold distance when Go-Go is off")]
    public float holdDistance = 1.5f;
    public float moveSpeed = 12f;

    [Header("Go-Go push / pull")]
    [Tooltip("Go-Go: stretch the arm to push, pull back to bring closer")]
    public bool useGoGo = true;
    [Tooltip("Go-Go sensitivity (exponential)")]
    public float goGoGain = 3f;
    public float minHoldDistance = 0.4f;
    [Tooltip("Head (defaults to main camera)")]
    public Transform head;
    public float shoulderDrop = 0.25f;

    [Header("Hand tools")]
    [Tooltip("Distance in front of the hand for tools (lantern)")]
    public float toolHoldDistance = 0.3f;

    [Header("Weight")]
    [Tooltip("Bigger objects move slower")]
    public bool sizeAffectsSpeed = true;

    [Header("Scaling Settings")]
    public float scaleSpeed = 0.5f;
    public float minScaleMultiplier = 0.5f;
    public float maxScaleMultiplier = 2f;

    [Header("Jump")]
    [Tooltip("Jump provider, disabled while holding (A also scales). Auto if empty")]
    public Behaviour jumpProvider;

    private Rigidbody heldObject;
    public Rigidbody HeldObject => heldObject;
    private bool originalGravity;
    private bool originalKinematic;
    private RigidbodyConstraints originalConstraints;
    private Vector3 initialScale;

    // Wrist rotation: hand and object rotations when Y was pressed.
    private bool isRotating;
    private Quaternion handRotationAtClutch;
    private Quaternion objectRotationAtClutch;

    private bool jumpWasEnabled;

    // The held object is a hand tool (held like a flashlight).
    private bool holdingTool;
    public bool HoldingTool => heldObject != null && holdingTool;

    // Go-Go state, measured when the object is grabbed.
    private float grabDistance;
    private float grabArmExtension;
    private float currentHoldDistance;
    private float holdDistanceVelocity;

    // Current distance between the hand and the held object's anchor.
    public float CurrentHoldDistance => currentHoldDistance;

    // original scale of each object (for the scale limits)
    private readonly Dictionary<Rigidbody, Vector3> originalScales =
        new Dictionary<Rigidbody, Vector3>();

    void Start()
    {
        if (jumpProvider == null)
            jumpProvider = FindJumpProvider();

        if (head == null && Camera.main != null)
            head = Camera.main.transform;
    }

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

        if (holdingTool)
        {
            HoldTool();
            return;
        }

        UpdateHoldDistance();

        Vector3 targetPosition =
            transform.position + transform.forward * currentHoldDistance;

        Vector3 direction = targetPosition - heldObject.position;

        heldObject.linearVelocity = direction * FollowSpeed();
        heldObject.angularVelocity = Vector3.zero;

        UpdateRotation();
    }

    // tools (lantern) stay in front of the hand
    void HoldTool()
    {
        float distance = toolHoldDistance + 0.1f * (HeldSizeMultiplier - 1f);

        Vector3 targetPosition = transform.position + transform.forward * distance;
        heldObject.linearVelocity = (targetPosition - heldObject.position) * moveSpeed * 1.5f;
        heldObject.angularVelocity = Vector3.zero;
        heldObject.MoveRotation(transform.rotation);
    }

    // Go-Go: distance grows exponentially with arm extension
    void UpdateHoldDistance()
    {
        if (!useGoGo)
        {
            currentHoldDistance = holdDistance;
            return;
        }

        float stretch = ArmExtension() - grabArmExtension;

        float target = Mathf.Clamp(
            grabDistance * Mathf.Exp(goGoGain * stretch),
            minHoldDistance,
            maxDistance
        );

        // Smoothed, so small hand tremors don't shake a far object.
        currentHoldDistance = Mathf.SmoothDamp(
            currentHoldDistance, target, ref holdDistanceVelocity,
            0.08f, Mathf.Infinity, Time.fixedDeltaTime
        );
    }

    float ArmExtension()
    {
        if (head == null)
            return 0f;

        Vector3 shoulders = head.position - Vector3.up * shoulderDrop;
        return Vector3.Distance(transform.position, shoulders);
    }

    // Size multiplier of the held object (1 = its original size).
    public float HeldSizeMultiplier =>
        heldObject == null || initialScale.x <= 0f
            ? 1f
            : heldObject.transform.localScale.x / initialScale.x;

    float FollowSpeed()
    {
        if (!sizeAffectsSpeed)
            return moveSpeed;

        // 2x size -> half speed (heavy), 0.5x -> 1.5x speed (light).
        return moveSpeed * Mathf.Clamp(1f / HeldSizeMultiplier, 0.5f, 1.5f);
    }

    // while Y is held the object copies the wrist rotation
    void UpdateRotation()
    {
        bool pressed = rotateAction != null &&
                       rotateAction.action != null &&
                       rotateAction.action.IsPressed();

        if (!pressed)
        {
            isRotating = false;
            return;
        }

        if (!isRotating)
        {
            isRotating = true;
            handRotationAtClutch = transform.rotation;
            objectRotationAtClutch = heldObject.rotation;
        }

        Quaternion wristDelta =
            transform.rotation * Quaternion.Inverse(handRotationAtClutch);

        heldObject.MoveRotation(wristDelta * objectRotationAtClutch);
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

        // ignore triggers (delivery tomb)
        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
            return;

        Rigidbody rb = hit.rigidbody;

        if (rb == null || rb.isKinematic)
            return;

        if (!rb.CompareTag("Levitatable"))
            return;

        heldObject = rb;
        isRotating = false;
        holdingTool = rb.GetComponent<GhostLantern>() != null;

        originalGravity = rb.useGravity;
        originalKinematic = rb.isKinematic;
        originalConstraints = rb.constraints;
        initialScale = GetOriginalScale(rb);

        // keep the grab distance
        grabDistance = Mathf.Clamp(hit.distance, minHoldDistance, maxDistance);
        grabArmExtension = ArmExtension();
        currentHoldDistance = useGoGo ? grabDistance : holdDistance;
        holdDistanceVelocity = 0f;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Prevent collisions from causing uncontrolled rotation.
        rb.constraints |= RigidbodyConstraints.FreezeRotation;

        SetJumpEnabled(false);
    }

    Vector3 GetOriginalScale(Rigidbody rb)
    {
        // remove destroyed objects
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
            // pumpkins use their spawn scale
            PumpkinItem pumpkin = rb.GetComponentInParent<PumpkinItem>();
            scale = pumpkin != null ? pumpkin.BaseScale : rb.transform.localScale;
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
        isRotating = false;

        SetJumpEnabled(true);
    }

    // Off: remember whether jumping was on. On: restore that state.
    void SetJumpEnabled(bool enable)
    {
        if (jumpProvider == null)
            return;

        if (!enable)
        {
            jumpWasEnabled = jumpProvider.enabled;
            jumpProvider.enabled = false;
        }
        else if (jumpWasEnabled)
        {
            jumpProvider.enabled = true;
        }
    }

    // find the jump provider by name inside the rig
    Behaviour FindJumpProvider()
    {
        foreach (MonoBehaviour behaviour in
                 transform.root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour != null &&
                behaviour.GetType().Name == "JumpProvider")
                return behaviour;
        }

        Debug.LogWarning("WizardLevitation: no JumpProvider found in the XR rig.", this);
        return null;
    }

    void OnDisable()
    {
        ReleaseObject();
    }
}
