
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[RequireComponent(typeof(LineRenderer))]
public class WizardLevitationFeedback : MonoBehaviour
{
    [Header("Levitation")]
    public WizardLevitation levitation;

    [Header("Ray Settings")]
    public float maxDistance = 4f;
    public float rayWidth = 0.012f;

    [Header("Colors")]
    public Color targetColor = Color.cyan;
    public Color holdingColor = new Color(1f, 0.75f, 0.1f);

    [Header("Object Tint")]
    [Tooltip("How strongly the aimed or levitated object is tinted (0-1).")]
    [Range(0f, 1f)]
    public float tintStrength = 0.5f;

    [Header("Haptics")]
    [Tooltip("Haptics of the levitating controller.")]
    public HapticImpulsePlayer haptics;

    [Tooltip("Seconds between the soft pulses felt while levitating.")]
    public float holdPulseInterval = 0.25f;

    // URP Lit uses _BaseColor; built-in shaders use _Color.
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    private LineRenderer ray;
    private MaterialPropertyBlock propertyBlock;
    private Rigidbody tintedBody;
    private Renderer[] tintedRenderers;
    private Rigidbody lastHeld;
    private Rigidbody lastAimed;
    private float nextHoldPulseTime;

    void Awake()
    {
        ray = GetComponent<LineRenderer>();
        ray.positionCount = 2;
        ray.startWidth = rayWidth;
        ray.endWidth = rayWidth;
        ray.useWorldSpace = true;
        ray.enabled = false;
        propertyBlock = new MaterialPropertyBlock();
    }

    void OnDisable()
    {
        ray.enabled = false;
        SetTint(null, Color.white);
    }

    void LateUpdate()
    {
        // missing or paused (menu open)
        if (levitation == null || !levitation.isActiveAndEnabled)
        {
            ray.enabled = false;
            SetTint(null, Color.white);
            return;
        }

        Rigidbody held = levitation.HeldObject;
        UpdateHoldFeedback(held);

        if (held != null)
        {
            DrawRay(held.worldCenterOfMass, holdingColor);
            SetTint(held, holdingColor);
            return;
        }

        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out RaycastHit hit,
            maxDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            Rigidbody target = hit.rigidbody;

            if (target != null &&
                !target.isKinematic &&
                target.CompareTag("Levitatable"))
            {
                // A small tick when a new object comes under the ray.
                if (target != lastAimed)
                    MagicFeedback.Pulse(haptics, 0.1f, 0.03f);

                lastAimed = target;
                DrawRay(hit.point, targetColor);
                SetTint(target, targetColor);
                return;
            }
        }

        lastAimed = null;
        ray.enabled = false;
        SetTint(null, Color.white);
    }

    // grab / release / hold feedback
    void UpdateHoldFeedback(Rigidbody held)
    {
        if (held != lastHeld)
        {
            if (held != null)
            {
                MagicFeedback.Play(MagicFeedback.GrabSound, held.position, 0.6f);
                MagicFeedback.Pulse(haptics, 0.4f, 0.1f);
            }
            else if (lastHeld != null)
            {
                MagicFeedback.Play(MagicFeedback.ReleaseSound, lastHeld.position, 0.5f);
                MagicFeedback.Pulse(haptics, 0.2f, 0.05f);
            }

            lastHeld = held;
            nextHoldPulseTime = Time.time + holdPulseInterval;
        }

        if (held != null && Time.time >= nextHoldPulseTime)
        {
            MagicFeedback.Pulse(haptics, 0.06f, 0.05f);
            nextHoldPulseTime = Time.time + holdPulseInterval;
        }
    }

    // Tints the aimed or held object; null clears the previous one.
    void SetTint(Rigidbody body, Color color)
    {
        if (body != tintedBody)
        {
            if (tintedRenderers != null)
            {
                foreach (Renderer renderer in tintedRenderers)
                {
                    // The object may have been destroyed meanwhile.
                    if (renderer != null)
                        renderer.SetPropertyBlock(null);
                }
            }

            tintedBody = body;
            tintedRenderers = body != null
                ? body.GetComponentsInChildren<Renderer>()
                : null;
        }

        if (tintedRenderers == null)
            return;

        Color tint = Color.Lerp(Color.white, color, tintStrength);

        foreach (Renderer renderer in tintedRenderers)
        {
            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, tint);
            propertyBlock.SetColor(ColorId, tint);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }


    void DrawRay(Vector3 endPoint, Color color)
    {
        ray.enabled = true;
        ray.SetPosition(0, transform.position);
        ray.SetPosition(1, endPoint);

        ray.startColor = color;
        ray.endColor = color;

        // Set the URP material color so it matches the targeting state.
        ray.material.SetColor("_BaseColor", color);
    }

}
