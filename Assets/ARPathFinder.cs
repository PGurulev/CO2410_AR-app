using UnityEngine;
using UnityEngine.AI;

public class ARPathFinder : MonoBehaviour
{
    [Header("Настройки навигации")]
    public Transform target;

    private LineRenderer line;
    private NavMeshPath path;
    private Transform cameraTransform;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        path = new NavMeshPath();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
        }
    }

    void Update()
    {
        if (target == null || cameraTransform == null) return;

        // Ищем ближайшие точки НА сетке NavMesh в радиусе 5 метров (увеличили радиус)
        NavMeshHit startHit, targetHit;

        bool validStart = NavMesh.SamplePosition(cameraTransform.position, out startHit, 5.0f, NavMesh.AllAreas);
        bool validTarget = NavMesh.SamplePosition(target.position, out targetHit, 5.0f, NavMesh.AllAreas);

        if (validStart && validTarget)
        {
            if (NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, path))
            {
                line.positionCount = path.corners.Length;
                for (int i = 0; i < path.corners.Length; i++)
                {
                    line.SetPosition(i, path.corners[i] + Vector3.up * 0.05f);
                }
            }
            else
            {
                Debug.LogWarning("Путь не найден, хотя точки на сетке.");
                line.positionCount = 0;
            }
        }
        else
        {
            // Если видишь это - значит ты СЛИШКОМ далеко от синей сетки
            Debug.LogWarning($"Точки вне NavMesh! Старт: {validStart}, Цель: {validTarget}");
            line.positionCount = 0;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}