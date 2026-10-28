
using UnityEngine;
using TMPro;

public class PumpkinDeliveryZone : MonoBehaviour
{
    [Header("Score Display")]
    public TMP_Text scoreText;

    private int totalScore = 0;

    void Start()
    {
        UpdateScoreDisplay();
    }

    private void OnTriggerEnter(Collider other)
    {
        PumpkinItem pumpkin = other.GetComponentInParent<PumpkinItem>();

        if (pumpkin == null)
            return;

        totalScore += pumpkin.pointValue;

        Debug.Log(
            $"Pumpkin delivered! +{pumpkin.pointValue} points. " +
            $"Total score: {totalScore}"
        );

        UpdateScoreDisplay();

        Destroy(pumpkin.gameObject);
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"PUMPKINS: {totalScore}";
    }
}
