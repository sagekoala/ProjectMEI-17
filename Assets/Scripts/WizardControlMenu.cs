using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Wrist "spellbook" menu: a world-space panel that floats above the left
// controller, organized in tabs: spell color, locomotion options and the
// world (day/night and the soul hunt mini-game).
// Open/close it with the left Menu button; use it with the right hand
// ray (trigger) or by poking it.
public class WizardControlMenu : MonoBehaviour
{
    [Header("Targets")]
    public WizardSpellCaster spellCaster;
    public WizardLocomotionSettings locomotion;
    public NightAtmosphere atmosphere;
    public SoulHunt soulHunt;

    [Tooltip("Paused while the menu is open, so menu clicks don't cast spells or levitate objects.")]
    public Behaviour[] disableWhileOpen;

    [Header("Placement")]
    [Tooltip("The menu floats above this transform (the left controller).")]
    public Transform followTarget;

    [Tooltip("The menu turns to face this transform. Defaults to the main camera.")]
    public Transform head;

    public Vector3 followOffset = new Vector3(0f, 0.08f, 0f);

    [Tooltip("World size of one canvas pixel, in meters.")]
    public float metersPerPixel = 0.0004f;

    [Header("Input")]
    [Tooltip("Input System binding paths that open and close the menu.")]
    public string[] toggleBindings =
    {
        "<XRController>{LeftHand}/menu",
        "<Keyboard>/f2",
    };

    public bool startOpen = false;

    [Header("Feedback")]
    [Tooltip("Haptics of the hand that clicks the menu (the right controller).")]
    public HapticImpulsePlayer haptics;

    [Header("Spell Colors")]
    public Color[] spellColors =
    {
        Color.cyan,
        new Color(1f, 0.45f, 0.1f),  // fire
        new Color(0.35f, 1f, 0.35f), // poison
        new Color(0.7f, 0.35f, 1f),  // arcane
    };

    static readonly Color PanelColor = new Color(0.08f, 0.06f, 0.12f, 0.9f);
    static readonly Color ButtonColor = new Color(0.25f, 0.22f, 0.32f);
    static readonly Color SelectedColor = new Color(0.55f, 0.35f, 0.9f);
    static readonly Color TextColor = new Color(0.95f, 0.92f, 1f);

    const float PanelWidth = 440f;
    const float ButtonHeight = 56f;
    const float StepperButtonWidth = 72f;

    private InputAction toggleAction;
    private GameObject canvasObject;

    private readonly List<Button> tabButtons = new List<Button>();
    private readonly List<GameObject> tabPages = new List<GameObject>();

    private Button[] colorButtons;
    private Button walkButton;
    private Button teleportButton;
    private Button smoothTurnButton;
    private Button snapTurnButton;
    private TextMeshProUGUI moveSpeedValue;
    private TextMeshProUGUI turnTitle;
    private TextMeshProUGUI turnValue;
    private Button dayButton;
    private Button nightButton;
    private Button huntButton;
    private TextMeshProUGUI huntStatus;

    public bool IsOpen =>
        canvasObject != null && canvasObject.activeSelf;

    void Awake()
    {
        toggleAction = new InputAction(
            "Toggle Wizard Menu", InputActionType.Button
        );

        foreach (string binding in toggleBindings)
        {
            if (!string.IsNullOrWhiteSpace(binding))
                toggleAction.AddBinding(binding);
        }

        EnsureXREventSystem();
    }

    void OnEnable()
    {
        toggleAction.Enable();

        if (locomotion != null)
            locomotion.Changed += Refresh;

        if (atmosphere != null)
            atmosphere.Changed += Refresh;

        if (soulHunt != null)
            soulHunt.Changed += Refresh;
    }

    void OnDisable()
    {
        toggleAction.Disable();

        if (locomotion != null)
            locomotion.Changed -= Refresh;

        if (atmosphere != null)
            atmosphere.Changed -= Refresh;

        if (soulHunt != null)
            soulHunt.Changed -= Refresh;
    }

    void OnDestroy()
    {
        toggleAction.Dispose();
    }

