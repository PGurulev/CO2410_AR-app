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
    [Tooltip("Multiplier applied to every TMP_Text fontSize when Big Text is on.")]
    [SerializeField] private float bigTextScale = 1.5f;

    [Header("Contrast settings")]
    [SerializeField] private Color contrastTextColor = Color.yellow;
    [SerializeField] private Color contrastBgColor = Color.black;

    private TMP_Text[] uiTexts;
    private Image[] uiBackgrounds;
    private float[] originalFontSizes;
    private Color[] originalTextColors;
    private Color[] originalBgColors;

    private bool bigTextActive;
    private bool contrastActive;

    void Start()
    {
        ResolveReferences();
        CacheCanvasContent();
        SubscribeToggles();
    }

    void OnDestroy()
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
            GameObject obj = GameObject.Find("Canvas");
            if (obj != null)
            {
                targetCanvas = obj.GetComponent<Canvas>();
            }
        }

        if (targetCanvas == null)
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
        }
    }

    private void CacheCanvasContent()
    {
        if (targetCanvas == null)
        {
            Debug.LogError("AccessibilityManager: Canvas not found.");
            return;
        }

        uiTexts = targetCanvas.GetComponentsInChildren<TMP_Text>(true);
        uiBackgrounds = targetCanvas.GetComponentsInChildren<Image>(true);

        originalFontSizes = new float[uiTexts.Length];
        originalTextColors = new Color[uiTexts.Length];
        for (int i = 0; i < uiTexts.Length; i++)
        {
            if (uiTexts[i] == null)
            {
                continue;
            }
            originalFontSizes[i] = uiTexts[i].fontSize;
            originalTextColors[i] = uiTexts[i].color;
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
    }

    private void SubscribeToggles()
    {
        if (bigTextToggle != null)
        {
            bigTextToggle.onValueChanged.AddListener(OnBigTextChanged);
            // sync internal state with current toggle value without triggering scaling twice
            bigTextActive = false;
            if (bigTextToggle.isOn)
            {
                ApplyBigText(true);
            }
        }
        else
        {
            Debug.LogError("AccessibilityManager: BigTextT toggle not found.");
        }

        if (contrastToggle != null)
        {
            contrastToggle.onValueChanged.AddListener(OnContrastChanged);
            contrastActive = false;
            if (contrastToggle.isOn)
            {
                ApplyContrast(true);
            }
        }
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
        if (uiTexts == null || originalFontSizes == null)
        {
            return;
        }

        if (isOn == bigTextActive)
        {
            return;
        }
        bigTextActive = isOn;

        float scale = bigTextScale > 0f ? bigTextScale : 1f;

        for (int i = 0; i < uiTexts.Length; i++)
        {
            if (uiTexts[i] == null)
            {
                continue;
            }

            if (isOn)
            {
                uiTexts[i].fontSize = originalFontSizes[i] * scale;//multiply the original font size by the scale
            }
            else
            {
                uiTexts[i].fontSize = originalFontSizes[i];
            }
        }

        Debug.Log("Big Text: " + (isOn ? "ON" : "OFF"));
    }

    private void ApplyContrast(bool isOn)
    {
        if (isOn == contrastActive)
        {
            return;
        }
        contrastActive = isOn;

        if (uiTexts != null)
        {
            for (int i = 0; i < uiTexts.Length; i++)
            {
                if (uiTexts[i] == null)
                {
                    continue;
                }

                if (isOn)
                {
                    uiTexts[i].color = contrastTextColor;
                }
                else
                {
                    uiTexts[i].color = originalTextColors[i];
                }
            }
        }

        if (uiBackgrounds != null)
        {
            for (int i = 0; i < uiBackgrounds.Length; i++)
            {
                if (uiBackgrounds[i] == null)
                {
                    continue;
                }

                if (isOn)
                {
                    uiBackgrounds[i].color = contrastBgColor;
                }
                else
                {
                    uiBackgrounds[i].color = originalBgColors[i];
                }
            }
        }

        Debug.Log("Contrast: " + (isOn ? "ON" : "OFF"));
    }
}
