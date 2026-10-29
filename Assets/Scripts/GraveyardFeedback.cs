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

    public void PumpkinDelivered(Vector3 position)
    {
        MagicFeedback.Play(MagicFeedback.ChimeSound, position, 1f);
        SpawnFlash(position, 1.2f, deliveredColor, 0.4f);
        PulseBoth(0.4f, 0.15f);
    }

    public void GoblinAppeared(Vector3 position)
    {
        MagicFeedback.Play(MagicFeedback.GrowlSound, position, 1f);
    }

    static void SpawnFlash(Vector3 position, float size, Color color, float duration)
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "Feedback Flash";
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * size;

        // Visual only.
        Destroy(flash.GetComponent<Collider>());

        flash.GetComponent<Renderer>().material.color = color;
        Destroy(flash, duration);
    }

    void PulseBoth(float amplitude, float duration)
    {
        MagicFeedback.Pulse(leftHaptics, amplitude, duration);
        MagicFeedback.Pulse(rightHaptics, amplitude, duration);
    }
}
