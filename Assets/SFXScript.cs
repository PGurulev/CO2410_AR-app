using TMPro;
using UnityEngine;

public class SFXScript : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip changeClip;

    //straight ahead — find dropdown and audio source, subscribe to events
    void Start()
    {
        if (dropdown == null)
        {
            GameObject dropdownObj = GameObject.Find("DestinationDropdown");
            if (dropdownObj != null)
            {
                dropdown = dropdownObj.GetComponent<TMP_Dropdown>();
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
        else
        {
            Debug.LogError("SFXScript: TMP_Dropdown not found.");
        }
    }

    //straight ahead — unsubscribe listener on destroy
    void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
    }

    //straight ahead — play sound when dropdown value changes
    private void OnDropdownChanged(int index)
    {
        if (changeClip == null)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(changeClip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(changeClip, transform.position);
        }
    }
}
