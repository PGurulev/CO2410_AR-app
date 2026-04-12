using UnityEngine;
using UnityEngine.AI;
using Unity.XR.CoreUtils;

public class ARPathFinder : MonoBehaviour
{
    [Header("Navigation settings")]
    public Transform target;

    [Tooltip("Max distance to snap a point onto NavMesh (targets / probes).")]
    [SerializeField] private float navMeshSampleRadius = 50.0f;

    [Tooltip("If true, only draw when a full path exists (avoids odd partial paths).")]
    [SerializeField] private bool requireCompletePath = true;

    private LineRenderer line;
    private NavMeshPath path;
    private Transform cameraTransform;
    private XROrigin cachedXrOrigin;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        path = new NavMeshPath();
        ResolveCameraReference();
    }

    void OnEnable()
    {
        ResolveCameraReference();
    }

    private void ResolveCameraReference()
    {
        if (cachedXrOrigin == null)
        {
            cachedXrOrigin = FindFirstObjectByType<XROrigin>();
        }

        if (cachedXrOrigin != null && cachedXrOrigin.Camera != null)
        {
            cameraTransform = cachedXrOrigin.Camera.transform;
            return;
        }

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        if (cameraTransform == null)
        {
            ResolveCameraReference();
            if (cameraTransform == null)
            {
                return;
            }
        }

        if (!NavMesh.SamplePosition(target.position, out NavMeshHit targetHit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            line.positionCount = 0;
            return;
        }

        if (!TryFindStartHit(cameraTransform.position, targetHit, out NavMeshHit startHit))
        {
            line.positionCount = 0;
            return;
        }

        if (path.corners.Length == 0)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = path.corners.Length;
        for (int i = 0; i < path.corners.Length; i++)
        {
            line.SetPosition(i, path.corners[i] + Vector3.up * 0.05f);
        }
    }

    private bool TryFindStartHit(Vector3 cameraWorld, NavMeshHit targetHit, out NavMeshHit startHit)
    {
        startHit = default;
        float[] radii = new float[] { 2f, 8f, 25f, navMeshSampleRadius, 120f };

        for (int step = 0; step <= 8; step++)
        {
            float t = step / 8f;
            Vector3 probe = Vector3.Lerp(cameraWorld, targetHit.position, t);

            for (int ri = 0; ri < radii.Length; ri++)
            {
                if (!NavMesh.SamplePosition(probe, out startHit, radii[ri], NavMesh.AllAreas))
                {
                    continue;
                }

                if (!NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, path))
                {
                    continue;
                }

                if (requireCompletePath && path.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
