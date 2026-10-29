using UnityEngine;
using UnityEngine.UI;

// Damage feedback for an enemy: a health bar floating above it (always
// facing the player, green to red) and a short red flash on its body
// when it is hit. Added in code by GoblinEnemy.
public class GoblinHealthBar : MonoBehaviour
{
    public float barWidth = 0.5f;
    public float heightAboveTop = 0.25f;
    public float flashDuration = 0.12f;

    static readonly Color FullColor = new Color(0.35f, 1f, 0.35f);
    static readonly Color LowColor = new Color(1f, 0.25f, 0.2f);
    static readonly Color BackColor = new Color(0f, 0f, 0f, 0.6f);
    static readonly Color FlashColor = new Color(1f, 0.2f, 0.2f);

    // URP Lit uses _BaseColor; built-in shaders use _Color.
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    const float WidthPixels = 100f;
    const float HeightPixels = 12f;

    private Renderer[] bodyRenderers;
    private MaterialPropertyBlock propertyBlock;
    private GameObject bar;
    private RectTransform fill;
    private Image fillImage;
    private float flashEndTime;
    private bool isFlashing;

    void Awake()
    {
        // Before the bar exists, so only the body flashes.
        bodyRenderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        BuildBar();
        SetHealth(1f);
    }

    void OnDestroy()
    {
        if (bar != null)
            Destroy(bar);
    }

    // fraction: 1 = full health, 0 = dead.
    public void SetHealth(float fraction)
    {
        fraction = Mathf.Clamp01(fraction);
        fill.anchorMax = new Vector2(fraction, 1f);
        fillImage.color = Color.Lerp(LowColor, FullColor, fraction);
    }

    public void Flash()
    {
        flashEndTime = Time.time + flashDuration;
        SetTint(true);
    }

    void LateUpdate()
    {
        if (isFlashing && Time.time >= flashEndTime)
            SetTint(false);

        PlaceBar();
    }

    void SetTint(bool on)
    {
        isFlashing = on;

        foreach (Renderer body in bodyRenderers)
        {
            if (body == null)
                continue;

            if (!on)
            {
                body.SetPropertyBlock(null);
                continue;
            }

            body.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, FlashColor);
            propertyBlock.SetColor(ColorId, FlashColor);
            body.SetPropertyBlock(propertyBlock);
        }
    }

    void PlaceBar()
    {
        float top = transform.position.y + 1f;
        if (bodyRenderers.Length > 0 && bodyRenderers[0] != null)
            top = bodyRenderers[0].bounds.max.y;

        Vector3 position = new Vector3(
            transform.position.x, top + heightAboveTop, transform.position.z
        );
        bar.transform.position = position;

        // Face away from the player's head, like the other world UIs.
        Camera head = Camera.main;
        if (head == null)
            return;

        Vector3 away = position - head.transform.position;
        away.y = 0f;
        if (away.sqrMagnitude > 0.0001f)
            bar.transform.rotation = Quaternion.LookRotation(away, Vector3.up);
    }

    // Not parented to the enemy: its scale and turning would distort it.
    void BuildBar()
    {
        bar = new GameObject("Health Bar", typeof(RectTransform));
        bar.transform.localScale = Vector3.one * (barWidth / WidthPixels);

        Canvas canvas = bar.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        ((RectTransform)bar.transform).sizeDelta =
            new Vector2(WidthPixels, HeightPixels);

        Image back = CreateImage("Background", bar.transform, BackColor);
        Stretch(back.rectTransform);

        fillImage = CreateImage("Fill", back.transform, FullColor);
        fill = fillImage.rectTransform;
        Stretch(fill);
    }

    static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
