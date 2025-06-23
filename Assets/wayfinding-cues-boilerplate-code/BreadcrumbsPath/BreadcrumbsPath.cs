using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BreadcrumbsPath : MonoBehaviour
{
    [Header("Marker configuration")]
    [SerializeField] private GameObject[] markers;
    [SerializeField] private Vector3 inactivePosition = new(0, -50, 0);
    [SerializeField] private float markerSpacing = 4f;
    [SerializeField] private int skipNearPlayer = 2;
    [SerializeField] private float markerYOffset = 1f; // ← added for vertical offset control

    [Header("Waypoints in quest order")]
    [Tooltip("Drag the Merchant first, then the Elder, then the next target …")]
    public List<Transform> waypoints = new();

    [Header("References")]
    [SerializeField] private Transform player;

    /* ---------- private ---------- */
    NavMeshPath _path;
    int _currentIndex = -1;

    /* ---------- life-cycle ---------- */
    void Awake() => _path = new NavMeshPath();

    void Start() => SetTargetByIndex(0);

    void Update()
    {
        if (_currentIndex < 0 || _currentIndex >= waypoints.Count) return;

        Transform target = waypoints[_currentIndex];
        if (!player || !target) return;

        // Sample positions from NavMesh (no height offset here)
        if (!NavMesh.SamplePosition(player.position, out var pHit, 2, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(target.position, out var tHit, 2, NavMesh.AllAreas))
            return;

        if (!NavMesh.CalculatePath(tHit.position, pHit.position, NavMesh.AllAreas, _path) ||
            _path.corners.Length < 2) return;

        List<Vector3> pts = SamplePath(_path.corners, markerSpacing);
        UpdateMarkers(pts); // ← height offset applied only here
    }

    /* ---------- public API ---------- */
    public void SetTargetByIndex(int index)
    {
        if (index < 0 || index >= waypoints.Count)
        {
            HideAllMarkers();
            _currentIndex = -1;
            return;
        }
        _currentIndex = index;
    }

    /* ---------- helpers ---------- */
    static List<Vector3> SamplePath(IReadOnlyList<Vector3> c, float spacing)
    {
        var pts = new List<Vector3>();
        float carry = 0;
        Vector3 from = c[0];

        for (int i = 1; i < c.Count; i++)
        {
            Vector3 to = c[i];
            float seg = Vector3.Distance(from, to);

            while (seg + carry >= spacing)
            {
                float d = spacing - carry;
                Vector3 p = Vector3.Lerp(from, to, d / seg);
                pts.Add(p);

                seg -= d;
                carry = 0;
                from = p;
            }
            carry += seg;
            from = to;
        }
        return pts;
    }

    void UpdateMarkers(IReadOnlyList<Vector3> pts)
    {
        for (int i = 0; i < markers.Length; i++)
        {
            int idx = pts.Count - 1 - skipNearPlayer - i;
            if (idx >= 0)
            {
                Vector3 pos = pts[idx];
                pos.y += markerYOffset; // ← apply height offset here only
                markers[i].transform.position = pos;
            }
            else
            {
                markers[i].transform.position = inactivePosition;
            }
        }
    }

    void HideAllMarkers()
    {
        foreach (var m in markers)
            m.transform.position = inactivePosition;
    }
}
