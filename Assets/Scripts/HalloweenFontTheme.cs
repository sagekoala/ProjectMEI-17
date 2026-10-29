using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Gives the game's texts a Halloween look: a spooky font (Creepster) on
// every TextMeshPro text of the scene, including the ones the grimoire
// and the HUD build in code, and the 3D texts in the world. Small
// interface texts keep the default font, because a decorative font is
// hard to read at small sizes in VR.
public class HalloweenFontTheme : MonoBehaviour
{
    [Tooltip("Spooky font file (.ttf) with 'Include Font Data' enabled.")]
    public Font font;

    [Tooltip("Interface (canvas) texts smaller than this keep the default font, " +
             "for readability. 3D world texts always get the spooky font.")]
    public float minFontSize = 24f;

    private TMP_FontAsset fontAsset;

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
    }

    IEnumerator Start()
    {
        if (fontAsset == null)
            yield break;

        // Wait one frame: the grimoire and the HUD create their texts in
        // their own Start/Awake.
        yield return null;

        TMP_Text[] texts = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include, FindObjectsSortMode.None
        );

        foreach (TMP_Text text in texts)
        {
            if (UsesSpookyFont(text))
                text.font = fontAsset;
        }
    }

    bool UsesSpookyFont(TMP_Text text)
    {
        // 3D texts (e.g. the delivery sign) measure size in other units,
        // so the size limit only applies to canvas texts.
        if (!(text is TextMeshProUGUI))
            return true;

        // Auto-sized texts (buttons) are judged by their largest size.
        float size = text.enableAutoSizing ? text.fontSizeMax : text.fontSize;
        return size >= minFontSize;
    }

    void OnDestroy()
    {
        if (fontAsset != null)
            Destroy(fontAsset);
    }
}
