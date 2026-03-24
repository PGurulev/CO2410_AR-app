using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NavigationManager : MonoBehaviour
{
    public ARPathFinder pathfinder;
    public List<Transform> destinations; // Список точек из префаба

    private TMP_Dropdown dropdown;

    void Start()
    {
        // 1. Ищем Dropdown на сцене по имени
        GameObject dropdownObj = GameObject.Find("DestinationDropdown");

        if (dropdownObj != null)
        {
            dropdown = dropdownObj.GetComponent<TMP_Dropdown>();

            // 2. Автоматически заполняем Dropdown названиями ваших точек
            SetupDropdown();

            // 3. Подписываемся на изменение значения (тот самый триггер)
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(OnDropdownChanged);

            Debug.Log("NavigationManager: Успешно подключен к Dropdown.");

        }
        else
        {
            Debug.LogError("NavigationManager: Объект 'DestinationDropdown' не найден на сцене!");
        }
    }

    void SetupDropdown()
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>();// прост

        foreach (Transform t in destinations)
        {
            options.Add(t.name); // Берем имя объекта (напр. "Room 101")
        }

        dropdown.AddOptions(options);
    }

    // Метод-обработчик (теперь принимает индекс напрямую от Dropdown)
    public void OnDropdownChanged(int index)
    {
        if (index < destinations.Count)
        {
            Transform newTarget = destinations[index];
            pathfinder.SetTarget(newTarget);
            Debug.Log($"Маршрут изменен на: {newTarget.name}");
            // Замените старый Debug.Log на этот:
            Debug.Log($"Цель: {newTarget.name} | Координаты: {newTarget.position}");
        }
    }


}