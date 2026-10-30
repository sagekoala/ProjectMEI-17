using UnityEngine;

// Body follows the head, turns after a threshold
public class WizardBodyFollow : MonoBehaviour
{
    [Tooltip("The wizard body (child of the XR Origin).")]
    public Transform body;

    [Tooltip("The player's head. Defaults to the main camera.")]
    public Transform head;

    [Header("Turning")]
    [Tooltip("Start turning angle (degrees)")]
    public float startTurnAngle = 30f;

    [Tooltip("Stop turning angle (degrees)")]
    public float stopTurnAngle = 3f;

    [Tooltip("How fast the body turns (degrees per second).")]
    public float turnSpeed = 180f;

    [Header("Position")]
    [Tooltip("Horizontal distance of the body's pivot behind the head.")]
    public float backOffset = 0f;

    private bool isTurning;

    // Update so the animator/IK sees the new position
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

        // Stand under the head, at the body's current height.
        Vector3 position = head.position - body.forward * backOffset;
        position.y = body.position.y;
        body.position = position;
    }
}
