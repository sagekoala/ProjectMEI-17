
using TMPro;
using UnityEngine;

// Floating text that rises, faces the camera and fades out
public class FloatingText : MonoBehaviour
{
    public float duration = 1.6f;
    public float riseDistance = 0.5f;

    private TextMeshPro label;
    private Color baseColor;
    private Vector3 startPosition;
    private float startTime;
    private Transform viewer;

    public static FloatingText Spawn(Vector3 position, string text, Color color)
    {
        GameObject go = new GameObject("Floating Text");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.1f;

        TextMeshPro label = go.AddComponent<TextMeshPro>();
        HalloweenFontTheme.Apply(label);
        label.text = text;
        label.fontSize = 14f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.color = color;
        label.rectTransform.sizeDelta = new Vector2(20f, 5f);

        FloatingText floating = go.AddComponent<FloatingText>();
        floating.label = label;
        floating.baseColor = color;
        return floating;
    }

    void Start()
    {
        startPosition = transform.position;
        startTime = Time.time;

        if (Camera.main != null)
            viewer = Camera.main.transform;

        FaceViewer();
    }

    void LateUpdate()
    {
        float t = (Time.time - startTime) / duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = startPosition + Vector3.up * (riseDistance * t);

        Color color = baseColor;
        color.a = 1f - t * t;
        label.color = color;

        FaceViewer();
    }

    void FaceViewer()
    {
        if (viewer == null)
            return;

        Vector3 away = transform.position - viewer.position;
        if (away.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(away, Vector3.up);
    }
}