    void Start()
    {
        // Built in Start so the locomotion settings have validated
        // their references in Awake.
        BuildMenu();
        SetOpen(startOpen);
    }

    void Update()
    {
        if (canvasObject != null && toggleAction.WasPressedThisFrame())
        {
            SetOpen(!IsOpen);
            MagicFeedback.Play(
                MagicFeedback.WhooshSound, canvasObject.transform.position, 0.4f
            );
        }

        // Keep the hunt timer ticking while the menu is open.
        if (IsOpen && soulHunt != null && soulHunt.IsRunning)
            RefreshHunt();
    }

    void LateUpdate()
    {
        if (IsOpen)
            UpdatePlacement();
    }

    public void SetOpen(bool open)
    {
        canvasObject.SetActive(open);

        foreach (Behaviour behaviour in disableWhileOpen)
        {
            if (behaviour != null)
                behaviour.enabled = !open;
        }

        if (open)
        {
            Refresh();
            UpdatePlacement();
        }
    }

    void UpdatePlacement()
    {
        if (followTarget == null)
            return;

        if (head == null && Camera.main != null)
            head = Camera.main.transform;

        Vector3 position = followTarget.position + followOffset;
        canvasObject.transform.position = position;

        if (head == null)
            return;

        // A world-space canvas is read from behind its forward axis,
        // so point it away from the head.
        Vector3 away = position - head.position;
        if (away.sqrMagnitude > 0.0001f)
        {
            canvasObject.transform.rotation =
                Quaternion.LookRotation(away, Vector3.up);
        }
    }

    void SelectSpellColor(int index)
    {
        spellCaster.spellColor = spellColors[index];
        Refresh();
    }

    void Refresh()
    {
        if (colorButtons != null)
        {
            for (int i = 0; i < colorButtons.Length; i++)
            {
                colorButtons[i].GetComponent<Outline>().enabled =
                    spellColors[i] == spellCaster.spellColor;
            }
        }

        if (walkButton != null)
        {
            bool walk = locomotion.IsWalkEnabled;
            SetSelected(walkButton, walk);
            SetSelected(teleportButton, !walk);

            bool smooth = locomotion.IsSmoothTurnEnabled;
            SetSelected(smoothTurnButton, smooth);
            SetSelected(snapTurnButton, !smooth);

            moveSpeedValue.text = $"{locomotion.MoveSpeed:0.0} m/s";

            if (smooth)
            {
                turnTitle.text = "Velocidad de giro";
                turnValue.text = $"{locomotion.TurnSpeed:0}°/s";
            }
            else
            {
                turnTitle.text = "Ángulo de giro";
                turnValue.text = $"{locomotion.SnapAngle:0}°";
            }
        }

        if (dayButton != null)
        {
            SetSelected(dayButton, !atmosphere.IsNight);
            SetSelected(nightButton, atmosphere.IsNight);
        }

        RefreshHunt();
    }

    void RefreshHunt()
    {
        if (huntButton == null)
            return;

        if (soulHunt.IsRunning)
        {
            huntStatus.text =
                $"Almas: {soulHunt.Remaining}/{soulHunt.Total}   " +
                SoulHunt.FormatTime(soulHunt.ElapsedTime);
        }
        else if (soulHunt.BestTime >= 0f)
        {
            huntStatus.text =
                $"Récord: {SoulHunt.FormatTime(soulHunt.BestTime)}";
        }
        else
        {
            huntStatus.text = "Exorciza todas las almas con tus hechizos";
        }

        huntButton.GetComponentInChildren<TextMeshProUGUI>().text =
            soulHunt.IsRunning ? "Reiniciar caza" : "Empezar caza";
    }

    void StartHunt()
    {
        soulHunt.StartHunt();

        // Close the book so the player can start hunting straight away.
        SetOpen(false);
    }

    void SelectTab(int index)
    {
        for (int i = 0; i < tabPages.Count; i++)
        {
            tabPages[i].SetActive(i == index);
            SetSelected(tabButtons[i], i == index);
        }
    }

