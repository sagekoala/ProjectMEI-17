using UnityEngine;

// Fades a light out over a short time, then removes its GameObject.
[RequireComponent(typeof(Light))]
public class FadingLight : MonoBehaviour
{
    public float duration = 0.5f;

    private Light fadingLight;
    private float startIntensity;
    private float startTime;

    void Start()
    {
        fadingLight = GetComponent<Light>();
        startIntensity = fadingLight.intensity;
        startTime = Time.time;
    }

    void Update()
    {
        float progress = (Time.time - startTime) / duration;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        fadingLight.intensity = startIntensity * (1f - progress);
    }
}
