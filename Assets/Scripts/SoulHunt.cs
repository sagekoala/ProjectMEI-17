using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

// Mini-game: banish every wandering soul in the graveyard with spells,
// as fast as possible. The best time is kept between sessions.
public class SoulHunt : MonoBehaviour
{
    const string BestTimeKey = "SoulHunt.BestTime";

    [Header("Souls")]
    public int soulCount = 8;
    public float soulSize = 0.5f;

    [Tooltip("Ghostly material, e.g. XRI's FresnelHighlight (uses _BaseColor and _RimColor).")]
    public Material soulMaterial;
    public Color soulColor = new Color(0.6f, 0.9f, 1f, 0.35f);
    public Color soulRimColor = new Color(0.8f, 1f, 1f, 1f);

    [Header("Area (world space)")]
    public Vector3 areaCenter = new Vector3(5f, 2.5f, -21.5f);
    public Vector3 areaSize = new Vector3(17f, 2f, 16f);

    [Header("HUD")]
    [Tooltip("The hunt status floats above this transform (the left controller).")]
    public Transform hudAnchor;
    public Vector3 hudOffset = new Vector3(0f, 0.03f, 0f);
    public float resultDisplaySeconds = 6f;

    public event Action Changed;

    public bool IsRunning { get; private set; }
    public int Remaining => souls.Count;
    public int Total => soulCount;

    public float ElapsedTime =>
        IsRunning ? Time.time - startTime : lastTime;

    // Negative when there is no record yet.
    public float BestTime => PlayerPrefs.GetFloat(BestTimeKey, -1f);

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int RimColorId = Shader.PropertyToID("_RimColor");

    private readonly List<WanderingSoul> souls = new List<WanderingSoul>();
    private float startTime;
    private float lastTime;

    private GameObject hudObject;
    private TextMeshProUGUI hudText;
    private string resultMessage = "";
    private float hideResultTime;

    void Awake()
    {
        BuildHud();
    }

    void OnDestroy()
    {
        ClearSouls();
    }

    void LateUpdate()
    {
        UpdateHud();
    }

    public void StartHunt()
    {
        ClearSouls();

        for (int i = 0; i < soulCount; i++)
            SpawnSoul();

        startTime = Time.time;
        IsRunning = true;
        hideResultTime = 0f;

        Changed?.Invoke();
    }

    public Vector3 RandomPointInArea()
    {
        Vector3 random = new Vector3(
            UnityEngine.Random.value - 0.5f,
            UnityEngine.Random.value - 0.5f,
            UnityEngine.Random.value - 0.5f
        );

        return areaCenter + Vector3.Scale(areaSize, random);
    }

    // Called by a soul when a spell hits it.
    public void OnSoulBanished(WanderingSoul soul)
    {
        if (!souls.Remove(soul))
            return;

        Vector3 position = soul.transform.position;

        MagicProjectile.SpawnFlash(position, soulSize * 2.5f, soulRimColor, 0.4f);
        MagicFeedback.Play(MagicFeedback.ChimeSound, position, 1f);

        Destroy(soul.gameObject);

        if (IsRunning && souls.Count == 0)
            Finish();

        Changed?.Invoke();
    }

    public static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        return $"{total / 60}:{total % 60:00}";
    }

    void Finish()
    {
        IsRunning = false;
        lastTime = Time.time - startTime;

        float best = BestTime;
        bool isRecord = best < 0f || lastTime < best;

        if (isRecord)
        {
            PlayerPrefs.SetFloat(BestTimeKey, lastTime);
            PlayerPrefs.Save();
        }

        resultMessage =
            $"¡Cementerio purificado!\n{FormatTime(lastTime)}" +
            (isRecord ? "  ¡Récord!" : "");
        hideResultTime = Time.time + resultDisplaySeconds;
    }

    void SpawnSoul()
    {
        GameObject soul = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        soul.name = "Wandering Soul";
        soul.transform.position = RandomPointInArea();
        soul.transform.localScale = Vector3.one * soulSize;

        // A trigger: spells hit it, but it never blocks the player.
        soul.GetComponent<Collider>().isTrigger = true;

        // Moving trigger colliders should have a kinematic body.
        Rigidbody body = soul.AddComponent<Rigidbody>();
        body.isKinematic = true;

        Renderer renderer = soul.GetComponent<Renderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;

        if (soulMaterial != null)
        {
            renderer.sharedMaterial = soulMaterial;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, soulColor);
            block.SetColor(RimColorId, soulRimColor);
            renderer.SetPropertyBlock(block);
        }
        else
        {
            renderer.material.color = soulColor;
        }

        WanderingSoul wandering = soul.AddComponent<WanderingSoul>();
        wandering.hunt = this;
        souls.Add(wandering);
    }

    void ClearSouls()
    {
        foreach (WanderingSoul soul in souls)
        {
            if (soul != null)
                Destroy(soul.gameObject);
        }

        souls.Clear();
        IsRunning = false;
    }

    // ---------- HUD ----------

    void BuildHud()
    {
        hudObject = new GameObject("Soul Hunt HUD", typeof(RectTransform));
        hudObject.transform.SetParent(transform, false);
        hudObject.transform.localScale = Vector3.one * 0.0004f;

        Canvas canvas = hudObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        ((RectTransform)hudObject.transform).sizeDelta = new Vector2(500f, 120f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(hudObject.transform, false);

        hudText = textObject.AddComponent<TextMeshProUGUI>();
        hudText.fontSize = 34f;
        hudText.fontStyle = FontStyles.Bold;
        hudText.alignment = TextAlignmentOptions.Center;
        hudText.color = soulRimColor;
        hudText.raycastTarget = false;

        RectTransform textRect = hudText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        hudObject.SetActive(false);
    }

    void UpdateHud()
    {
        bool showResult = !IsRunning && Time.time < hideResultTime;
        bool visible = hudAnchor != null && (IsRunning || showResult);

        hudObject.SetActive(visible);
        if (!visible)
            return;

        hudText.text = IsRunning
            ? $"Almas: {Remaining}/{Total}   {FormatTime(ElapsedTime)}"
            : resultMessage;

        Vector3 position = hudAnchor.position + hudOffset;
        hudObject.transform.position = position;

        // Face away from the head, like the wizard menu.
        Camera head = Camera.main;
        if (head != null)
        {
            Vector3 away = position - head.transform.position;
            if (away.sqrMagnitude > 0.0001f)
            {
                hudObject.transform.rotation =
                    Quaternion.LookRotation(away, Vector3.up);
            }
        }
    }
}