    // Called by the menu buttons.
    public void OnButtonHovered()
    {
        MagicFeedback.Play(
            MagicFeedback.HoverSound, canvasObject.transform.position, 0.4f
        );
        MagicFeedback.Pulse(haptics, 0.08f, 0.02f);
    }

    void OnButtonClicked()
    {
        MagicFeedback.Play(
            MagicFeedback.ClickSound, canvasObject.transform.position, 0.6f
        );
        MagicFeedback.Pulse(haptics, 0.2f, 0.04f);
    }

    // ---------- Menu construction ----------

    void BuildMenu()
    {
        canvasObject = new GameObject(
            "Wizard Menu Canvas", typeof(RectTransform)
        );
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localScale = Vector3.one * metersPerPixel;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (Camera.main != null)
            canvas.worldCamera = Camera.main;

        // Lets XR interactors (ray and poke) click the canvas.
        canvasObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Bottom pivot: the panel grows upwards from above the hand.
        RectTransform panel = CreateRect("Panel", canvasObject.transform);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.sizeDelta = new Vector2(PanelWidth, 0f);
        panel.gameObject.AddComponent<Image>().color = PanelColor;

        VerticalLayoutGroup layout =
            panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        AddText(panel, "Grimorio", 40f, FontStyles.Bold,
            TextAlignmentOptions.Center);

        RectTransform tabRow = AddRow(panel);

        if (spellCaster != null)
            AddTab(panel, tabRow, "Hechizo", BuildSpellSection);

        if (locomotion != null && locomotion.IsConfigured)
            AddTab(panel, tabRow, "Movimiento", BuildLocomotionSection);

        if (atmosphere != null || soulHunt != null)
            AddTab(panel, tabRow, "Mundo", BuildWorldSection);

        AddButton(panel, "Cerrar", ButtonColor, () => SetOpen(false));

        AddText(panel, "Botón Menú (mando izq.): abrir / cerrar",
            20f, FontStyles.Italic, TextAlignmentOptions.Center);

        SelectTab(0);

        // Sound and vibration for every button.
        foreach (Button button in canvasObject.GetComponentsInChildren<Button>(true))
        {
            button.onClick.AddListener(OnButtonClicked);
            button.gameObject.AddComponent<MenuButtonHover>().menu = this;
        }
    }

    // A tab button plus the page it shows. Pages are laid out below the
    // tab row; only the selected one is active.
    void AddTab(
        Transform panel,
        Transform tabRow,
        string title,
        Action<Transform> buildPage)
    {
        int index = tabPages.Count;

        tabButtons.Add(AddButton(tabRow, title, ButtonColor,
            () => SelectTab(index)));

        RectTransform page = CreateRect(title + " Page", panel);

        VerticalLayoutGroup layout =
            page.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        buildPage(page);
        tabPages.Add(page.gameObject);
    }

    void BuildSpellSection(Transform panel)
    {
        AddSectionTitle(panel, "Color del hechizo");

        RectTransform row = AddRow(panel);
        colorButtons = new Button[spellColors.Length];

        for (int i = 0; i < spellColors.Length; i++)
        {
            int index = i;
            Button button = AddButton(row, "", spellColors[i],
                () => SelectSpellColor(index));

            // Highlights the selected color.
            Outline outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(5f, -5f);

            colorButtons[i] = button;
        }
    }

    void BuildLocomotionSection(Transform panel)
    {
        AddSectionTitle(panel, "Stick izquierdo");
        RectTransform moveRow = AddRow(panel);
        walkButton = AddButton(moveRow, "Caminar", ButtonColor,
            () => locomotion.SetWalkEnabled(true));
        teleportButton = AddButton(moveRow, "Teletransporte", ButtonColor,
            () => locomotion.SetWalkEnabled(false));

        AddSectionTitle(panel, "Velocidad al caminar");
        moveSpeedValue = AddStepper(panel,
            () => locomotion.ChangeMoveSpeed(-1),
            () => locomotion.ChangeMoveSpeed(1));

        AddSectionTitle(panel, "Giro");
        RectTransform turnRow = AddRow(panel);
        smoothTurnButton = AddButton(turnRow, "Suave", ButtonColor,
            () => locomotion.SetSmoothTurn(true));
        snapTurnButton = AddButton(turnRow, "Por pasos", ButtonColor,
            () => locomotion.SetSmoothTurn(false));

        turnTitle = AddSectionTitle(panel, "");
        turnValue = AddStepper(panel,
            () => locomotion.ChangeTurn(-1),
            () => locomotion.ChangeTurn(1));
    }

