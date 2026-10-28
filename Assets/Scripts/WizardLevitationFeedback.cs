using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

// Visual feedback for levitation: a beam from the hand and a color tint
// on the object that is aimed at (purple) or levitated (gold).
[RequireComponent(typeof(WizardLevitation))]
public class WizardLevitationFeedback : MonoBehaviour
{
    [Header("Beam")]
    [Tooltip("Material for the beam. It should use vertex colors.")]
    public Material beamMaterial;
    public float beamWidth = 0.01f;
    public Color aimBeamColor = new Color(0.7f, 0.4f, 1f);
    public Color holdBeamColor = new Color(1f, 0.85f, 0.3f);

    [Header("Object Tint")]
    public Color aimTint = new Color(0.75f, 0.55f, 1f);
    public Color holdTint = new Color(1f, 0.8f, 0.35f);

    [Header("Haptics")]
    [Tooltip("Haptics of the levitating controller.")]
    public HapticImpulsePlayer haptics;

    [Tooltip("Seconds between the soft pulses felt while levitating.")]
    public float holdPulseInterval = 0.25f;

    // URP Lit uses _BaseColor; built-in shaders use _Color.
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    private WizardLevitation levitation;
    private LineRenderer beam;
    private MaterialPropertyBlock propertyBlock;

    private Rigidbody highlightedBody;
    private Renderer[] highlightedRenderers;

    private Rigidbody lastHeld;
    private Rigidbody lastAimed;
    private float nextHoldPulseTime;

    void Awake()
    {
        levitation = GetComponent<WizardLevitation>();
        propertyBlock = new MaterialPropertyBlock();

        if (beamMaterial == null)
        {
            Debug.LogWarning(
                "WizardLevitationFeedback has no beam material; " +
                "only the object tint will be shown.", this);
            return;
        }

        // On a child: a GameObject can only have one Renderer, and the
        // hand may already have a MeshRenderer.
        GameObject beamObject = new GameObject("Levitation Beam");
        beamObject.transform.SetParent(transform, false);

        beam = beamObject.AddComponent<LineRenderer>();
        beam.sharedMaterial = beamMaterial;
        beam.positionCount = 2;
        beam.useWorldSpace = true;
        beam.widthMultiplier = beamWidth;
        beam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beam.receiveShadows = false;
        beam.enabled = false;
    }

    void OnDisable()
    {
        HideAll();
    }

    void LateUpdate()
    {
        // Paused (e.g. while the control menu is open).
        if (!levitation.isActiveAndEnabled)
        {
            HideAll();
            return;
        }

        Rigidbody held = levitation.HeldObject;

        UpdateHoldFeedback(held);

        if (held != null)
        {
            ShowBeam(held.position, holdBeamColor);
            SetHighlight(held, holdTint);
            return;
        }

        Rigidbody aimed = levitation.FindLevitatable(out Vector3 hitPoint);

        if (aimed != null && aimed != lastAimed)
        {
            // A small tick when a new object comes under the beam.
            MagicFeedback.Pulse(haptics, 0.1f, 0.03f);
        }
        lastAimed = aimed;

        if (aimed != null)
        {
            ShowBeam(hitPoint, aimBeamColor);
            SetHighlight(aimed, aimTint);
            return;
        }

        HideAll();
    }

    // Sound and vibration when grabbing/releasing, and a soft
    // continuous pulse while an object is levitated.
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

    void HideAll()
    {
        if (beam != null)
            beam.enabled = false;

        ClearHighlight();
    }

    void ShowBeam(Vector3 end, Color color)
    {
        if (beam == null)
            return;

        beam.enabled = true;
        beam.SetPosition(0, transform.position);
        beam.SetPosition(1, end);
        beam.startColor = color;
        beam.endColor = color;
    }

    void SetHighlight(Rigidbody body, Color tint)
    {
        if (body != highlightedBody)
        {
            ClearHighlight();

            highlightedBody = body;
            highlightedRenderers = body != null
                ? body.GetComponentsInChildren<Renderer>()
                : null;
        }

        if (highlightedRenderers == null)
            return;

        foreach (Renderer renderer in highlightedRenderers)
        {
            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, tint);
            propertyBlock.SetColor(ColorId, tint);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    void ClearHighlight()
    {
        if (highlightedRenderers != null)
        {
            foreach (Renderer renderer in highlightedRenderers)
            {
                // The object may have been destroyed meanwhile.
                if (renderer != null)
                    renderer.SetPropertyBlock(null);
            }
        }

        highlightedBody = null;
        highlightedRenderers = null;
    }
}
