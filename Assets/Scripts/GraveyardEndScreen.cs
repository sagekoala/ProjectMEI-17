
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

// End of round panel (result, stats, play again)
public class GraveyardEndScreen : MonoBehaviour
{
    [Header("Targets (found automatically if empty)")]
    public GraveyardGameManager game;
    public Transform head;

    [Tooltip("Disabled while the screen is shown")]
    public Behaviour[] disableWhileShown;

    [Header("Placement")]
    public float distance = 1.6f;
    [Tooltip("Height offset")]
    public float heightOffset = -0.1f;
    [Tooltip("Delay before showing (s)")]
    public float delay = 1f;

    [Tooltip("Canvas scale")]
    public float metersPerPixel = 0.0012f;

    static readonly Color PanelColor = new Color(0.08f, 0.06f, 0.12f, 0.92f);
    static readonly Color ButtonColor = new Color(0.25f, 0.22f, 0.32f);
    static readonly Color AccentColor = new Color(0.55f, 0.35f, 0.9f);
    static readonly Color TextColor = new Color(0.95f, 0.92f, 1f);
    static readonly Color VictoryColor = new Color(0.4f, 1f, 0.55f);
    static readonly Color DefeatColor = new Color(1f, 0.3f, 0.3f);

    const float PanelWidth = 720f;

    private GameObject canvasObject;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI reasonText;
    private TextMeshProUGUI statsText;

    private bool shown;
    private float gameOverTime = -1f;

    void Start()
    {
        if (game == null)
            game = GraveyardGameManager.Instance;

        if (head == null && Camera.main != null)
            head = Camera.main.transform;

        if (disableWhileShown == null || disableWhileShown.Length == 0)
        {
            disableWhileShown = new Behaviour[]
            {
                FindAnyObjectByType<WizardSpellCaster>(),
                FindAnyObjectByType<WizardLevitation>(),
            };
        }

        BuildScreen();
        canvasObject.SetActive(false);
    }

    void Update()
    {
        if (game == null || shown || !game.GameOver)
            return;

        if (gameOverTime < 0f)
            gameOverTime = Time.unscaledTime;

        if (Time.unscaledTime - gameOverTime >= delay)
            Show();
    }

    void Show()
    {
        shown = true;

        bool victory = game.Victory;
        titleText.text = victory ? "¡VICTORIA!" : "FIN DE LA PARTIDA";
        titleText.color = victory ? VictoryColor : DefeatColor;

        reasonText.text = victory
            ? "¡Has salvado el cementerio!"
            : game.DefeatReason;

        int survived = Mathf.FloorToInt(
            Mathf.Max(0f, game.roundDuration - game.RemainingTime));

        statsText.text =
            $"Puntos: <b>{game.TotalScore}</b>\n" +
            $"Energía final: <b>{game.CurrentEnergy:0}%</b>\n" +
            $"Vidas restantes: <b>{game.GhostLivesLeft}/{game.ghostLives}</b>\n" +
            $"Tiempo resistido: <b>{survived / 60}:{survived % 60:00}</b>";

        PlaceInFrontOfPlayer();
        canvasObject.SetActive(true);
        SetGameplayEnabled(false);

        Vector3 position = canvasObject.transform.position;
        MagicFeedback.Play(
            victory ? MagicFeedback.ChimeSound : MagicFeedback.FailSound,
            position, 1f);
    }

    void Close()
    {
        canvasObject.SetActive(false);
        SetGameplayEnabled(true);
    }

    void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void SetGameplayEnabled(bool enable)
    {
        foreach (Behaviour behaviour in disableWhileShown)
        {
            if (behaviour != null)
                behaviour.enabled = enable;
        }
    }

    // Straight ahead of the player, ignoring whether they look up or down.
    void PlaceInFrontOfPlayer()
    {
        if (head == null)
            return;

        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.ProjectOnPlane(head.up, Vector3.up);
        forward.Normalize();

        Vector3 position = head.position + forward * distance;
        position.y += heightOffset;

        canvasObject.transform.SetPositionAndRotation(
            position, Quaternion.LookRotation(forward, Vector3.up));
    }

    // ---------- Screen construction ----------

    void BuildScreen()
    {
        canvasObject = new GameObject("End Screen Canvas", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localScale = Vector3.one * metersPerPixel;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (Camera.main != null)
            canvas.worldCamera = Camera.main;

        // Lets XR interactors (ray and poke) click the canvas.
        canvasObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        RectTransform panel = CreateRect("Panel", canvasObject.transform);
        panel.sizeDelta = new Vector2(PanelWidth, 0f);
        panel.gameObject.AddComponent<Image>().color = PanelColor;

        VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 36, 36);
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        titleText = AddText(panel, "", 72f, FontStyles.Bold);
        reasonText = AddText(panel, "", 34f, FontStyles.Italic);
        statsText = AddText(panel, "", 36f, FontStyles.Normal);

        RectTransform row = CreateRect("Buttons", panel);
        HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 16f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = false;

        AddButton(row, "Jugar de nuevo", AccentColor, PlayAgain);
        AddButton(row, "Cerrar", ButtonColor, Close);

        AddText(panel, "Apunta con la mano derecha y pulsa el gatillo",
            22f, FontStyles.Italic);
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static TextMeshProUGUI AddText(Transform parent, string text, float size, FontStyles style)
    {
        RectTransform rect = CreateRect("Text", parent);

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.color = TextColor;
        label.raycastTarget = false;

        return label;
    }

    static Button AddButton(Transform parent, string text, Color color, UnityAction onClick)
    {
        RectTransform rect = CreateRect("Button", parent);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.85f, 0.85f, 0.85f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        colors.selectedColor = colors.normalColor;
        button.colors = colors;

        button.onClick.AddListener(() =>
            MagicFeedback.Play(MagicFeedback.ClickSound, rect.position, 0.6f));
        button.onClick.AddListener(onClick);

        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 90f;
        layout.flexibleWidth = 1f;

        TextMeshProUGUI label = AddText(rect, text, 36f, FontStyles.Bold);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }
}
