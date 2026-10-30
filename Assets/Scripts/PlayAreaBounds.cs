
using Unity.XR.CoreUtils;
using UnityEngine;

// Keeps the player inside the cemetery walls. Every frame, after
// walking, teleporting or flying, if the head is outside the box the
// whole XR rig is pushed back in. It works like an invisible wall that
// also stops you from flying over the real walls, and a ceiling so you
// can't fly off into the sky.
// The default box matches the walls of Colonial_Graveyard; select the
// object in the editor to see it (yellow) and adjust it.
public class PlayAreaBounds : MonoBehaviour
{
    [Header("Play Area (world space)")]
    public Vector3 center = new Vector3(3.34f, 0f, -12.62f);
    [Tooltip("Width (X) and depth (Z) of the area between the walls. Y is ignored.")]
    public Vector3 size = new Vector3(12.59f, 0f, 15.53f);

    [Tooltip("How close the head can get to the walls, in meters.")]
    public float wallMargin = 0.4f;

    [Tooltip("Highest the head can go (world Y), so flying has a ceiling.")]
    public float maxHeadHeight = 7f;

    [Header("Rig (found automatically if empty)")]
    public XROrigin xrOrigin;

    void Start()
    {
        if (xrOrigin == null)
            xrOrigin = FindAnyObjectByType<XROrigin>();
    }

    void LateUpdate()
    {
        if (xrOrigin == null || xrOrigin.Camera == null)
            return;

        Vector3 head = xrOrigin.Camera.transform.position;

        Vector3 half = size * 0.5f;
        float minX = center.x - half.x + wallMargin;
        float maxX = center.x + half.x - wallMargin;
        float minZ = center.z - half.z + wallMargin;
        float maxZ = center.z + half.z - wallMargin;

        Vector3 push = Vector3.zero;

        if (head.x < minX) push.x = minX - head.x;
        else if (head.x > maxX) push.x = maxX - head.x;

        if (head.z < minZ) push.z = minZ - head.z;
        else if (head.z > maxZ) push.z = maxZ - head.z;

        if (head.y > maxHeadHeight) push.y = maxHeadHeight - head.y;

        if (push == Vector3.zero)
            return;

        xrOrigin.transform.position += push;

        // Let the character controller know the rig moved.
        Physics.SyncTransforms();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 boxCenter = new Vector3(center.x, maxHeadHeight * 0.5f, center.z);
        Gizmos.DrawWireCube(boxCenter, new Vector3(size.x, maxHeadHeight, size.z));

        Gizmos.color = new Color(1f, 0.6f, 0f);
        Gizmos.DrawWireCube(
            boxCenter,
            new Vector3(size.x - 2f * wallMargin, maxHeadHeight, size.z - 2f * wallMargin)
        );
    }
}