    void BuildWorldSection(Transform panel)
    {
        if (atmosphere != null)
        {
            AddSectionTitle(panel, "Ambiente");
            RectTransform row = AddRow(panel);
            dayButton = AddButton(row, "Día", ButtonColor,
                () => atmosphere.SetNight(false));
            nightButton = AddButton(row, "Noche", ButtonColor,
                () => atmosphere.SetNight(true));
        }

        if (soulHunt != null)
        {
            AddSectionTitle(panel, "Caza de almas");
            huntStatus = AddText(panel, "", 24f, FontStyles.Normal,
                TextAlignmentOptions.Center);
            huntButton = AddButton(panel, "Empezar caza", SelectedColor,
                StartHunt);
        }
    }

    static void EnsureXREventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            new GameObject(
                "EventSystem", typeof(EventSystem), typeof(XRUIInputModule)
            );
            return;
        }

        if (!eventSystem.TryGetComponent(out XRUIInputModule _))
        {
            Debug.LogWarning(
                "The EventSystem has no XRUIInputModule; the wizard menu " +
                "may not receive XR controller input.", eventSystem);
        }
    }

    static void SetSelected(Button button, bool selected)
    {
        button.targetGraphic.color =
            selected ? SelectedColor : ButtonColor;
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static RectTransform AddRow(Transform parent)
    {
        RectTransform row = CreateRect("Row", parent);

        HorizontalLayoutGroup layout =
            row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        return row;
    }

    static TextMeshProUGUI AddSectionTitle(Transform parent, string text)
    {
        return AddText(parent, text, 26f, FontStyles.Normal,
            TextAlignmentOptions.Left);
    }

    static TextMeshProUGUI AddText(
        Transform parent,
        string text,
        float size,
        FontStyles style,
        TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect("Text", parent);

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = TextColor;
        label.raycastTarget = false;

        return label;
    }

    static Button AddButton(
        Transform parent,
        string text,
        Color color,
        UnityAction onClick)
    {
        RectTransform rect = CreateRect("Button", parent);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        // Hover/press feedback. "Selected" keeps the normal tint so a
        // clicked button does not stay highlighted.
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.85f, 0.85f, 0.85f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        colors.selectedColor = colors.normalColor;
        button.colors = colors;

        button.onClick.AddListener(onClick);

        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = ButtonHeight;
        layout.flexibleWidth = 1f;

        if (!string.IsNullOrEmpty(text))
        {
            TextMeshProUGUI label = AddText(rect, text, 26f,
                FontStyles.Bold, TextAlignmentOptions.Center);

            // Long labels shrink to fit narrow buttons.
            label.enableAutoSizing = true;
            label.fontSizeMin = 14f;
            label.fontSizeMax = 26f;

            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        return button;
    }

    // [-] value [+] row. Returns the value label.
    static TextMeshProUGUI AddStepper(
        Transform parent,
        UnityAction onLess,
        UnityAction onMore)
    {
        RectTransform row = AddRow(parent);

        Button less = AddButton(row, "-", ButtonColor, onLess);

        TextMeshProUGUI value = AddText(row, "", 28f, FontStyles.Bold,
            TextAlignmentOptions.Center);
        value.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

        Button more = AddButton(row, "+", ButtonColor, onMore);

        foreach (Button button in new[] { less, more })
        {
            LayoutElement layout = button.GetComponent<LayoutElement>();
            layout.flexibleWidth = 0f;
            layout.preferredWidth = StepperButtonWidth;
        }

        return value;
    }
}
