using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

// Shared sound and haptic feedback for the wizard's actions.
// The project has no suitable magic sound assets, so the sounds are
// synthesized once in code and cached.
public static class MagicFeedback
{
    const int SampleRate = 44100;

    static AudioClip castSound;
    static AudioClip impactSound;
    static AudioClip chimeSound;
    static AudioClip whooshSound;
    static AudioClip failSound;
    static AudioClip grabSound;
    static AudioClip releaseSound;
    static AudioClip clickSound;
    static AudioClip hoverSound;

    // Rising tone.
    public static AudioClip CastSound =>
        castSound != null ? castSound :
        (castSound = Sweep("Spell Cast", 0.25f, 500f, 1400f, 0.35f, 12f));

    // Short noisy thump.
    public static AudioClip ImpactSound =>
        impactSound != null ? impactSound :
        (impactSound = Noise("Spell Impact", 0.3f, 0.5f, 18f, 0.25f, 1));

    // Bright three-note arpeggio.
    public static AudioClip ChimeSound =>
        chimeSound != null ? chimeSound :
        (chimeSound = Chime("Magic Chime", new[] { 880f, 1108.7f, 1318.5f }, 0.07f, 0.8f));

    // Airy rush that fades in and out.
    public static AudioClip WhooshSound =>
        whooshSound != null ? whooshSound :
        (whooshSound = Whoosh("Whoosh", 0.4f, 0.3f));

    // Low falling buzz.
    public static AudioClip FailSound =>
        failSound != null ? failSound :
        (failSound = Sweep("Fail", 0.3f, 220f, 110f, 0.3f, 8f));

    public static AudioClip GrabSound =>
        grabSound != null ? grabSound :
        (grabSound = Sweep("Levitate", 0.2f, 300f, 700f, 0.25f, 10f));

    public static AudioClip ReleaseSound =>
        releaseSound != null ? releaseSound :
        (releaseSound = Sweep("Release", 0.2f, 700f, 300f, 0.2f, 10f));

    public static AudioClip ClickSound =>
        clickSound != null ? clickSound :
        (clickSound = Sweep("Click", 0.06f, 1200f, 1000f, 0.25f, 60f));

    public static AudioClip HoverSound =>
        hoverSound != null ? hoverSound :
        (hoverSound = Sweep("Hover", 0.03f, 2000f, 2000f, 0.1f, 120f));

    // Plays a 3D sound at a world position.
    public static void Play(AudioClip clip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    // Vibrates a controller. Does nothing if there is no haptic player.
    public static void Pulse(
        HapticImpulsePlayer haptics, float amplitude, float duration)
    {
        if (haptics != null)
            haptics.SendHapticImpulse(amplitude, duration);
    }

    // ---------- Synthesis ----------

    static AudioClip Create(
        string name, float seconds, Func<float, float> sample)
    {
        int count = Mathf.CeilToInt(seconds * SampleRate);
        float[] data = new float[count];

        // Samples are generated in order, so generators may keep state.
        for (int i = 0; i < count; i++)
            data[i] = Mathf.Clamp(sample((float)i / SampleRate), -1f, 1f);

        AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Fast attack, then exponential decay. Avoids clicks at the start.
    static float Envelope(float time, float decay)
    {
        const float attack = 0.005f;
        float rise = Mathf.Clamp01(time / attack);
        return rise * Mathf.Exp(-time * decay);
    }

    static AudioClip Sweep(
        string name, float seconds, float fromHz, float toHz,
        float volume, float decay)
    {
        float phase = 0f;

        return Create(name, seconds, time =>
        {
            float hz = Mathf.Lerp(fromHz, toHz, time / seconds);
            phase += 2f * Mathf.PI * hz / SampleRate;
            return volume * Envelope(time, decay) * Mathf.Sin(phase);
        });
    }

    static AudioClip Noise(
        string name, float seconds, float volume, float decay,
        float smoothing, int seed)
    {
        // Fixed seed: the same sound every run.
        System.Random random = new System.Random(seed);
        float previous = 0f;

        return Create(name, seconds, time =>
        {
            float white = (float)(random.NextDouble() * 2.0 - 1.0);

            // Simple low-pass filter for a duller, "thump" sound.
            previous = Mathf.Lerp(previous, white, smoothing);
            return volume * Envelope(time, decay) * previous;
        });
    }

    static AudioClip Whoosh(string name, float seconds, float volume)
    {
        System.Random random = new System.Random(2);
        float previous = 0f;

        return Create(name, seconds, time =>
        {
            float white = (float)(random.NextDouble() * 2.0 - 1.0);
            previous = Mathf.Lerp(previous, white, 0.1f);

            // Swells in the middle of the clip.
            float shape = Mathf.Sin(Mathf.PI * time / seconds);
            return volume * shape * previous * 3f;
        });
    }

    static AudioClip Chime(
        string name, float[] notesHz, float noteGap, float volume)
    {
        float seconds = noteGap * notesHz.Length + 0.6f;

        return Create(name, seconds, time =>
        {
            float value = 0f;

            for (int i = 0; i < notesHz.Length; i++)
            {
                float noteTime = time - i * noteGap;
                if (noteTime < 0f)
                    continue;

                value += Envelope(noteTime, 6f) *
                    Mathf.Sin(2f * Mathf.PI * notesHz[i] * noteTime);
            }

            return volume * value / notesHz.Length;
        });
    }
}
