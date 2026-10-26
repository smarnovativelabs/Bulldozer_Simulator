using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class NavmeshPathDraw : MonoBehaviour
{
    public static NavmeshPathDraw Instance;

    [Header("Target")]
    public Transform destination;

    [Header("Recalculation")]
    public bool recalculatePath = true;
    [Min(0.1f)] public float recalculationTime = 0.25f;
    [Min(0.01f)] public float movementThreshold = 0.1f;

    [Header("Ground Validation")]
    public LayerMask groundLayers = 1;
    [Min(0.5f)] public float groundRayDistance = 8f;
    public float pathHeightOffset = 0.1f;
    [Tooltip("Stops the visible path before the target so the marker does not cover the object.")]
    [Min(0f)] public float destinationOffset = 0.75f;

    [Header("NavMesh Agent Type")]
    [Tooltip("Index from Navigation > Agents (0 = first agent type).")]
    public int agentTypeIndex;

    [Header("Fallback")]
    [Tooltip("Draw a direct guide when this scene has no baked NavMesh yet.")]
    public bool drawDirectFallback = true;

    readonly Vector3[] _corners = new Vector3[64];

    NavMeshPath _path;
    NavMeshQueryFilter _filter;
    LineRenderer _line;
    Vector3 _lastOrigin;
    Vector3 _lastDestination;
    float _elapsed;
    bool _hasLastPositions;
    bool _stopped = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = true;
        _line.alignment = LineAlignment.TransformZ;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        _path = new NavMeshPath();
        CacheAgentFilter();

        if (_line.sharedMaterial == null)
            _line.sharedMaterial = Resources.Load<Material>("material/path_mat");

        Stop();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void CacheAgentFilter()
    {
        int settingsCount = NavMesh.GetSettingsCount();
        if (settingsCount <= 0)
        {
            _filter = new NavMeshQueryFilter
            {
                agentTypeID = 0,
                areaMask = NavMesh.AllAreas
            };
            return;
        }

        int index = Mathf.Clamp(agentTypeIndex, 0, settingsCount - 1);
        NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index);
        _filter = new NavMeshQueryFilter
        {
            agentTypeID = settings.agentTypeID,
            areaMask = NavMesh.AllAreas
        };
    }

    // Kept for compatibility with the plugin's original public API.
    public void setDestinationAndDrawPath(Transform target)
    {
        destination = target;
        if (destination == null)
        {
            Stop();
            return;
        }

        _stopped = false;
        enabled = true;
        Draw();
    }

    public void Draw()
    {
        DrawPath(true);
    }

    void DrawPath(bool force)
    {
        if (destination == null || _line == null)
        {
            Stop();
            return;
        }

        Vector3 origin = GroundedPosition(transform.position);
        Vector3 target = GroundedPosition(destination.position);

        float sqrThreshold = movementThreshold * movementThreshold;
        if (!force && _hasLastPositions &&
            (origin - _lastOrigin).sqrMagnitude < sqrThreshold &&
            (target - _lastDestination).sqrMagnitude < sqrThreshold)
        {
            return;
        }

        _lastOrigin = origin;
        _lastDestination = target;
        _hasLastPositions = true;

        bool hasOrigin = NavMesh.SamplePosition(origin, out NavMeshHit originHit, 2f, _filter.areaMask);
        bool hasDestination = NavMesh.SamplePosition(target, out NavMeshHit destinationHit, 2f, _filter.areaMask);
        bool foundPath = false;

        if (hasOrigin && hasDestination)
        {
            foundPath = NavMesh.CalculatePath(
                originHit.position,
                destinationHit.position,
                _filter,
                _path);
        }

        int cornerCount = foundPath ? _path.GetCornersNonAlloc(_corners) : 0;
        if (cornerCount > 0)
        {
            int visibleCornerCount = TrimPathEnd(cornerCount, destinationOffset);
            _line.positionCount = visibleCornerCount;
            for (int i = 0; i < visibleCornerCount; i++)
            {
                Vector3 point = _corners[i];
                point.y += pathHeightOffset;
                _line.SetPosition(i, point);
            }
            return;
        }

        if (!drawDirectFallback)
        {
            _line.positionCount = 0;
            return;
        }

        origin.y += pathHeightOffset;
        target = OffsetDirectDestination(origin, target, destinationOffset);
        target.y += pathHeightOffset;
        _line.positionCount = 2;
        _line.SetPosition(0, origin);
        _line.SetPosition(1, target);
    }

    int TrimPathEnd(int cornerCount, float offset)
    {
        if (cornerCount < 2 || offset <= 0f)
            return cornerCount;

        int visibleCount = cornerCount;
        float remaining = offset;

        while (visibleCount > 1 && remaining > 0f)
        {
            int last = visibleCount - 1;
            Vector3 from = _corners[last];
            Vector3 to = _corners[last - 1];
            float segmentLength = Vector3.Distance(from, to);

            if (segmentLength <= 0.001f)
            {
                visibleCount--;
                continue;
            }

            if (remaining >= segmentLength && visibleCount > 2)
            {
                remaining -= segmentLength;
                visibleCount--;
                continue;
            }

            float appliedOffset = Mathf.Min(remaining, segmentLength * 0.9f);
            _corners[last] = Vector3.MoveTowards(from, to, appliedOffset);
            break;
        }

        return visibleCount;
    }

    static Vector3 OffsetDirectDestination(Vector3 origin, Vector3 target, float offset)
    {
        if (offset <= 0f)
            return target;

        Vector3 flatDirection = target - origin;
        flatDirection.y = 0f;
        float distance = flatDirection.magnitude;
        if (distance <= 0.001f)
            return target;

        float appliedOffset = Mathf.Min(offset, distance * 0.9f);
        return target - flatDirection / distance * appliedOffset;
    }

    Vector3 GroundedPosition(Vector3 position)
    {
        if (groundLayers.value == 0)
            return position;

        Vector3 rayOrigin = position + Vector3.up;
        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundRayDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore))
        {
            position.y = hit.point.y;
        }

        return position;
    }

    public void Stop()
    {
        _stopped = true;
        _hasLastPositions = false;
        _elapsed = 0f;

        if (_line != null)
            _line.positionCount = 0;

        enabled = false;
    }

    void LateUpdate()
    {
        if (!recalculatePath || _stopped)
            return;

        _elapsed += Time.deltaTime;
        if (_elapsed < Mathf.Max(0.1f, recalculationTime))
            return;

        _elapsed = 0f;
        DrawPath(false);
    }
}
