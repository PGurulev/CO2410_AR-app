using UnityEngine;
using UnityEngine.UI;

public class StartButtonManager : MonoBehaviour
{
    // references to Start button and panels it controls
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject destinationDropdown;
    [SerializeField] private GameObject accessibilityDropdown;

    private bool menusVisible;

    // find UI objects by name if not assigned, hide menus, subscribe
    void Start()
    {
        if (startButton == null)
        {
            GameObject obj = GameObject.Find("Startbutton");
            if (obj != null)
            {
                startButton = obj.GetComponent<Button>();
            }
        }

        if (destinationDropdown == null)
        {
            GameObject obj = GameObject.Find("DestinationDropdown");
            if (obj != null)
            {
                destinationDropdown = obj;
            }
        }

        if (accessibilityDropdown == null)
        {
            GameObject obj = GameObject.Find("AccessibilityDropdown");
            if (obj != null)
            {
                accessibilityDropdown = obj;
            }
        }

        // hide everything until Start is pressed
        SetMenusVisible(false);

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }
        else
        {
            Debug.LogError("StartButtonManager: StartButton not found.");
        }
    }

    // clean up listener on destroy
    void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartClicked);
        }
    }

    // toggle menus visibility on each click
    private void OnStartClicked()
    {
        menusVisible = !menusVisible;
        SetMenusVisible(menusVisible);
    }

    // show or hide both dropdowns
    private void SetMenusVisible(bool visible)
    {
        if (destinationDropdown != null)
        {
            destinationDropdown.SetActive(visible);
        }

        if (accessibilityDropdown != null)
        {
            accessibilityDropdown.SetActive(visible);
        }
    }
}
