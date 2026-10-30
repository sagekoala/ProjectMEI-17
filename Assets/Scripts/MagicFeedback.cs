using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

// Sounds (generated in code) and haptics
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
    static AudioClip explosionSound;
    static AudioClip growlSound;
    static AudioClip hitSound;
    static AudioClip windLoop;
    static AudioClip hornSound;

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

    // Long, low rumble with a sharp start.
    public static AudioClip ExplosionSound =>
        explosionSound != null ? explosionSound :
        (explosionSound = Noise("Explosion", 0.9f, 0.9f, 5f, 0.08f, 3));

    // Low, rough falling tone: an enemy appears.
    public static AudioClip GrowlSound =>
        growlSound != null ? growlSound :
        (growlSound = Growl("Growl", 0.7f, 0.6f));

    // Short, punchy hit.
    public static AudioClip HitSound =>
        hitSound != null ? hitSound :
        (hitSound = Noise("Hit", 0.15f, 0.7f, 30f, 0.4f, 4));

    // Seamless looping wind for the background.
    public static AudioClip WindLoop =>
        windLoop != null ? windLoop :
        (windLoop = Wind("Wind Loop", 8f, 0.5f));

    // Long, low rising call: the final wave is coming.
    public static AudioClip HornSound =>
        hornSound != null ? hornSound :
        (hornSound = Growl("Horn", 1.6f, 0.7f));

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

    static AudioClip Growl(string name, float seconds, float volume)
    {
        System.Random random = new System.Random(5);
        float phase = 0f;
        float previous = 0f;

        return Create(name, seconds, time =>
        {
            float hz = Mathf.Lerp(140f, 70f, time / seconds);
            phase += 2f * Mathf.PI * hz / SampleRate;

            float white = (float)(random.NextDouble() * 2.0 - 1.0);
            previous = Mathf.Lerp(previous, white, 0.2f);

            // A square-ish tone plus noise sounds rough.
            float tone = Mathf.Sign(Mathf.Sin(phase)) * 0.5f + Mathf.Sin(phase);
            float shape = Mathf.Clamp01(time / 0.05f) * Mathf.Clamp01((seconds - time) / 0.3f);
            return volume * shape * (0.35f * tone + 0.8f * previous);
        });
    }

    // filtered noise, crossfaded so it loops
    static AudioClip Wind(string name, float seconds, float volume)
    {
        const float fadeSeconds = 0.5f;
        int count = Mathf.CeilToInt(seconds * SampleRate);
        int fade = Mathf.CeilToInt(fadeSeconds * SampleRate);

        System.Random random = new System.Random(6);
        float previous = 0f;
        float[] raw = new float[count + fade];

        for (int i = 0; i < raw.Length; i++)
        {
            float time = (float)i / SampleRate;
            float white = (float)(random.NextDouble() * 2.0 - 1.0);
            previous = Mathf.Lerp(previous, white, 0.02f);

            // Two whole gusts per clip, so the swell also loops.
            float gust = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 2f * time / seconds);
            raw[i] = volume * 6f * gust * previous;
        }

        float[] data = new float[count];
        for (int i = 0; i < count; i++)
            data[i] = raw[i];

        for (int i = 0; i < fade; i++)
        {
            float t = (float)i / fade;
            data[i] = raw[i] * t + raw[count + i] * (1f - t);
        }

        for (int i = 0; i < count; i++)
            data[i] = Mathf.Clamp(data[i], -1f, 1f);

        AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
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
