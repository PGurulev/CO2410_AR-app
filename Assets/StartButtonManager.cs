using UnityEngine;
using UnityEngine.UI;

public class StartButtonManager : MonoBehaviour
{
    //straight ahead — references to Start button and panels it controls
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject destinationDropdown;
    [SerializeField] private GameObject optionsButton;

    private bool menusVisible;

    //straight ahead — find UI objects by name if not assigned, hide menus, subscribe
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

        if (optionsButton == null)
        {
            GameObject obj = GameObject.Find("SettingsButton");
            if (obj != null)
            {
                optionsButton = obj;
            }
        }

        //straight ahead — hide everything until Start is pressed
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

    //straight ahead — clean up listener on destroy
    void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartClicked);
        }
    }

    //straight ahead — toggle menus visibility on each click
    private void OnStartClicked()
    {
        menusVisible = !menusVisible;
        SetMenusVisible(menusVisible);
    }

    //straight ahead — show or hide destination dropdown and options button
    private void SetMenusVisible(bool visible)
    {
        if (destinationDropdown != null)
        {
            destinationDropdown.SetActive(visible);
        }

        if (optionsButton != null)
        {
            optionsButton.SetActive(visible);
        }
    }
}
