
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

        GraveyardGameManager.Instance.PumpkinDelivered(
            pumpkin.pointValue
        );

        if (scoreText != null)
        {
            scoreText.text =
                $"SCORE: {GraveyardGameManager.Instance.TotalScore}";
        }

        Destroy(pumpkin.gameObject);
    }
}
