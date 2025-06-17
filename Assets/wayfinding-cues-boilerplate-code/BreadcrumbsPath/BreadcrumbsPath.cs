using System;
using System.Collections;
using System.Collections.Generic;
using Packages.Rider.Editor.UnitTesting;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.AI;

public class BreadcrumbsPath : MonoBehaviour
{
    [Header("Marker Configuration")]
    [SerializeField] private GameObject[] markers;
    [SerializeField] private Vector3 inactivePosition;
    [SerializeField] private float markerDistance = 5.0f;
    [SerializeField] private int skipAFewMarkers = 1;
    [Header("Agent Configuration")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    private NavMeshPath _currentPath;
    private NavMeshAgent _navAgent;

    // Start is called before the first frame update
    private void Start()
    {
        _currentPath = new NavMeshPath();
    }

    // Update is called once per frame
    private void Update()
    {
        // ...

        NavMeshHit target_pos;
        NavMeshHit player_pos;
        NavMesh.SamplePosition(target.position, out target_pos, 2.0f, NavMesh.AllAreas);
        NavMesh.SamplePosition(player.position, out player_pos, 2.0f, NavMesh.AllAreas);

        NavMesh.CalculatePath(player_pos.position, target_pos.position, NavMesh.AllAreas, _currentPath);
        for (int i = 0; i < _currentPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(_currentPath.corners[i], _currentPath.corners[i + 1], Color.red);
        }

        UpdateMarkers(_currentPath.corners);
    }

    private void UpdateMarkers(Vector3[] path)
    {
        if (path.Length < 2)
        {
            return;
        }

        var potentialMarkerPositions = new List<Vector3>();
        var rest = 0.0f;
        var from = Vector3.zero;
        var to = Vector3.zero;
        float remaining = 0.0f;

        for (int i = 0; i < path.Length - 1; i++)
        {

            from = path[i];
            to = path[i + 1];
            float seg_len = (to - from).magnitude;
            remaining = seg_len + rest;
            while (remaining > markerDistance)
            {
                remaining -= markerDistance;
                potentialMarkerPositions.Add(from + (to - from).normalized * (seg_len - remaining));
            }
            rest = remaining;
        }

        for (int i = 0; i < markers.Length; i++)
        {
            if (potentialMarkerPositions.Count > i + skipAFewMarkers)
            {
                Vector3 position = potentialMarkerPositions[i + skipAFewMarkers];
                position.y -= 1;
                markers[i].transform.position = position;
            }
            else
            {
                markers[i].transform.position = inactivePosition;
            }
        }
        

    }
}