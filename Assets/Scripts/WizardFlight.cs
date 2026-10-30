using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

// Magic flight (custom navigation): hold the right grip to fly where the
// right hand points; squeezing harder flies faster. The XR rig's character
// controller still collides with walls and the ground. Gravity is
// switched off while flying and comes back when the grip is released.
public class WizardFlight : LocomotionProvider, IGravityController
{
    [Header("Flight")]
    [Tooltip("The player flies towards this transform's forward (the right controller).")]
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

    [Header("Comfort")]
    [Tooltip("Darkens the edges of the view while flying, which reduces " +
             "motion sickness. Can be switched in the grimoire.")]
    [SerializeField] private bool comfortVignette = true;

    [Tooltip("The rig's tunneling vignette. Found automatically if empty.")]
    public TunnelingVignetteController vignette;

    private LocomotionVignetteProvider vignetteProvider;

    public bool ComfortVignette
    {
        get => comfortVignette;
        set
        {
            comfortVignette = value;
            if (vignetteProvider != null)
                vignetteProvider.enabled = value;
        }
    }

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

    // Registers the flight with the rig's tunneling vignette, so the
    // vignette also reacts to flying (the template only lists its own
    // move, turn and teleport providers).
    protected void Start()
    {
        if (vignette == null)
            vignette = FindAnyObjectByType<TunnelingVignetteController>();

        if (vignette == null)
        {
            Debug.LogWarning("WizardFlight: no TunnelingVignetteController found.", this);
            return;
        }

        vignetteProvider = new LocomotionVignetteProvider
        {
            locomotionProvider = this,
            enabled = comfortVignette,
        };
        vignette.locomotionVignetteProviders.Add(vignetteProvider);
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
