using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

// Game-wide feedback for events that happen away from the player's hands:
// 3D sounds (so the player hears where things happen), vibration on both
// controllers for the important ones, and a looping background wind.
// Gameplay scripts call it through Instance, like GraveyardGameManager.
public class GraveyardFeedback : MonoBehaviour
{
    public static GraveyardFeedback Instance { get; private set; }

    [Header("Controllers")]
    public HapticImpulsePlayer leftHaptics;
    public HapticImpulsePlayer rightHaptics;

    [Header("Ambience")]
    [Range(0f, 1f)]
    public float windVolume = 0.25f;

    [Header("Effects")]
    public Color deliveredColor = new Color(1f, 0.8f, 0.3f);
    public Color lostColor = new Color(1f, 0.25f, 0.2f);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AudioSource wind = gameObject.AddComponent<AudioSource>();
        wind.clip = MagicFeedback.WindLoop;
        wind.loop = true;
        wind.volume = windVolume;
        wind.spatialBlend = 0f;
        wind.Play();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // A goblin destroyed a pumpkin: the player must notice, even if it
    // happened behind them.
    public void PumpkinLost(Vector3 position)
    {
        MagicFeedback.Play(MagicFeedback.FailSound, position, 1f);
        SpawnFlash(position, 0.8f, lostColor, 0.3f);
        PulseBoth(0.6f, 0.3f);
    }

    // A ghost appeared: a whoosh is played where it is; a light tick on
    // both hands so the wizard looks around.
    public void GhostAppeared(Vector3 position)
    {
        PulseBoth(0.2f, 0.1f);
    }

    // The ghost caught the wizard: strong on both hands, and a pale flash.
    public void PlayerCaught(Vector3 position)
    {
        MagicFeedback.Play(MagicFeedback.FailSound, position, 1f);
        SpawnFlash(position, 0.6f, new Color(0.7f, 0.95f, 1f), 0.25f);
        PulseBoth(0.9f, 0.4f);
    }

    public void PumpkinDelivered(Vector3 position)
    {
        MagicFeedback.Play(MagicFeedback.ChimeSound, position, 1f);
        SpawnFlash(position, 1.2f, deliveredColor, 0.4f);
        PulseBoth(0.4f, 0.15f);
    }

    // The final wave begins: a horn, a strong vibration and a sign in
    // front of the player for a few seconds.
    public void AnnounceFinalWave()
    {
        Camera head = Camera.main;
        if (head == null)
            return;

        MagicFeedback.Play(MagicFeedback.HornSound, head.transform.position, 1f);
        PulseBoth(0.8f, 0.6f);
        ShowSign(head.transform, "¡OLEADA FINAL!\n<size=60%>¡Llegan los arqueros!</size>", lostColor, 3.5f);
    }

    public void GoblinAppeared(Vector3 position)
    {
        MagicFeedback.Play(MagicFeedback.GrowlSound, position, 1f);
    }

    static void SpawnFlash(Vector3 position, float size, Color color, float duration)
    {
        GameObject flash = GlowSphere.Create("Feedback Flash", position, size, color);
        Destroy(flash, duration);
    }

    // A big text floating in front of the player, fixed in the world.
    static void ShowSign(Transform head, string text, Color color, float duration)
    {
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;
        forward.Normalize();

        GameObject sign = new GameObject("Announcement", typeof(RectTransform));
        sign.transform.position = head.position + forward * 2.5f + Vector3.up * 0.3f;
        sign.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        sign.transform.localScale = Vector3.one * 0.002f;

        Canvas canvas = sign.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        ((RectTransform)sign.transform).sizeDelta = new Vector2(1000f, 300f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(sign.transform, false);

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        if (HalloweenFontTheme.Current != null)
            label.font = HalloweenFontTheme.Current;
        label.text = text;
        label.fontSize = 110f;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        RectTransform rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Destroy(sign, duration);
    }

    void PulseBoth(float amplitude, float duration)
    {
        MagicFeedback.Pulse(leftHaptics, amplitude, duration);
        MagicFeedback.Pulse(rightHaptics, amplitude, duration);
    }
}
