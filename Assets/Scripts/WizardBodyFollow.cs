using UnityEngine;

// Keeps the first-person wizard body under the player's head and turns it
// towards where the head looks. Quick glances don't move the body: it
// starts turning once the head is turned past a threshold, like a real
// person would, and keeps turning until it faces the head again.
//
// With a head bone, the model's head is placed just behind the camera
// (whatever the model's own pivot offset is), the model is scaled to the
// player's height, and its head is hidden so the camera never sees the
// inside of it. The player sees the wizard's own body, hands and staff.
public class WizardBodyFollow : MonoBehaviour
{
    [Tooltip("The wizard body (child of the XR Origin).")]
    public Transform body;

    [Tooltip("The player's head. Defaults to the main camera.")]
    public Transform head;

    [Header("Turning")]
    [Tooltip("The body starts turning when the head is turned more than this (degrees).")]
    public float startTurnAngle = 30f;

    [Tooltip("The body stops turning when it is within this angle of the head (degrees).")]
    public float stopTurnAngle = 3f;

    [Tooltip("How fast the body turns (degrees per second).")]
    public float turnSpeed = 180f;

    [Header("Position")]
    [Tooltip("Without a head bone: horizontal distance of the body's pivot behind the head.")]
    public float backOffset = 0f;

    [Header("First-Person Body")]
    [Tooltip("Name of the model's head bone (searched under the body).")]
    public string headBoneName = "head";

    [Tooltip("Horizontal distance of the model's head bone behind the camera.")]
    public float headBehindCamera = 0.12f;

    [Tooltip("Shrink the head bone so the camera doesn't see the inside of the head.")]
    public bool hideHead = true;

    [Tooltip("Scale the model so its head matches the player's height.")]
    public bool matchPlayerHeight = true;

    [Tooltip("Height of the eyes above the head bone (meters).")]
    public float eyesAboveHeadBone = 0.1f;

    public float minBodyScale = 0.7f;
    public float maxBodyScale = 1.3f;

    private bool isTurning;
    private Transform headBone;
    private float headBoneRestHeight;
    private float tallestEyeHeight;

    void Start()
    {
        if (body == null)
            return;

        headBone = FindChild(body, headBoneName);

        if (headBone == null)
        {
            Debug.LogWarning(
                $"WizardBodyFollow: no '{headBoneName}' bone under {body.name}; " +
                "using the body pivot instead.", this);
            return;
        }

        // Head height at scale 1, measured in the starting pose.
        headBoneRestHeight =
            (headBone.position.y - body.position.y) / body.lossyScale.y;
    }

    // Update, not LateUpdate: the Animator (and the hand IK) runs after
    // Update, so it sees the body already in its new place.
    void Update()
    {
        if (body == null)
            return;

        if (head == null)
        {
            if (Camera.main == null)
                return;

            head = Camera.main.transform;
        }

        UpdateRotation();

        if (headBone == null)
        {
            // Stand under the head, at the body's current height.
            Vector3 position = head.position - body.forward * backOffset;
            position.y = body.position.y;
            body.position = position;
            return;
        }

        UpdateScale();

        // Move the body so its head bone ends up just behind the camera.
        Vector3 target = head.position - body.forward * headBehindCamera;
        Vector3 delta = target - headBone.position;
        delta.y = 0f;
        body.position += delta;
    }

    void LateUpdate()
    {
        // After the Animator has posed the bones, before rendering.
        if (hideHead && headBone != null)
            headBone.localScale = Vector3.one * 0.0001f;
    }

    void UpdateRotation()
    {
        Vector3 headForward =
            Vector3.ProjectOnPlane(head.forward, Vector3.up);

        // Looking straight up or down: no clear horizontal direction.
        if (headForward.sqrMagnitude < 0.0001f)
            return;

        float headYaw = Quaternion.LookRotation(headForward).eulerAngles.y;
        float bodyYaw = body.eulerAngles.y;
        float difference = Mathf.Abs(Mathf.DeltaAngle(bodyYaw, headYaw));

        if (difference > startTurnAngle)
            isTurning = true;
        else if (difference < stopTurnAngle)
            isTurning = false;

        if (isTurning)
        {
            bodyYaw = Mathf.MoveTowardsAngle(
                bodyYaw, headYaw, turnSpeed * Time.deltaTime
            );
        }

        body.rotation = Quaternion.Euler(0f, bodyYaw, 0f);
    }

    // Uses the tallest head height seen so far, so crouching or looking
    // at the floor doesn't shrink the wizard.
    void UpdateScale()
    {
        if (!matchPlayerHeight || headBoneRestHeight < 0.1f)
            return;

        float eyeHeight = head.position.y - body.position.y;
        if (eyeHeight <= tallestEyeHeight)
            return;

        tallestEyeHeight = eyeHeight;

        float scale = Mathf.Clamp(
            (tallestEyeHeight - eyesAboveHeadBone) / headBoneRestHeight,
            minBodyScale,
            maxBodyScale
        );

        body.localScale = Vector3.one * scale;
    }

    static Transform FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}
