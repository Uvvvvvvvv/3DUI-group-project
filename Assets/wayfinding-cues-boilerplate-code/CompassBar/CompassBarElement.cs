using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

public class CompassBarElement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;
    [SerializeField] private bool useFixDirection = false;
    [SerializeField] private Vector3 fixDirection;

    private CompassBar bar;
    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        bar = GetComponentInParent<CompassBar>();
    }

    private void Update()
    {
        // ...
    }
}