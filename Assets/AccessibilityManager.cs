using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
    //straight ahead — settings button that shows/hides its toggle children
    [Header("Options Modal")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject bigTextToggleObj;
    [SerializeField] private GameObject contrastToggleObj;

    //straight ahead — toggle components for listening to value changes
    [Header("Toggles")]
    [SerializeField] private Toggle bigTextToggle;
    [SerializeField] private Toggle contrastToggle;

    //straight ahead — all TMP texts and UI backgrounds affected by accessibility
    [Header("UI elements")]
    [SerializeField] private TMP_Text[] uiTexts;
    [SerializeField] private Image[] uiBackgrounds;

    //straight ahead — font sizes for normal and big text modes
    [Header("Big Text settings")]
    [SerializeField] private float normalFontSize = 14f;
    [SerializeField] private float bigFontSize = 24f;

    //straight ahead — colors for contrast mode
    [Header("Contrast settings")]
    [SerializeField] private Color contrastTextColor = Color.yellow;
    [SerializeField] private Color contrastBgColor = Color.black;

    private Color[] originalTextColors;
    private Color[] originalBgColors;
    private float[] originalFontSizes;

    //straight ahead — find objects by name, hide toggles, wire up events
    void Start()
    {
        if (settingsButton == null)
        {
            GameObject obj = GameObject.Find("SettingsButton");
            if (obj != null)
            {
                settingsButton = obj.GetComponent<Button>();
            }
        }

        if (bigTextToggleObj == null)
        {
            GameObject obj = GameObject.Find("BigTextT");
            if (obj != null)
            {
                bigTextToggleObj = obj;
                bigTextToggle = obj.GetComponent<Toggle>();
            }
        }

        if (contrastToggleObj == null)
        {
            GameObject obj = GameObject.Find("ContrastT");
            if (obj != null)
            {
                contrastToggleObj = obj;
                contrastToggle = obj.GetComponent<Toggle>();
            }
        }

        //straight ahead — hide toggles until settings button is pressed
        SetTogglesVisible(false);

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (bigTextToggle != null)
        {
            bigTextToggle.isOn = false;
            bigTextToggle.onValueChanged.AddListener(OnBigTextToggled);
        }

        if (contrastToggle != null)
        {
            contrastToggle.isOn = false;
            contrastToggle.onValueChanged.AddListener(OnContrastToggled);
        }

        CacheOriginalValues();
    }

    //straight ahead — remove all listeners on destroy
    void OnDestroy()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        if (bigTextToggle != null)
        {
            bigTextToggle.onValueChanged.RemoveListener(OnBigTextToggled);
        }

        if (contrastToggle != null)
        {
            contrastToggle.onValueChanged.RemoveListener(OnContrastToggled);
        }
    }

    //straight ahead — save original colors and sizes before any changes
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

    //straight ahead — show or hide toggle children when settings is clicked
    private void OnSettingsClicked()
    {
        if (bigTextToggleObj == null && contrastToggleObj == null)
        {
            return;
        }

        bool currentlyVisible = false;
        if (bigTextToggleObj != null)
        {
            currentlyVisible = bigTextToggleObj.activeSelf;
        }

        SetTogglesVisible(!currentlyVisible);
    }

    //straight ahead — set both toggle objects active or inactive
    private void SetTogglesVisible(bool visible)
    {
        if (bigTextToggleObj != null)
        {
            bigTextToggleObj.SetActive(visible);
        }

        if (contrastToggleObj != null)
        {
            contrastToggleObj.SetActive(visible);
        }
    }

    //straight ahead — apply or revert big text on all ui texts
    private void OnBigTextToggled(bool isOn)
    {
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

            if (isOn)
            {
                uiTexts[i].fontSize = bigFontSize;
            }
            else
            {
                uiTexts[i].fontSize = originalFontSizes[i];
            }
        }

        Debug.Log("Big Text: " + (isOn ? "ON" : "OFF"));
    }

    //straight ahead — apply or revert contrast colors on texts and backgrounds
    private void OnContrastToggled(bool isOn)
    {
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

        Debug.Log("Contrast Colors: " + (isOn ? "ON" : "OFF"));
    }
}
