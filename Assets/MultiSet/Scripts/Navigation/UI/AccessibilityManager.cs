using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
    [Header("Toggles")]// main toggles for accessibility buttons
    [SerializeField] private Toggle bigTextToggle;
    [SerializeField] private Toggle contrastToggle;

    [Header("Target Canvas (auto-found if empty)")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Big Text settings")]
    [Tooltip("Multiplier for TMP and uGUI Text font sizes (and auto-size / best-fit bounds) when Big Text is on.")]
    [SerializeField] private float bigTextScale = 2.0f;

    [Header("Contrast settings (light-theme UI: near-black text, crisp white/light surfaces)")]
    [SerializeField] private Color contrastTextColor = new Color(0.02f, 0.02f, 0.05f, 1f);
    [Tooltip("Target fill for light panel images in high-contrast mode (opaque white by default).")]
    [SerializeField] private Color contrastBgColor = Color.white;

    private static readonly int ShaderColorId = Shader.PropertyToID("_Color");
    private static readonly int ShaderIntensityId = Shader.PropertyToID("_Intensity");

    private TMP_Text[] uiTexts;
    private Text[] uiLegacyTexts;
    private Image[] uiBackgrounds;
    private float[] originalFontSizes;
    private float[] originalFontSizeMins;
    private float[] originalFontSizeMaxs;
    private bool[] originalEnableAutoSizing;
    private Color[] originalTextColors;
    private float[] originalLegacyFontSizes;
    private int[] originalLegacyResizeMin;
    private int[] originalLegacyResizeMax;
    private bool[] originalLegacyBestFit;
    private Color[] originalLegacyTextColors;
    private Color[] originalBgColors;

    private Material _navPathMaterial;
    private Color _navPathColorOriginal;
    private float _navPathIntensityOriginal;
    private bool _navPathVisualCached;
    private bool _navPathHasColorProperty;
    private bool _navPathHasIntensityProperty;

    private bool bigTextActive;
    private bool contrastActive;

    private bool initialized;

    void Start()
    {
        TryInitializeCore();
    }

    void OnDestroy()
    {
        UnsubscribeToggles();
    }

    /// <summary>
    /// Runtime wiring for accessibility controls. Returns false if the UI canvas or toggles could not be resolved.
    /// </summary>
    public bool Bind(Toggle bigText, Toggle contrast, Canvas canvas = null)
    {
        UnsubscribeToggles();

        bigTextToggle = bigText;
        contrastToggle = contrast;

        var priorCanvas = targetCanvas;
        if (canvas != null)
        {
            targetCanvas = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
        }

        // Re-opening the sheet calls Bind again. Re-caching font sizes would snapshot already-scaled text
        // and multiply the big-text factor again — keep the first baseline for the same root canvas.
        var reuseBaseline =
            initialized &&
            uiTexts != null &&
            priorCanvas != null &&
            targetCanvas == priorCanvas;

        if (reuseBaseline)
        {
            return SubscribeToggles();
        }

        _navPathVisualCached = false;
        _navPathMaterial = null;

        initialized = false;
        return TryInitializeCore();
    }

    private bool TryInitializeCore()
    {
        if (initialized)
        {
            return true;
        }

        ResolveReferences();
        if (!CacheCanvasContent())
        {
            return false;
        }

        if (!SubscribeToggles())
        {
            return false;
        }

        initialized = true;
        return true;
    }

    private void UnsubscribeToggles()
    {
        if (bigTextToggle != null)
        {
            bigTextToggle.onValueChanged.RemoveListener(OnBigTextChanged);
        }
        if (contrastToggle != null)
        {
            contrastToggle.onValueChanged.RemoveListener(OnContrastChanged);
        }
    }

    private void ResolveReferences()
    {
        if (bigTextToggle == null)
        {
            GameObject obj = GameObject.Find("BigTextT");
            if (obj != null)
            {
                bigTextToggle = obj.GetComponent<Toggle>();
            }
        }

        if (contrastToggle == null)
        {
            GameObject obj = GameObject.Find("ContrastT");
            if (obj != null)
            {
                contrastToggle = obj.GetComponent<Toggle>();
            }
        }

        if (targetCanvas == null)
        {
            var fromToggle = bigTextToggle != null
                ? bigTextToggle.GetComponentInParent<Canvas>(true)
                : contrastToggle != null
                    ? contrastToggle.GetComponentInParent<Canvas>(true)
                    : null;
            if (fromToggle != null)
            {
                targetCanvas = fromToggle.rootCanvas != null ? fromToggle.rootCanvas : fromToggle;
            }
        }

        if (targetCanvas == null)
        {
            var obj = GameObject.Find("Canvas");
            if (obj != null)
            {
                var c = obj.GetComponent<Canvas>();
                if (c != null)
                {
                    targetCanvas = c.rootCanvas != null ? c.rootCanvas : c;
                }
            }
        }

        if (targetCanvas == null)
        {
            targetCanvas = PickRootCanvasWithMostTextGraphics();
        }

        if (targetCanvas == null)
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
        }
    }

    /// <summary>
    /// Prefer the root canvas that hosts the most text (TMP + legacy <see cref="Text"/>), so scenes that still use uGUI Text are covered.
    /// </summary>
    private static Canvas PickRootCanvasWithMostTextGraphics()
    {
        Canvas best = null;
        var bestCount = 0;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c == null)
            {
                continue;
            }

            var root = c.rootCanvas != null ? c.rootCanvas : c;
            var n = root.GetComponentsInChildren<TMP_Text>(true).Length +
                    root.GetComponentsInChildren<Text>(true).Length;
            if (n > bestCount)
            {
                bestCount = n;
                best = root;
            }
        }

        return best;
    }

    private bool CacheCanvasContent()
    {
        if (targetCanvas == null)
        {
            Debug.LogError("AccessibilityManager: Canvas not found.");
            return false;
        }

        uiTexts = targetCanvas.GetComponentsInChildren<TMP_Text>(true);
        uiLegacyTexts = targetCanvas.GetComponentsInChildren<Text>(true);
        uiBackgrounds = CollectImagesForContrastRecolor(targetCanvas);

        originalFontSizes = new float[uiTexts.Length];
        originalFontSizeMins = new float[uiTexts.Length];
        originalFontSizeMaxs = new float[uiTexts.Length];
        originalEnableAutoSizing = new bool[uiTexts.Length];
        originalTextColors = new Color[uiTexts.Length];
        for (int i = 0; i < uiTexts.Length; i++)
        {
            if (uiTexts[i] == null)
            {
                continue;
            }

            originalFontSizes[i] = uiTexts[i].fontSize;
            originalFontSizeMins[i] = uiTexts[i].fontSizeMin;
            originalFontSizeMaxs[i] = uiTexts[i].fontSizeMax;
            originalEnableAutoSizing[i] = uiTexts[i].enableAutoSizing;
            originalTextColors[i] = uiTexts[i].color;
        }

        originalLegacyFontSizes = new float[uiLegacyTexts.Length];
        originalLegacyResizeMin = new int[uiLegacyTexts.Length];
        originalLegacyResizeMax = new int[uiLegacyTexts.Length];
        originalLegacyBestFit = new bool[uiLegacyTexts.Length];
        originalLegacyTextColors = new Color[uiLegacyTexts.Length];
        for (int i = 0; i < uiLegacyTexts.Length; i++)
        {
            var t = uiLegacyTexts[i];
            if (t == null)
            {
                continue;
            }

            originalLegacyFontSizes[i] = t.fontSize;
            originalLegacyResizeMin[i] = t.resizeTextMinSize;
            originalLegacyResizeMax[i] = t.resizeTextMaxSize;
            originalLegacyBestFit[i] = t.resizeTextForBestFit;
            originalLegacyTextColors[i] = t.color;
        }

        originalBgColors = new Color[uiBackgrounds.Length];
        for (int i = 0; i < uiBackgrounds.Length; i++)
        {
            if (uiBackgrounds[i] == null)
            {
                continue;
            }
            originalBgColors[i] = uiBackgrounds[i].color;
        }

        return true;
    }

    /// <summary>
    /// High-contrast mode recolors large panel <see cref="Image"/> fills. Skips <see cref="Image"/> under
    /// <see cref="Selectable"/> so circular icon buttons (Capture, destinations, STOP, etc.) keep their artwork.
    /// </summary>
    private static Image[] CollectImagesForContrastRecolor(Canvas canvas)
    {
        var all = canvas.GetComponentsInChildren<Image>(true);
        var keep = new List<Image>(all.Length);
        for (var i = 0; i < all.Length; i++)
        {
            var img = all[i];
            if (img == null || SkipContrastRecolorForImage(img))
            {
                continue;
            }

            keep.Add(img);
        }

        return keep.ToArray();
    }

    private static bool SkipContrastRecolorForImage(Image image)
    {
        return image.GetComponentInParent<Selectable>(true) != null;
    }

    private bool SubscribeToggles()
    {
        UnsubscribeToggles();

        if (bigTextToggle == null || contrastToggle == null)
        {
            Debug.LogError("AccessibilityManager: Big text or contrast toggle is null; cannot subscribe.");
            return false;
        }

        bigTextToggle.onValueChanged.AddListener(OnBigTextChanged);
        bigTextActive = false;
        if (bigTextToggle.isOn)
        {
            ApplyBigText(true);
        }

        contrastToggle.onValueChanged.AddListener(OnContrastChanged);
        contrastActive = false;
        if (contrastToggle.isOn)
        {
            ApplyContrast(true);
        }

        return true;
    }

    private void OnBigTextChanged(bool isOn)
    {
        ApplyBigText(isOn);
    }

    private void OnContrastChanged(bool isOn)
    {
        ApplyContrast(isOn);
    }

    private void ApplyBigText(bool isOn)
    {
        if (isOn == bigTextActive)
        {
            return;
        }

        var tmpReady = TmpBigTextCachesReady();
        var legacyReady = LegacyBigTextCachesReady();
        if (!tmpReady && !legacyReady)
        {
            return;
        }

        bigTextActive = isOn;

        float bigScale = bigTextScale > 0f ? bigTextScale : 1f;
        float scale = isOn ? bigScale : 1f;

        if (tmpReady)
        {
            for (int i = 0; i < uiTexts.Length; i++)
            {
                var tmp = uiTexts[i];
                if (tmp == null)
                {
                    continue;
                }

                if (isOn)
                {
                    if (originalEnableAutoSizing[i])
                    {
                        var scaledMin = Mathf.Max(1f, originalFontSizeMins[i] * scale);
                        var scaledMax = Mathf.Max(scaledMin + 1f, originalFontSizeMaxs[i] * scale);
                        tmp.fontSizeMin = scaledMin;
                        tmp.fontSizeMax = scaledMax;
                        tmp.fontSize = Mathf.Clamp(originalFontSizes[i] * scale, scaledMin, scaledMax);
                    }
                    else
                    {
                        tmp.fontSize = originalFontSizes[i] * scale;
                    }
                }
                else
                {
                    tmp.fontSizeMin = originalFontSizeMins[i];
                    tmp.fontSizeMax = originalFontSizeMaxs[i];
                    tmp.fontSize = originalFontSizes[i];
                }

                tmp.ForceMeshUpdate(true);
            }
        }

        if (legacyReady)
        {
            for (int i = 0; i < uiLegacyTexts.Length; i++)
            {
                var t = uiLegacyTexts[i];
                if (t == null)
                {
                    continue;
                }

                if (isOn)
                {
                    if (originalLegacyBestFit[i])
                    {
                        t.resizeTextForBestFit = true;
                        var scaledMin = Mathf.Max(1, Mathf.RoundToInt(originalLegacyResizeMin[i] * scale));
                        var scaledMax = Mathf.Max(scaledMin + 1, Mathf.RoundToInt(originalLegacyResizeMax[i] * scale));
                        t.resizeTextMinSize = scaledMin;
                        t.resizeTextMaxSize = scaledMax;
                        var scaledFont = Mathf.RoundToInt(originalLegacyFontSizes[i] * scale);
                        t.fontSize = Mathf.Clamp(scaledFont, scaledMin, scaledMax);
                    }
                    else
                    {
                        t.fontSize = Mathf.Max(1, Mathf.RoundToInt(originalLegacyFontSizes[i] * scale));
                    }
                }
                else
                {
                    t.resizeTextMinSize = originalLegacyResizeMin[i];
                    t.resizeTextMaxSize = originalLegacyResizeMax[i];
                    t.resizeTextForBestFit = originalLegacyBestFit[i];
                    t.fontSize = Mathf.RoundToInt(originalLegacyFontSizes[i]);
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        if (targetCanvas != null && targetCanvas.transform is RectTransform canvasRt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRt);
        }
    }

    private bool TmpBigTextCachesReady()
    {
        return uiTexts != null && originalFontSizes != null && originalEnableAutoSizing != null &&
               originalFontSizeMins != null && originalFontSizeMaxs != null &&
               originalFontSizes.Length == uiTexts.Length && originalFontSizeMins.Length == uiTexts.Length &&
               originalFontSizeMaxs.Length == uiTexts.Length && originalEnableAutoSizing.Length == uiTexts.Length;
    }

    private bool LegacyBigTextCachesReady()
    {
        return uiLegacyTexts != null && originalLegacyFontSizes != null && originalLegacyResizeMin != null &&
               originalLegacyResizeMax != null && originalLegacyBestFit != null &&
               originalLegacyFontSizes.Length == uiLegacyTexts.Length &&
               originalLegacyResizeMin.Length == uiLegacyTexts.Length &&
               originalLegacyResizeMax.Length == uiLegacyTexts.Length &&
               originalLegacyBestFit.Length == uiLegacyTexts.Length;
    }

    private void ApplyContrast(bool isOn)
    {
        if (isOn == contrastActive)
        {
            return;
        }
        contrastActive = isOn;

        if (uiTexts != null && originalTextColors != null && originalTextColors.Length == uiTexts.Length)
        {
            for (int i = 0; i < uiTexts.Length; i++)
            {
                if (uiTexts[i] == null)
                {
                    continue;
                }

                if (isOn)
                {
                    var o = originalTextColors[i];
                    uiTexts[i].color = new Color(contrastTextColor.r, contrastTextColor.g, contrastTextColor.b, Mathf.Max(o.a, 0.92f));
                }
                else
                {
                    uiTexts[i].color = originalTextColors[i];
                }
            }
        }

        if (uiLegacyTexts != null && originalLegacyTextColors != null &&
            originalLegacyTextColors.Length == uiLegacyTexts.Length)
        {
            for (int i = 0; i < uiLegacyTexts.Length; i++)
            {
                var t = uiLegacyTexts[i];
                if (t == null)
                {
                    continue;
                }

                if (isOn)
                {
                    var o = originalLegacyTextColors[i];
                    t.color = new Color(contrastTextColor.r, contrastTextColor.g, contrastTextColor.b, Mathf.Max(o.a, 0.92f));
                }
                else
                {
                    t.color = originalLegacyTextColors[i];
                }
            }
        }

        if (uiBackgrounds != null && originalBgColors != null && originalBgColors.Length == uiBackgrounds.Length)
        {
            for (int i = 0; i < uiBackgrounds.Length; i++)
            {
                if (uiBackgrounds[i] == null)
                {
                    continue;
                }

                if (isOn)
                {
                    uiBackgrounds[i].color = ComputeHighContrastImageColor(originalBgColors[i]);
                }
                else
                {
                    uiBackgrounds[i].color = originalBgColors[i];
                }
            }
        }

        ApplyNavPathHighContrast(isOn);
    }

    private void CacheNavPathVisualIfNeeded()
    {
        if (_navPathVisualCached)
        {
            return;
        }

        if (ShowPath.instance == null)
        {
            return;
        }

        var lr = ShowPath.instance.GetComponent<LineRenderer>();
        if (lr == null)
        {
            return;
        }

        _navPathMaterial = lr.material;
        if (_navPathMaterial == null)
        {
            return;
        }

        _navPathHasColorProperty = _navPathMaterial.HasProperty(ShaderColorId);
        if (_navPathHasColorProperty)
        {
            _navPathColorOriginal = _navPathMaterial.GetColor(ShaderColorId);
        }

        _navPathHasIntensityProperty = _navPathMaterial.HasProperty(ShaderIntensityId);
        if (_navPathHasIntensityProperty)
        {
            _navPathIntensityOriginal = _navPathMaterial.GetFloat(ShaderIntensityId);
        }

        _navPathVisualCached = true;
    }

    /// <summary>
    /// High-contrast mode: navigation path stays purple but reads clearly on bright UI surfaces.
    /// </summary>
    private void ApplyNavPathHighContrast(bool isOn)
    {
        CacheNavPathVisualIfNeeded();
        if (_navPathMaterial == null)
        {
            return;
        }

        if (isOn)
        {
            if (_navPathHasColorProperty)
            {
                _navPathMaterial.SetColor(ShaderColorId, new Color(0.95f, 0.55f, 1f, 1f));
            }

            if (_navPathHasIntensityProperty)
            {
                _navPathMaterial.SetFloat(ShaderIntensityId, Mathf.Max(_navPathIntensityOriginal * 1.45f, 4.5f));
            }
        }
        else
        {
            if (_navPathHasColorProperty)
            {
                _navPathMaterial.SetColor(ShaderColorId, _navPathColorOriginal);
            }

            if (_navPathHasIntensityProperty)
            {
                _navPathMaterial.SetFloat(ShaderIntensityId, _navPathIntensityOriginal);
            }
        }
    }

    /// <summary>
    /// Maps washed-out light fills to opaque white (or near-white) so body text can be near-black with strong separation.
    /// </summary>
    private Color ComputeHighContrastImageColor(Color original)
    {
        var lum = 0.299f * original.r + 0.587f * original.g + 0.114f * original.b;
        var a = original.a;

        if (a < 0.04f)
        {
            return original;
        }

        if (lum >= 0.72f)
        {
            return new Color(contrastBgColor.r, contrastBgColor.g, contrastBgColor.b, Mathf.Max(a, 0.98f));
        }

        if (lum <= 0.22f)
        {
            return new Color(0.94f, 0.95f, 0.97f, Mathf.Max(a, 0.96f));
        }

        return new Color(1f, 1f, 1f, Mathf.Max(a, 0.94f));
    }
}
