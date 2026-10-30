
using UnityEngine;
using TMPro;

public class PumpkinDeliveryZone : MonoBehaviour
{
    [Header("Score Display")]
    public TMP_Text scoreText;

    private void OnTriggerEnter(Collider other)
    {
        PumpkinItem pumpkin =
            other.GetComponentInParent<PumpkinItem>();

        if (pumpkin == null)
            return;

        if (GraveyardGameManager.Instance == null)
        {
            Debug.LogWarning("GraveyardGameManager not found.");
            return;
        }

        // Bigger pumpkins are worth more points and energy.
        bool counted = GraveyardGameManager.Instance.IsPlaying;
        int points = pumpkin.DeliveryPoints;
        float energy =
            GraveyardGameManager.Instance.energyPerDelivery * pumpkin.SizeMultiplier;

        GraveyardGameManager.Instance.PumpkinDelivered(
            points, pumpkin.SizeMultiplier
        );

        if (counted)
        {
            FloatingText.Spawn(
                pumpkin.transform.position + Vector3.up * 0.4f,
                $"+{points} pts\n+{energy:0}% energía",
                new Color(1f, 0.85f, 0.3f)
            );
        }

        if (scoreText != null)
        {
            scoreText.text =
                $"SCORE: {GraveyardGameManager.Instance.TotalScore}";
        }

        if (GraveyardFeedback.Instance != null)
        {
            GraveyardFeedback.Instance.PumpkinDelivered(
                pumpkin.transform.position
            );
        }

        Destroy(pumpkin.gameObject);
    }
}
