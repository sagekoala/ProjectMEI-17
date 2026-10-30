using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Wrist "spellbook" menu: a world-space panel that floats above the left
// controller, organized in tabs: spell, movement, game and help.
// Open/close it with the X button; use it with the right hand ray
// (trigger) or by poking it. The panel is built in code.
public class WizardControlMenu : MonoBehaviour
{
    [Header("Targets")]
    public WizardSpellCaster spellCaster;
    public WizardLocomotionSettings locomotion;
    public WizardFlight flight;
    public GraveyardGameManager game;

    [Tooltip("Used to show the points of a delivered pumpkin in the help.")]
    public PumpkinSpawner pumpkinSpawner;

    [Tooltip("Paused while the menu is open, so menu clicks don't cast spells, levitate or fly.")]
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
        "<XRController>{LeftHand}/{PrimaryButton}",
        "<Keyboard>/f2",
    };

    public bool startOpen = false;

    [Header("Flight Speed (m/s)")]
    public float minFlightSpeed = 1f;
    public float maxFlightSpeed = 8f;
    public float flightSpeedStep = 1f;

    [Header("Feedback")]
    [Tooltip("Haptics of the hand that clicks the menu (the right controller).")]
    public HapticImpulsePlayer haptics;

    static readonly Color PanelColor = new Color(0.08f, 0.06f, 0.12f, 0.9f);
    static readonly Color ButtonColor = new Color(0.25f, 0.22f, 0.32f);
    static readonly Color SelectedColor = new Color(0.55f, 0.35f, 0.9f);
    static readonly Color TextColor = new Color(0.95f, 0.92f, 1f);
    static readonly Color VictoryColor = new Color(0.4f, 1f, 0.55f);
    static readonly Color DefeatColor = new Color(1f, 0.3f, 0.3f);
    static readonly Color LockedColor = new Color(0.16f, 0.15f, 0.18f);

    const float PanelWidth = 460f;
    const float ButtonHeight = 56f;
    const float StepperButtonWidth = 72f;

    private InputAction toggleAction;
    private GameObject canvasObject;
    private bool isPaused;

    private readonly List<Button> tabButtons = new List<Button>();
    private readonly List<GameObject> tabPages = new List<GameObject>();

    private Button[] colorButtons;
    private TextMeshProUGUI[] spellLabels;
    private Image[] spellSwatches;
    private int shownDeliveries = -1;
    private Button walkButton;
    private Button teleportButton;
    private Button smoothTurnButton;
    private Button snapTurnButton;
    private TextMeshProUGUI moveSpeedValue;
    private TextMeshProUGUI turnTitle;
    private TextMeshProUGUI turnValue;
    private TextMeshProUGUI flightSpeedValue;

    private TextMeshProUGUI gameStatus;
    private Button pauseButton;

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
    }

    void OnDisable()
    {
        toggleAction.Disable();

        if (locomotion != null)
            locomotion.Changed -= Refresh;

        SetPaused(false);
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
            SetOpen(!IsOpen);

        // Keep the score and the timer up to date while open.
        if (IsOpen)
            RefreshGame();
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

        // Never leave the game paused with the book closed.
        if (!open)
            SetPaused(false);

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

    // ---------- Actions ----------

    void SelectSpellColor(int index)
    {
        if (!spellCaster.IsUnlocked(index))
            return;

        spellCaster.SetSpellColor(index);
        Refresh();
    }

    void ChangeFlightSpeed(int direction)
    {
        flight.maxSpeed = Mathf.Clamp(
            flight.maxSpeed + direction * flightSpeedStep,
            minFlightSpeed,
            maxFlightSpeed
        );
        Refresh();
    }

    void SetPaused(bool paused)
    {
        if (isPaused == paused)
            return;

        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        RefreshGame();
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------- Refresh ----------

    void Refresh()
    {
        RefreshSpells();

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

        if (flightSpeedValue != null)
            flightSpeedValue.text = $"{flight.maxSpeed:0} m/s";

        RefreshGame();
    }

    // Spell list, one line each: a color swatch, the name and either its
    // effect (unlocked) or the pumpkins still needed (locked; the next
    // one in gold). The chosen spell has a white border and a check.
    void RefreshSpells()
    {
        if (colorButtons == null)
            return;

        shownDeliveries = spellCaster.Deliveries;
        bool nextLockedShown = false;

        for (int i = 0; i < colorButtons.Length; i++)
        {
            SpellType spell = spellCaster.spells[i];
            bool unlocked = spellCaster.IsUnlocked(i);
            bool chosen = i == spellCaster.CurrentColorIndex;

            colorButtons[i].interactable = unlocked;
            colorButtons[i].GetComponent<Outline>().enabled = chosen;
            colorButtons[i].targetGraphic.color = chosen ? SelectedColor
                : unlocked ? ButtonColor : LockedColor;

            // Locked spells show their color dimmed: you see what's coming.
            spellSwatches[i].color = unlocked
                ? spell.color
                : Color.Lerp(spell.color, LockedColor, 0.6f);

            if (unlocked)
            {
                spellLabels[i].text = chosen
                    ? $"<b>{spell.name}</b>  <size=80%>{spell.description}</size>  <b>[ELEGIDO]</b>"
                    : $"<b>{spell.name}</b>  <size=80%>{spell.description}</size>";
            }
            else
            {
                int needed = spell.deliveriesToUnlock;
                int have = Mathf.Min(spellCaster.Deliveries, needed);
                string goal = $"entrega {needed} calabaza{(needed == 1 ? "" : "s")} ({have}/{needed})";

                if (!nextLockedShown)
                    goal = $"<color=#FFD24D>{goal}</color>";
                nextLockedShown = true;

                spellLabels[i].text =
                    $"<color=#9A93A8><b>{spell.name}</b></color>  <size=80%>{goal}</size>";
            }
        }
    }

    void RefreshGame()
    {
        // A delivery while the book is open unlocks spells live.
        if (colorButtons != null && spellCaster.Deliveries != shownDeliveries)
            RefreshSpells();

        if (gameStatus == null)
            return;

        int seconds = Mathf.CeilToInt(game.RemainingTime);
        string stats =
            $"Puntos: {game.TotalScore}\n" +
            $"Energía: {game.CurrentEnergy:0}%\n" +
            $"Tiempo: {seconds / 60}:{seconds % 60:00}";

        if (game.GameOver)
        {
            string result = game.Victory
                ? "¡VICTORIA! Cementerio salvado"
                : "DERROTA: las luces se han apagado";
            gameStatus.text = $"<b>{result}</b>\n{stats}";
            gameStatus.color = game.Victory ? VictoryColor : DefeatColor;
        }
        else
        {
            gameStatus.text = isPaused ? $"<b>EN PAUSA</b>\n{stats}" : stats;
            gameStatus.color = TextColor;
        }

        pauseButton.GetComponentInChildren<TextMeshProUGUI>().text =
            isPaused ? "Reanudar" : "Pausar";
        pauseButton.interactable = !game.GameOver;
    }

    void SelectTab(int index)
    {
        for (int i = 0; i < tabPages.Count; i++)
        {
            tabPages[i].SetActive(i == index);
            SetSelected(tabButtons[i], i == index);
        }
    }

    // Light vibration on the clicking hand.
    public void OnButtonHovered()
    {
        if (haptics != null)
            haptics.SendHapticImpulse(0.08f, 0.02f);
    }

    void OnButtonClicked()
    {
        if (haptics != null)
            haptics.SendHapticImpulse(0.25f, 0.05f);
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

        AddVerticalLayout(panel, 20);

        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        AddText(panel, "Grimorio", 40f, FontStyles.Bold,
            TextAlignmentOptions.Center);

        RectTransform tabRow = AddRow(panel);

        if (spellCaster != null)
            AddTab(panel, tabRow, "Hechizo", BuildSpellSection);

        if ((locomotion != null && locomotion.IsConfigured) || flight != null)
            AddTab(panel, tabRow, "Movimiento", BuildLocomotionSection);

        if (game != null)
            AddTab(panel, tabRow, "Partida", BuildGameSection);

        AddTab(panel, tabRow, "Ayuda", BuildHelpSection);

        AddButton(panel, "Cerrar", ButtonColor, () => SetOpen(false));

        AddText(panel, "Botón X: abrir / cerrar el grimorio",
            20f, FontStyles.Italic, TextAlignmentOptions.Center);

        SelectTab(0);

        // A light vibration on hover and click for every button.
        foreach (Button button in canvasObject.GetComponentsInChildren<Button>(true))
        {
            button.onClick.AddListener(OnButtonClicked);
            button.gameObject.AddComponent<MenuButtonHover>().menu = this;
        }
    }

    // A tab button plus the page it shows. Only the selected page is active.
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
        AddVerticalLayout(page, 0);

        buildPage(page);
        tabPages.Add(page.gameObject);
    }

    void BuildSpellSection(Transform page)
    {
        SpellType[] spells = spellCaster.spells;
        colorButtons = new Button[spells.Length];
        spellLabels = new TextMeshProUGUI[spells.Length];
        spellSwatches = new Image[spells.Length];

        for (int i = 0; i < spells.Length; i++)
        {
            int index = i;
            Button button = AddButton(page, "", ButtonColor,
                () => SelectSpellColor(index));
            button.GetComponent<LayoutElement>().preferredHeight = 50f;

            // Color swatch on the left.
            RectTransform swatch = CreateRect("Color", button.transform);
            swatch.anchorMin = new Vector2(0f, 0.5f);
            swatch.anchorMax = new Vector2(0f, 0.5f);
            swatch.pivot = new Vector2(0f, 0.5f);
            swatch.anchoredPosition = new Vector2(12f, 0f);
            swatch.sizeDelta = new Vector2(30f, 30f);
            Image swatchImage = swatch.gameObject.AddComponent<Image>();
            swatchImage.raycastTarget = false;

            // Kept under the Halloween theme's size limit: plain font,
            // easier to read at this size.
            TextMeshProUGUI label = AddText(button.transform, "", 22f,
                FontStyles.Normal, TextAlignmentOptions.Left);
            label.richText = true;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12f;
            label.fontSizeMax = 22f;

            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(54f, 2f);
            labelRect.offsetMax = new Vector2(-10f, -2f);

            // Highlights the chosen spell.
            Outline outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(4f, -4f);

            colorButtons[i] = button;
            spellLabels[i] = label;
            spellSwatches[i] = swatchImage;
        }

        AddText(page, "Entrega calabazas para desbloquear hechizos.",
            18f, FontStyles.Italic, TextAlignmentOptions.Center);
    }

    void BuildLocomotionSection(Transform page)
    {
        if (locomotion != null && locomotion.IsConfigured)
        {
            AddSectionTitle(page, "Stick izquierdo");
            RectTransform moveRow = AddRow(page);
            walkButton = AddButton(moveRow, "Caminar", ButtonColor,
                () => locomotion.SetWalkEnabled(true));
            teleportButton = AddButton(moveRow, "Teletransporte", ButtonColor,
                () => locomotion.SetWalkEnabled(false));

            AddSectionTitle(page, "Velocidad al caminar");
            moveSpeedValue = AddStepper(page,
                () => locomotion.ChangeMoveSpeed(-1),
                () => locomotion.ChangeMoveSpeed(1));

            AddSectionTitle(page, "Giro");
            RectTransform turnRow = AddRow(page);
            smoothTurnButton = AddButton(turnRow, "Suave", ButtonColor,
                () => locomotion.SetSmoothTurn(true));
            snapTurnButton = AddButton(turnRow, "Por pasos", ButtonColor,
                () => locomotion.SetSmoothTurn(false));

            turnTitle = AddSectionTitle(page, "");
            turnValue = AddStepper(page,
                () => locomotion.ChangeTurn(-1),
                () => locomotion.ChangeTurn(1));
        }

        if (flight != null)
        {
            AddSectionTitle(page, "Velocidad de vuelo");
            flightSpeedValue = AddStepper(page,
                () => ChangeFlightSpeed(-1),
                () => ChangeFlightSpeed(1));
        }
    }

    void BuildGameSection(Transform page)
    {
        AddSectionTitle(page, "Estado de la partida");
        gameStatus = AddText(page, "", 26f, FontStyles.Normal,
            TextAlignmentOptions.Center);

        RectTransform row = AddRow(page);
        pauseButton = AddButton(row, "Pausar", ButtonColor,
            () => SetPaused(!isPaused));
        AddButton(row, "Reiniciar", SelectedColor, RestartGame);
    }

    void BuildHelpSection(Transform page)
    {
        string pumpkinPoints = "";
        if (pumpkinSpawner != null && pumpkinSpawner.pumpkinPrefab != null &&
            pumpkinSpawner.pumpkinPrefab.TryGetComponent(out PumpkinItem pumpkin))
        {
            pumpkinPoints = $"+{pumpkin.pointValue} puntos y ";
        }

        string help =
            "<b>OBJETIVO</b>\n" +
            "Lleva las calabazas levitando hasta el altar iluminado y " +
            "elimina a los duendes antes de que las destruyan.\n";

        if (game != null)
        {
            help +=
                $"Mantén la energía por encima del {game.minimumVictoryEnergy:0}% " +
                "hasta que acabe el tiempo.\n\n" +
                "<b>PUNTOS</b>\n" +
                $"Calabaza entregada: {pumpkinPoints}+{game.energyPerDelivery:0}% energía " +
                "(más si es grande)\n" +
                $"Duende eliminado: +{game.pointsPerGoblin} puntos\n" +
                $"Calabaza destruida: −{game.energyLostPerPumpkin:0}% energía\n";
        }

        help +=
            "\n<b>CONTROLES</b>\n" +
            "Gatillo izq.: levitar / soltar\n" +
            "Estirar / recoger el brazo: alejar / acercar el objeto\n" +
            "Y + girar la muñeca: rotar el objeto levitado\n" +
            "A / B: agrandar / encoger\n" +
            "Gatillo der.: lanzar hechizo\n" +
            "Grip der.: volar hacia donde apunta la mano\n" +
            "Stick izq.: moverse · Stick der.: girar / teletransporte\n" +
            "X: abrir / cerrar el grimorio";

        AddText(page, help, 20f, FontStyles.Normal,
            TextAlignmentOptions.Left);
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

    // ---------- UI helpers ----------

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

    static void AddVerticalLayout(RectTransform rect, int padding)
    {
        VerticalLayoutGroup layout =
            rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
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
