using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

// Locomotion options used by the grimoire
public class WizardLocomotionSettings : MonoBehaviour
{
    [Header("XR Rig Controllers")]
    [Tooltip("Left controller. Its thumbstick either walks or teleports.")]
    public ControllerInputActionManager leftHandInput;

    [Tooltip("Right controller. Its thumbstick turns and teleports.")]
    public ControllerInputActionManager rightHandInput;

    [Header("XR Rig Providers")]
    public ContinuousMoveProvider moveProvider;
    public SnapTurnProvider snapTurnProvider;
    public ContinuousTurnProvider continuousTurnProvider;

    [Header("Walk Speed (m/s)")]
    public float minMoveSpeed = 0.5f;
    public float maxMoveSpeed = 6f;
    public float moveSpeedStep = 0.5f;

    [Header("Smooth Turn Speed (deg/s)")]
    public float minTurnSpeed = 30f;
    public float maxTurnSpeed = 180f;
    public float turnSpeedStep = 15f;

    [Header("Snap Turn Angle (deg)")]
    public float minSnapAngle = 15f;
    public float maxSnapAngle = 90f;
    public float snapAngleStep = 15f;

    // Raised after any setting changes, so UIs can refresh.
    public event Action Changed;

    public bool IsConfigured { get; private set; }

    public bool IsWalkEnabled =>
        IsConfigured && leftHandInput.smoothMotionEnabled;

    public bool IsSmoothTurnEnabled =>
        IsConfigured && rightHandInput.smoothTurnEnabled;

    public float MoveSpeed =>
        IsConfigured ? moveProvider.moveSpeed : 0f;

    public float TurnSpeed =>
        IsConfigured ? continuousTurnProvider.turnSpeed : 0f;

    public float SnapAngle =>
        IsConfigured ? snapTurnProvider.turnAmount : 0f;

    void Awake()
    {
        IsConfigured =
            leftHandInput != null &&
            rightHandInput != null &&
            moveProvider != null &&
            snapTurnProvider != null &&
            continuousTurnProvider != null;

        if (!IsConfigured)
        {
            Debug.LogError(
                "WizardLocomotionSettings is missing XR rig references; " +
                "locomotion options are disabled.", this);
        }
    }

    void Start()
    {
        if (!IsConfigured)
            return;

        // right hand controls turning, sync the rest
        SetSmoothTurn(rightHandInput.smoothTurnEnabled);
    }

    // true: left thumbstick walks. false: left thumbstick teleports.
    public void SetWalkEnabled(bool walk)
    {
        if (!IsConfigured)
            return;

        leftHandInput.smoothMotionEnabled = walk;
        Changed?.Invoke();
    }

    // true: smooth turning. false: snap turning.
    public void SetSmoothTurn(bool smooth)
    {
        if (!IsConfigured)
            return;

        leftHandInput.smoothTurnEnabled = smooth;
        rightHandInput.smoothTurnEnabled = smooth;

        // Only the active turn provider should be enabled.
        continuousTurnProvider.enabled = smooth;
        snapTurnProvider.enabled = !smooth;

        Changed?.Invoke();
    }

    // direction: +1 faster, -1 slower.
    public void ChangeMoveSpeed(int direction)
    {
        if (!IsConfigured)
            return;

        moveProvider.moveSpeed = Mathf.Clamp(
            moveProvider.moveSpeed + direction * moveSpeedStep,
            minMoveSpeed,
            maxMoveSpeed
        );

        Changed?.Invoke();
    }

    // direction: +1 more, -1 less
    public void ChangeTurn(int direction)
    {
        if (!IsConfigured)
            return;

        if (IsSmoothTurnEnabled)
        {
            continuousTurnProvider.turnSpeed = Mathf.Clamp(
                continuousTurnProvider.turnSpeed + direction * turnSpeedStep,
                minTurnSpeed,
                maxTurnSpeed
            );
        }
        else
        {
            snapTurnProvider.turnAmount = Mathf.Clamp(
                snapTurnProvider.turnAmount + direction * snapAngleStep,
                minSnapAngle,
                maxSnapAngle
            );
        }

        Changed?.Invoke();
    }
}
