
using UnityEngine;
using TMPro;

public class GraveyardGameManager : MonoBehaviour
{
    public static GraveyardGameManager Instance { get; private set; }

    [Header("Cemetery Energy")]
    [Range(0f, 100f)]
    public float startingEnergy = 50f;
    public float energyPerDelivery = 15f;
    public float energyLostPerPumpkin = 10f;

    [Header("Scoring")]
    public int pointsPerGoblin = 5;

    [Header("Round")]
    public float roundDuration = 300f;

    [Range(0f, 100f)]
    public float minimumVictoryEnergy = 10f;

    [Header("Interface")]
    public TMP_Text scoreText;
    public TMP_Text energyText;
    public TMP_Text timerText;
    public TMP_Text resultText;

    [Header("Result Overlay")]
    public GameObject resultBackground;

    public float CurrentEnergy { get; private set; }
    public int TotalScore { get; private set; }
    public bool GameOver { get; private set; }

    private float remainingTime;

    void Awake()
    {
        Instance = this;
        CurrentEnergy = startingEnergy;
        remainingTime = roundDuration;
    }

    void Start()
    {
        // Hide the end-of-round overlay when gameplay begins.
        if (resultBackground != null)
            resultBackground.SetActive(false);
        else if (resultText != null)
            resultText.gameObject.SetActive(false);

        UpdateDisplay();
    }

    void Update()
    {
        if (GameOver)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            // Victory requires more than the minimum energy.
            EndRound(CurrentEnergy > minimumVictoryEnergy);
        }

        UpdateDisplay();
    }

    public void PumpkinDelivered(int points)
    {
        if (GameOver)
            return;

        TotalScore += points;
        ChangeEnergy(energyPerDelivery);
        UpdateDisplay();

        Debug.Log(
            $"Pumpkin delivered! Score: {TotalScore}, " +
            $"Energy: {CurrentEnergy:0}%"
        );
    }

    public void PumpkinDestroyed()
    {
        if (GameOver)
            return;

        ChangeEnergy(-energyLostPerPumpkin);
        UpdateDisplay();

        Debug.Log(
            $"Pumpkin destroyed! Energy: {CurrentEnergy:0}%"
        );
    }

    public void GoblinDefeated()
    {
        if (GameOver)
            return;

        TotalScore += pointsPerGoblin;
        UpdateDisplay();

        Debug.Log($"Goblin defeated! Score: {TotalScore}");
    }

    void ChangeEnergy(float amount)
    {
        CurrentEnergy = Mathf.Clamp(
            CurrentEnergy + amount,
            0f,
            100f
        );

        if (CurrentEnergy <= 0f)
            EndRound(false);
    }

    void EndRound(bool victory)
    {
        if (GameOver)
            return;

        GameOver = true;

        string message = victory
            ? "VICTORY!\nGRAVEYARD RESTORED"
            : "GAME OVER!\nTHE LIGHTS HAVE FAILED";

        if (resultText != null)
        {
            resultText.text = message;

            resultText.color = victory
                ? new Color(0.4f, 1f, 0.55f)
                : new Color(1f, 0.3f, 0.3f);
        }

        // Display the result panel over the regular controls.
        if (resultBackground != null)
            resultBackground.SetActive(true);

        if (resultText != null)
            resultText.gameObject.SetActive(true);

        UpdateDisplay();

        Debug.Log(message);
    }

    void UpdateDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {TotalScore}";

        if (energyText != null)
            energyText.text = $"ENERGY: {CurrentEnergy:0}%";

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(
                Mathf.Max(0f, remainingTime)
            );

            int minutes = seconds / 60;

            timerText.text =
                $"TIME: {minutes:00}:{seconds % 60:00}";
        }
    }
}
