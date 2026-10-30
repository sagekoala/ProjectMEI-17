
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

// Puts the eyes at a fixed height when the game starts and after a recenter
public class EyeHeight : MonoBehaviour
{
    public float eyeHeight = 1.36f;

    private XROrigin origin;
    private readonly List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();

    IEnumerator Start()
    {
        origin = FindAnyObjectByType<XROrigin>();

        SubsystemManager.GetSubsystems(subsystems);
        foreach (XRInputSubsystem subsystem in subsystems)
            subsystem.trackingOriginUpdated += OnRecenter;

        // wait for a real head pose, then again in case the headset was put on late
        yield return new WaitForSeconds(0.5f);
        Apply();
        yield return new WaitForSeconds(2f);
        Apply();
    }

    void OnDestroy()
    {
        foreach (XRInputSubsystem subsystem in subsystems)
        {
            if (subsystem != null)
                subsystem.trackingOriginUpdated -= OnRecenter;
        }
    }

    void OnRecenter(XRInputSubsystem subsystem)
    {
        Apply();
    }

    public void Apply()
    {
        if (origin == null || origin.Camera == null || origin.CameraFloorOffsetObject == null)
            return;

        Transform offset = origin.CameraFloorOffsetObject.transform;
        Vector3 position = offset.localPosition;
        position.y = eyeHeight - origin.Camera.transform.localPosition.y;
        offset.localPosition = position;
    }
}
