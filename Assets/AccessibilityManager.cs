using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
    // accessibility dropdown reference
    [Header("Accessibility Dropdown")]
    [SerializeField] private TMP_Dropdown accessibilityDropdown;

    // all TMP texts and UI backgrounds affected by accessibility
    [Header("UI elements")]
    [SerializeField] private TMP_Text[] uiTexts;
    [SerializeField] private Image[] uiBackgrounds;

    // font sizes for normal and big text modes
    [Header("Big Text settings")]
    [SerializeField] private float normalFontSize = 14f;
    [SerializeField] private float bigFontSize = 24f;

    // colors for contrast mode
    [Header("Contrast settings")]
    [SerializeField] private Color contrastTextColor = Color.yellow;
    [SerializeField] private Color contrastBgColor = Color.black;

    private Color[] originalTextColors;
    private Color[] originalBgColors;
    private float[] originalFontSizes;
    private bool bigTextActive;
    private bool contrastActive;

    // find dropdown by name, populate options, subscribe
    void Start()
    {
        if (accessibilityDropdown == null)
        {
            GameObject obj = GameObject.Find("AccessibilityDropdown");
            if (obj != null)
            {
                accessibilityDropdown = obj.GetComponent<TMP_Dropdown>();
            }
        }

        if (accessibilityDropdown != null)
        {
            accessibilityDropdown.ClearOptions();
            accessibilityDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Accessibility",
                "Big Text",
                "Contrast Colors"
            });
            accessibilityDropdown.onValueChanged.AddListener(OnAccessibilityChanged);
        }
        else
        {
            Debug.LogError("AccessibilityManager: AccessibilityDropdown not found.");
        }

        CacheOriginalValues();
    }

    // remove listener on destroy
    void OnDestroy()
    {
        if (accessibilityDropdown != null)
        {
            accessibilityDropdown.onValueChanged.RemoveListener(OnAccessibilityChanged);
        }
    }

    // save original colors and sizes before any changes
    private void CacheOriginalValues()
    {
        if (uiTexts != null && uiTexts.Length > 0)
        {
            originalTextColors = new Color[uiTexts.Length];
            originalFontSizes = new float[uiTexts.Length];

            for (int i = 0; i < uiTexts.Length; i++)
            {
                if (uiTexts[i] == null)
                {
                    continue;
                }
                originalTextColors[i] = uiTexts[i].color;
                originalFontSizes[i] = uiTexts[i].fontSize;
            }
        }

        if (uiBackgrounds != null && uiBackgrounds.Length > 0)
        {
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
    }

    // handle dropdown selection, reset back to index 0
    private void OnAccessibilityChanged(int index)
    {
        switch (index)
        {
            case 1:
                ToggleBigText();
                break;
            case 2:
                ToggleContrast();
                break;
        }

        // snap dropdown back to "Accessibility" label
        accessibilityDropdown.SetValueWithoutNotify(0);
    }

    // apply or revert big text on all ui texts
    private void ToggleBigText()
    {
        bigTextActive = !bigTextActive;

        if (uiTexts == null)
        {
            return;
        }

        for (int i = 0; i < uiTexts.Length; i++)
        {
            if (uiTexts[i] == null)
            {
                continue;
            }

            if (bigTextActive)
            {
                uiTexts[i].fontSize = bigFontSize;
            }
            else
            {
                uiTexts[i].fontSize = originalFontSizes[i];
            }
        }

        Debug.Log("Big Text: " + (bigTextActive ? "ON" : "OFF"));
    }

    // apply or revert contrast colors on texts and backgrounds
    private void ToggleContrast()
    {
        contrastActive = !contrastActive;

        if (uiTexts != null)
        {
            for (int i = 0; i < uiTexts.Length; i++)
            {
                if (uiTexts[i] == null)
                {
                    continue;
                }

                if (contrastActive)
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

                if (contrastActive)
                {
                    uiBackgrounds[i].color = contrastBgColor;
                }
                else
                {
                    uiBackgrounds[i].color = originalBgColors[i];
                }
            }
        }

        Debug.Log("Contrast Colors: " + (contrastActive ? "ON" : "OFF"));
    }
}
