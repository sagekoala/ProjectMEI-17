
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
    [Header("Round Start")]
    [Tooltip("If true, the round waits for StartRound() (e.g. after the tutorial).")]
    public bool waitForStart = false;

    public bool GameOver { get; private set; }
    public bool RoundStarted { get; private set; }

    // The round is running: started and not over yet.
    public bool IsPlaying => RoundStarted && !GameOver;
    public bool Victory { get; private set; }
    public float RemainingTime => remainingTime;

    private float remainingTime;

    void Awake()
    {
        Instance = this;
        RoundStarted = !waitForStart;
        CurrentEnergy = startingEnergy;
        remainingTime = roundDuration;

        // The end-of-round screen and the cemetery limits. Added here if
        // they are not in the scene, with their default settings; add
        // them to this object in the editor to tweak them.
        if (FindFirstObjectByType<GraveyardEndScreen>() == null)
            gameObject.AddComponent<GraveyardEndScreen>();

        if (FindFirstObjectByType<PlayAreaBounds>() == null)
            gameObject.AddComponent<PlayAreaBounds>();
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
        if (!IsPlaying)
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
        if (!IsPlaying)
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
        if (!IsPlaying)
            return;

        ChangeEnergy(-energyLostPerPumpkin);
        UpdateDisplay();

        Debug.Log(
            $"Pumpkin destroyed! Energy: {CurrentEnergy:0}%"
        );
    }

    public void GoblinDefeated()
    {
        if (!IsPlaying)
            return;

        TotalScore += pointsPerGoblin;
        UpdateDisplay();

        Debug.Log($"Goblin defeated! Score: {TotalScore}");
    }

    // Starts a round that was waiting (see waitForStart).
    public void StartRound()
    {
        if (RoundStarted || GameOver)
            return;

        RoundStarted = true;
        UpdateDisplay();
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
        Victory = victory;

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
