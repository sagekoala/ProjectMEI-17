using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Sets the Creepster font on every text, whatever its size, including
// texts created later (they can call Apply; a periodic check catches the rest)
public class HalloweenFontTheme : MonoBehaviour
{
    [Tooltip("Spooky font file (.ttf) with 'Include Font Data' enabled.")]
    public Font font;

    [Tooltip("Seconds between checks for new texts without the spooky font")]
    public float recheckInterval = 1f;

    private TMP_FontAsset fontAsset;
    private float nextCheckTime;

    // reused lists, so the periodic check doesn't allocate (GC hitches on Quest)
    private readonly List<GameObject> roots = new List<GameObject>();
    private readonly List<TMP_Text> texts = new List<TMP_Text>();

    // The spooky font, for texts created later in the game (null if none).
    public static TMP_FontAsset Current { get; private set; }

    // Gives a text the spooky font right away, so it never shows the
    // default font for a moment. Does nothing if there is no theme.
    public static void Apply(TMP_Text text)
    {
        if (Current != null && text != null && text.font != Current)
            text.font = Current;
    }

    void Awake()
    {
        if (font == null)
        {
            Debug.LogWarning("HalloweenFontTheme has no font assigned.", this);
            return;
        }

        // A dynamic font asset: glyphs are added as they are needed.
        fontAsset = TMP_FontAsset.CreateFontAsset(font);

        // Any character the font lacks comes from the default font.
        if (TMP_Settings.defaultFontAsset != null)
        {
            if (fontAsset.fallbackFontAssetTable == null)
                fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();

            fontAsset.fallbackFontAssetTable.Add(TMP_Settings.defaultFontAsset);
        }

        Current = fontAsset;
    }

    IEnumerator Start()
    {
        if (fontAsset == null)
            yield break;

        // wait a frame, other UI is created in Start
        yield return null;
        ApplyToAllTexts();
    }

    void Update()
    {
        // Unscaled: keeps working while the game is paused.
        if (fontAsset == null || Time.unscaledTime < nextCheckTime)
            return;

        nextCheckTime = Time.unscaledTime + recheckInterval;
        ApplyToAllTexts();
    }

    // Every text in the loaded scenes, including inactive ones.
    void ApplyToAllTexts()
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;

            scene.GetRootGameObjects(roots);
            foreach (GameObject root in roots)
            {
                root.GetComponentsInChildren(true, texts);
                foreach (TMP_Text text in texts)
                {
                    if (text.font != fontAsset)
                        text.font = fontAsset;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (Current == fontAsset)
            Current = null;

        if (fontAsset != null)
            Destroy(fontAsset);
    }
}
