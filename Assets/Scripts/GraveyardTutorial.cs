using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Tutorial before each round
public class GraveyardTutorial : MonoBehaviour
{
    [Header("Game")]
    public GraveyardGameManager game;
    public WizardLevitation levitation;
    public WizardSpellCaster spellCaster;
    public WizardFlight flight;
    public WizardControlMenu menu;

    [Header("Placement")]
    [Tooltip("The panel turns to face this transform. Defaults to the main camera.")]
    public Transform head;

    public float distance = 1.6f;
    public float heightOffset = -0.15f;

    [Tooltip("Re-center angle (degrees)")]
    public float recenterAngle = 35f;

    public float followSpeed = 3f;
    public float metersPerPixel = 0.001f;

    [Header("Feedback")]
    public HapticImpulsePlayer haptics;

    static readonly Color PanelColor = new Color(0.08f, 0.04f, 0.1f, 0.92f);
    static readonly Color ButtonColor = new Color(0.3f, 0.2f, 0.35f);
    static readonly Color StartColor = new Color(0.9f, 0.45f, 0.1f);
    static readonly Color TitleColor = new Color(1f, 0.6f, 0.15f);
    static readonly Color TextColor = new Color(0.95f, 0.92f, 1f);
    static readonly Color DoneColor = new Color(0.45f, 1f, 0.4f);

    const float PanelWidth = 820f;

    class Step
    {
        public string title;
        public string body;

        // Null: a reading step, completed with the button.
        public Func<bool> isDone;
    }

    private readonly List<Step> steps = new List<Step>();
    private int stepIndex;
    private float stepShownTime;
    private bool stepCompleted;
    private bool isRecentering;
    private bool countingDown;

    private GameObject canvasObject;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bodyText;
    private TextMeshProUGUI statusText;
    private Button skipTutorialButton;
    private Button nextButton;
    private TextMeshProUGUI nextLabel;
    private Image nextImage;

    void Start()
    {
        if (game == null || game.RoundStarted)
            return;

        BuildSteps();
        BuildPanel();
        ShowStep(0);
        PlaceInFront(true);
    }

    void Update()
    {
        if (canvasObject == null || !canvasObject.activeSelf)
            return;

        if (countingDown || stepCompleted)
            return;

        Step step = steps[stepIndex];
        if (step.isDone != null && step.isDone())
            StartCoroutine(CompleteStep());
    }

    void LateUpdate()
    {
        if (canvasObject != null && canvasObject.activeSelf)
            PlaceInFront(false);
    }

    // ---------- Steps ----------

    void BuildSteps()
    {
        int minutes = Mathf.RoundToInt(game.RemainingTime / 60f);

        steps.Add(new Step
        {
            title = "Guardián del cementerio",
            body =
                "Los duendes quieren destruir las calabazas mágicas.\n\n" +
                "Lleva las calabazas a la tumba iluminada y elimina " +
                "a los duendes con tus hechizos.\n\n" +
                $"Mantén la energía por encima del {game.minimumVictoryEnergy:0}% " +
                $"durante {minutes} minutos para salvar el cementerio.",
        });

        if (levitation != null)
        {
            steps.Add(new Step
            {
                title = "Telequinesis",
                body =
                    "Apunta a una calabaza con la mano izquierda y " +
                    "mantén el GATILLO IZQUIERDO para levitarla.\n\n" +
                    "Estira el brazo para alejarla y recógelo para acercarla. " +
                    "Mantén Y y gira la muñeca para rotarla; A / B para " +
                    "agrandarla o encogerla.\n\n" +
                    "Las grandes valen más y los duendes tardan más en " +
                    "comérselas, pero pesan más.",
                isDone = () => levitation.HeldObject != null,
            });

            GhostSystem ghosts = GhostSystem.Instance;
            if (ghosts != null && ghosts.Lantern != null)
            {
                steps.Add(new Step
                {
                    title = "Linterna anti-fantasmas",
                    body =
                        "Durante la ronda, un fantasma vendrá a por ti. Si te toca, " +
                        $"pierdes una vida (tienes {game.ghostLives}).\n\n" +
                        "Coge la LINTERNA ANTI-FANTASMAS con telequinesis (está cerca de ti): " +
                        "lo sostienes como una linterna. MANTÉN Y para encender su luz " +
                        "verde y apúntala al fantasma girando la muñeca.\n\n" +
                        "A / B: haz más ancho o más estrecho.",
                    isDone = () => ghosts.Lantern.IsShining,
                });
            }
        }

        if (spellCaster != null)
        {
            steps.Add(new Step
            {
                title = "Hechizos",
                body =
                    "Pulsa el GATILLO DERECHO para lanzar un hechizo.\n\n" +
                    "La bolita de tu mano derecha muestra su color y " +
                    "crece mientras el hechizo se recarga.",
                // only count spells cast after the step starts
                isDone = () => spellCaster.LastCastTime > stepShownTime,
            });
        }

        if (flight != null)
        {
            steps.Add(new Step
            {
                title = "Vuelo mágico",
                body =
                    "Mantén el GRIP DERECHO para volar hacia donde " +
                    "apunta tu mano. Cuanto más aprietes, más rápido.\n\n" +
                    "Al soltarlo, vuelves a caer.",
                isDone = () => flight.IsFlying,
            });
        }

        if (menu != null)
        {
            steps.Add(new Step
            {
                title = "El grimorio",
                body =
                    "Pulsa X para abrir tu grimorio: hechizos, opciones " +
                    "de movimiento, estado de la partida y ayuda.\n\n" +
                    "Entregando calabazas desbloqueas hechizos más potentes.",
                isDone = () => menu.IsOpen,
            });
        }

        steps.Add(new Step
        {
            title = "¿Preparado?",
            body =
                "Los duendes aparecerán en cuanto empiece la ronda. " +
                "Al final llega la OLEADA FINAL: arqueros rápidos pero " +
                "débiles.\n\n" +
                "Cierra el grimorio si lo tienes abierto y pulsa " +
                "EMPEZAR RONDA.",
        });
    }

