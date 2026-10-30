
using Unity.XR.CoreUtils;
using UnityEngine;

// Keeps the player inside the graveyard walls
public class PlayAreaBounds : MonoBehaviour
{
    [Header("Play Area (world space)")]
    public Vector3 center = new Vector3(3.34f, 0f, -12.62f);
    [Tooltip("Size X/Z (Y not used)")]
    public Vector3 size = new Vector3(12.59f, 0f, 15.53f);

    [Tooltip("Distance from the walls")]
    public float wallMargin = 0.4f;

    [Tooltip("Max head height")]
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
