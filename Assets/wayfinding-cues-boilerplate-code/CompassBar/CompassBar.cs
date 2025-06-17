using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class CompassBar : MonoBehaviour
{
    public float BarRange => barRange;
    [SerializeField] private float barRange = 120;

    public RectTransform BarRectTransform => _rectTransform;
    private RectTransform _rectTransform;
    [SerializeField] private Transform target;
    public Transform Target => target;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        Transform player_trans = transform.parent.parent;
        float heading = player_trans.rotation.eulerAngles.y; // get the heading, range 0-360

        // map to NWES, poition the NWEST letters on the compass bar
        RectTransform N = (RectTransform)transform.Find("Viewport/N");
        RectTransform W = (RectTransform)transform.Find("Viewport/W");
        RectTransform O = (RectTransform)transform.Find("Viewport/O");
        RectTransform S = (RectTransform)transform.Find("Viewport/S");

        if (heading > 180)
        {
            heading = heading - 360;
        }

        N.anchoredPosition = new UnityEngine.Vector3(-heading * 4, 0, 0);
        W.anchoredPosition = new UnityEngine.Vector3(-heading * 4 - 360, 0, 0);
        O.anchoredPosition = new UnityEngine.Vector3(-heading * 4 + 360, 0, 0);

        if (heading > 0)
        {
            S.anchoredPosition = new UnityEngine.Vector3(-heading * 4 + 720, 0, 0);
        }
        else
        {
            S.anchoredPosition = new UnityEngine.Vector3(-heading * 4 - 720, 0, 0);
        }

        // position the target

        UnityEngine.Vector3 local_coord = player_trans.InverseTransformPoint(target.transform.position);
        local_coord.y = 0;
        Vector3 head = new UnityEngine.Vector3(0, 0, 1);
        float angle = UnityEngine.Vector3.SignedAngle(local_coord, head, Vector3.up);

        float offset = -angle * 4;
        if (offset > 360)
        {
            offset = 360;
        }
        else if (offset < -360)
        {
            offset = -360;
        }

        RectTransform target_heading = (RectTransform)transform.Find("Viewport/TargetMarker");
        target_heading.anchoredPosition = new UnityEngine.Vector3(offset, 0, 0);
    }
}