    void ShowStep(int index)
    {
        stepIndex = Mathf.Clamp(index, 0, steps.Count - 1);
        stepShownTime = Time.time;
        stepCompleted = false;

        Step step = steps[stepIndex];
        bool isLast = stepIndex == steps.Count - 1;

        progressText.text = $"Paso {stepIndex + 1} de {steps.Count}";
        titleText.text = step.title;
        bodyText.text = step.body;
        statusText.text = step.isDone != null ? "Pruébalo ahora" : "";
        statusText.color = TextColor;

        nextLabel.text = isLast
            ? "EMPEZAR RONDA"
            : step.isDone != null ? "Saltar paso" : "Siguiente";
        nextImage.color = isLast ? StartColor : ButtonColor;

        skipTutorialButton.gameObject.SetActive(!isLast);
    }

    IEnumerator CompleteStep()
    {
        stepCompleted = true;
        statusText.text = "¡Bien hecho!";
        statusText.color = DoneColor;

        MagicFeedback.Play(
            MagicFeedback.ChimeSound, canvasObject.transform.position, 0.8f
        );
        Pulse(0.4f, 0.15f);

        yield return new WaitForSeconds(1.2f);
        ShowStep(stepIndex + 1);
    }

    void OnNext()
    {
        Pulse(0.2f, 0.05f);

        if (stepIndex == steps.Count - 1)
        {
            StartCoroutine(Countdown());
            return;
        }

        StopAllCoroutines();
        ShowStep(stepIndex + 1);
    }

    void OnSkipTutorial()
    {
        Pulse(0.2f, 0.05f);
        StopAllCoroutines();
        ShowStep(steps.Count - 1);
    }

    IEnumerator Countdown()
    {
        countingDown = true;
        nextButton.gameObject.SetActive(false);
        skipTutorialButton.gameObject.SetActive(false);
        progressText.text = "";
        bodyText.text = "";
        statusText.text = "";

        for (int i = 3; i > 0; i--)
        {
            titleText.text = i.ToString();
            MagicFeedback.Play(
                MagicFeedback.ClickSound, canvasObject.transform.position, 1f
            );
            Pulse(0.3f, 0.1f);
            yield return new WaitForSeconds(1f);
        }

        titleText.text = "¡Defiende el cementerio!";
        MagicFeedback.Play(
            MagicFeedback.ChimeSound, canvasObject.transform.position, 1f
        );
        Pulse(0.6f, 0.3f);

        game.StartRound();

        yield return new WaitForSeconds(1.5f);
        canvasObject.SetActive(false);
    }

    void Pulse(float amplitude, float duration)
    {
        if (haptics != null)
            haptics.SendHapticImpulse(amplitude, duration);
    }

    // ---------- Placement ----------

    // keep the panel in front of the player
    void PlaceInFront(bool immediately)
    {
        if (head == null)
        {
            if (Camera.main == null)
                return;

            head = Camera.main.transform;
        }

        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            return;

        forward.Normalize();

        Vector3 target =
            head.position + forward * distance + Vector3.up * heightOffset;

        Transform panel = canvasObject.transform;

        Vector3 toPanel = Vector3.ProjectOnPlane(panel.position - head.position, Vector3.up);
        float angle = Vector3.Angle(forward, toPanel);

        if (angle > recenterAngle)
            isRecentering = true;
        else if (angle < 2f)
            isRecentering = false;

        if (immediately)
        {
            panel.position = target;
        }
        else if (isRecentering)
        {
            panel.position = Vector3.Lerp(
                panel.position, target, followSpeed * Time.deltaTime
            );
        }

        // A world-space canvas is read from behind its forward axis.
        Vector3 away = panel.position - head.position;
        away.y = 0f;
        if (away.sqrMagnitude > 0.0001f)
            panel.rotation = Quaternion.LookRotation(away, Vector3.up);
    }

    // ---------- Panel ----------

    void BuildPanel()
    {
        canvasObject = new GameObject("Tutorial Canvas", typeof(RectTransform));
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

        VerticalLayoutGroup layout =
            panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(40, 40, 30, 30);
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        progressText = AddText(panel, 26f, TextColor);
        titleText = AddText(panel, 64f, TitleColor);
        bodyText = AddText(panel, 34f, TextColor);
        statusText = AddText(panel, 40f, TextColor);

        GameObject row = new GameObject("Buttons", typeof(RectTransform));
        row.transform.SetParent(panel, false);

        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 20f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = false;

        skipTutorialButton = AddButton(row.transform, "Saltar tutorial", OnSkipTutorial);
        nextButton = AddButton(row.transform, "Siguiente", OnNext);
        nextLabel = nextButton.GetComponentInChildren<TextMeshProUGUI>();
        nextImage = (Image)nextButton.targetGraphic;
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static TextMeshProUGUI AddText(Transform parent, float size, Color color)
    {
        TextMeshProUGUI text =
            CreateRect("Text", parent).gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    static Button AddButton(Transform parent, string label, UnityAction onClick)
    {
        RectTransform rect = CreateRect("Button", parent);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = ButtonColor;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        // Hover/press feedback; "selected" keeps the normal tint.
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.85f, 0.85f, 0.85f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        colors.selectedColor = colors.normalColor;
        button.colors = colors;

        button.onClick.AddListener(onClick);

        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 90f;

        TextMeshProUGUI text = AddText(rect, 38f, TextColor);
        text.text = label;
        text.fontStyle = FontStyles.Bold;

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }
}
