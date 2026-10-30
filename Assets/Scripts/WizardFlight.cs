using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

// Flight: hold right grip, fly where the hand points
public class WizardFlight : LocomotionProvider, IGravityController
{
    [Header("Flight")]
    [Tooltip("Fly direction (right controller)")]
    public Transform steeringHand;

    public GravityProvider gravityProvider;

    [Tooltip("Speed in m/s with the grip fully pressed.")]
    public float maxSpeed = 4f;

    [Tooltip("Grip values below this do not fly.")]
    public float deadZone = 0.15f;

    [Header("Input")]
    [Tooltip("Input System binding path of the analog flight throttle.")]
    public string throttleBinding = "<XRController>{RightHand}/{Grip}";

    [Header("Feedback")]
    [Tooltip("Haptics of the steering controller.")]
    public HapticImpulsePlayer haptics;
    public float pulseInterval = 0.2f;

    private InputAction throttleAction;
    private readonly XROriginMovement movement = new XROriginMovement();
    private bool isFlying;
    private float nextPulseTime;

    public bool IsFlying => isFlying;

    // ---------- IGravityController ----------

    public bool canProcess => isActiveAndEnabled;

    public bool gravityPaused => isFlying;

    public bool TryLockGravity(GravityOverride gravityOverride)
    {
        return gravityProvider != null &&
            gravityProvider.TryLockGravity(this, gravityOverride);
    }

    public void RemoveGravityLock()
    {
        if (gravityProvider != null)
            gravityProvider.UnlockGravity(this);
    }

    public void OnGravityLockChanged(GravityOverride gravityOverride)
    {
    }

    public void OnGroundedChanged(bool isGrounded)
    {
    }

    // ---------- Flight ----------

    protected override void Awake()
    {
        base.Awake();

        throttleAction = new InputAction(
            "Flight Throttle", InputActionType.Value, throttleBinding
        );
    }

    // no vignette, turn off the template one
    protected void Start()
    {
        TunnelingVignetteController vignette =
            FindAnyObjectByType<TunnelingVignetteController>();

        if (vignette != null)
            vignette.gameObject.SetActive(false);
    }

    protected void OnEnable()
    {
        throttleAction.Enable();
    }

    protected void OnDisable()
    {
        throttleAction.Disable();
        StopFlying();
    }

    protected void OnDestroy()
    {
        throttleAction.Dispose();
    }

    protected void Update()
    {
        float grip = throttleAction.ReadValue<float>();

        if (grip < deadZone || steeringHand == null)
        {
            StopFlying();
            return;
        }

        if (!isFlying)
            StartFlying();

        if (!isLocomotionActive)
            return;

        // Light squeeze: slow. Full squeeze: maxSpeed.
        float speed = maxSpeed * Mathf.InverseLerp(deadZone, 1f, grip);

        movement.motion = steeringHand.forward * (speed * Time.deltaTime);
        TryQueueTransformation(movement);

        // A soft rumble that grows with speed.
        if (haptics != null && Time.time >= nextPulseTime)
        {
            haptics.SendHapticImpulse(0.05f + 0.15f * speed / maxSpeed, 0.1f);
            nextPulseTime = Time.time + pulseInterval;
        }
    }

    void StartFlying()
    {
        if (!TryStartLocomotionImmediately())
            return;

        isFlying = true;
        TryLockGravity(GravityOverride.ForcedOff);
    }

    void StopFlying()
    {
        if (!isFlying)
            return;

        isFlying = false;
        RemoveGravityLock();

        if (isLocomotionActive)
            TryEndLocomotion();
    }
}
