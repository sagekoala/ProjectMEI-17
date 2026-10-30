using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Sets the Creepster font on big texts
public class HalloweenFontTheme : MonoBehaviour
{
    [Tooltip("Spooky font file (.ttf) with 'Include Font Data' enabled.")]
    public Font font;

    [Tooltip("Canvas texts smaller than this keep the default font")]
    public float minFontSize = 24f;

    private TMP_FontAsset fontAsset;

    // The spooky font, for texts created later in the game (null if none).
    public static TMP_FontAsset Current { get; private set; }

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

        // Every text in the loaded scenes, including inactive ones.
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (UsesSpookyFont(text))
                        text.font = fontAsset;
                }
            }
        }
    }

    bool UsesSpookyFont(TMP_Text text)
    {
        // size limit only for canvas texts
        if (!(text is TextMeshProUGUI))
            return true;

        // Auto-sized texts (buttons) are judged by their largest size.
        float size = text.enableAutoSizing ? text.fontSizeMax : text.fontSize;
        return size >= minFontSize;
    }

    void OnDestroy()
    {
        if (Current == fontAsset)
            Current = null;

        if (fontAsset != null)
            Destroy(fontAsset);
    }
}
