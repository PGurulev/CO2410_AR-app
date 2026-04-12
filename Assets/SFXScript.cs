using System;
using TMPro;
using UnityEngine;

public class SFXScript : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private AudioSource audioSource;

    [Header("Звук для каждого направления")]
    [SerializeField] private AudioClip lunchRoomClip;
    [SerializeField] private AudioClip hallClip;
    [SerializeField] private AudioClip theatreClip;
    [SerializeField] private AudioClip courtRoomClip;

    [Header("Общий звук (если клип направления не задан)")]
    [SerializeField] private AudioClip fallbackClip;

    void Start()
    {
        if (dropdown == null)
        {
            GameObject dropdownObj = GameObject.Find("DestinationDropdown");
            if (dropdownObj != null)
                dropdown = dropdownObj.GetComponent<TMP_Dropdown>();
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (dropdown != null)
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        else
            Debug.LogError("SFXScript: TMP_Dropdown не найден. Укажите в инспекторе или назовите объект DestinationDropdown.");
    }

    void OnDestroy()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        AudioClip clip = GetClipForIndex(index);
        if (clip == null)
            clip = fallbackClip;
        if (clip == null)
            return;

        if (audioSource != null)
            audioSource.PlayOneShot(clip);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    private AudioClip GetClipForIndex(int index)
    {
        string optionText = dropdown.options[index].text;

        return optionText switch
        {
            "LunchRoom" => lunchRoomClip,
            "Hall"      => hallClip,
            "Theathre"  => theatreClip,
            "CourtRoom" => courtRoomClip,
            _           => null,
        };
    }
}
