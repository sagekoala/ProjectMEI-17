
using UnityEngine;

public class GraveyardLightingController : MonoBehaviour
{
    [Header("Protective Lights")]
    public Light[] protectiveLights;

    [Header("Brightness")]
    public float maxIntensity = 8f;
    public float transitionSpeed = 3f;
    public float brightnessExponent = 2f;

    void Update()
    {
        if (GraveyardGameManager.Instance == null)
            return;

        float energy = Mathf.Clamp01(
            GraveyardGameManager.Instance.CurrentEnergy / 100f
        );

        // Make the lights dim faster as cemetery energy decreases.
        float brightness = Mathf.Pow(energy, brightnessExponent);
        float targetIntensity = maxIntensity * brightness;

        foreach (Light light in protectiveLights)
        {
            if (light == null)
                continue;

            light.intensity = Mathf.MoveTowards(
                light.intensity,
                targetIntensity,
                transitionSpeed * Time.deltaTime
            );
        }
    }
}
