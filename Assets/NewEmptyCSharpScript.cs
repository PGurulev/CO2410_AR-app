using TMPro;
using UnityEngine;

public class NewEmptyCSharpScript : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip changeClip;//метод "меняем звук"

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
            Debug.LogError("NewEmptyCSharpScript: TMP_Dropdown не найден. Укажите в инспекторе или назовите объект DestinationDropdown.");
    }

    void OnDestroy()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);//удалит из списка listener метод OnDropdownChanged
    }

    private void OnDropdownChanged(int index)//функция "меняем звук", вызывает звук при изменении значения в Dropdown
    {
        if (changeClip == null)
            return;

        if (audioSource != null)
            audioSource.PlayOneShot(changeClip);//воспроизведет звук один раз
        else
            AudioSource.PlayClipAtPoint(changeClip, transform.position);
    }
}
