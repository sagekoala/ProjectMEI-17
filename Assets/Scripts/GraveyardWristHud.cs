using TMPro;
using UnityEngine;

// Score display over the left hand
public class GraveyardWristHud : MonoBehaviour
{
    public GraveyardGameManager game;

    [Tooltip("The display floats above this transform (the left controller).")]
    public Transform anchor;

    public Vector3 offset = new Vector3(0f, 0.03f, 0f);
    public float metersPerPixel = 0.0004f;

    [Header("Colors")]
    public Color textColor = new Color(1f, 0.85f, 0.4f);
    public Color lowEnergyColor = new Color(1f, 0.3f, 0.3f);
    public Color victoryColor = new Color(0.4f, 1f, 0.55f);

    [Tooltip("Energy (%) below which the display turns red.")]
    public float lowEnergyWarning = 25f;

    [Tooltip("How long the display flashes when the energy changes (seconds).")]
    public float changeFlashDuration = 0.6f;
    public Color energyGainColor = new Color(0.4f, 1f, 0.55f);

    private GameObject hudObject;
    private TextMeshProUGUI hudText;
    private float lastEnergy = -1f;
    private float flashEndTime;
    private Color flashColor;

    void Awake()
    {
        hudObject = new GameObject("Wrist HUD", typeof(RectTransform));
        hudObject.transform.SetParent(transform, false);
        hudObject.transform.localScale = Vector3.one * metersPerPixel;

        Canvas canvas = hudObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        ((RectTransform)hudObject.transform).sizeDelta = new Vector2(560f, 120f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(hudObject.transform, false);

        hudText = textObject.AddComponent<TextMeshProUGUI>();
        hudText.fontSize = 30f;
        hudText.fontStyle = FontStyles.Bold;
        hudText.alignment = TextAlignmentOptions.Center;
        hudText.raycastTarget = false;

        RectTransform textRect = hudText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    void LateUpdate()
    {
        bool visible = game != null && anchor != null;
        hudObject.SetActive(visible);
        if (!visible)
            return;

        UpdateText();
        UpdatePlacement();
    }

    void UpdateText()
    {
        int seconds = Mathf.CeilToInt(game.RemainingTime);
        string stats =
            $"PUNTOS {game.TotalScore}   " +
            $"ENERGÍA {game.CurrentEnergy:0}%   " +
            $"VIDAS {game.GhostLivesLeft}   " +
            $"{seconds / 60}:{seconds % 60:00}";

        if (game.GameOver)
        {
            hudText.text = (game.Victory
                ? "¡VICTORIA! CEMENTERIO SALVADO"
                : "DERROTA: " + game.DefeatReason.ToUpper()) + "\n" + stats;
            hudText.color = game.Victory ? victoryColor : lowEnergyColor;
            return;
        }

        hudText.text = stats;

        // Flash red when energy is lost, green when it is gained.
        if (lastEnergy >= 0f && !Mathf.Approximately(game.CurrentEnergy, lastEnergy))
        {
            flashColor = game.CurrentEnergy < lastEnergy
                ? lowEnergyColor
                : energyGainColor;
            flashEndTime = Time.time + changeFlashDuration;
        }
        lastEnergy = game.CurrentEnergy;

        bool flashing = Time.time < flashEndTime;
        hudText.color = flashing
            ? flashColor
            : game.CurrentEnergy < lowEnergyWarning ? lowEnergyColor : textColor;
        hudText.transform.localScale = Vector3.one * (flashing ? 1.15f : 1f);
    }

    void UpdatePlacement()
    {
        Vector3 position = anchor.position + offset;
        hudObject.transform.position = position;

        // Face away from the head, like the wizard menu.
        Camera head = Camera.main;
        if (head == null)
            return;

        Vector3 away = position - head.transform.position;
        if (away.sqrMagnitude > 0.0001f)
        {
            hudObject.transform.rotation =
                Quaternion.LookRotation(away, Vector3.up);
        }
    }
}
