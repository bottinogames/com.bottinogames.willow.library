using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Willow.Library;
#if UNITY_EDITOR
using UnityEditor;
#endif 

public class FullOrbitCamera : MonoBehaviour
{
    [HideInInspector]
    public bool allowZoom;

    [HideInInspector]
    public float minZoomDist = 5f;
    [HideInInspector]
    public float maxZoomDist = 15f;
    [HideInInspector]
    public float dist = 10f;

    [Space(5)]
    [Range(0.2f, 10f)]
    public float orbitSmoothing = 2f;
    [Range(0f, 90f)]
    public float verticalClamping = 85f;
    [Range(0f, 360f)]
    public float horizontalClamping = 360f;

    [Space(5)]
    public float orbitSensitivity = 1f;
    public float zoomSensitivity = 1f;

    [Space(10)]
    public Transform target;

    [Space(5)]
    [Range(0f,10f)]
    public float positionalSmoothing = 4f;

    private Vector3 targetFallback;

    private float targetX;
    private float targetY;
    private float x;
    private float y;
    private float z;

    private Vector3 pos;

    private void Start()
    {
        targetFallback = transform.position;
        z = dist;
    }

    void Update()
    {
        if (Input.GetMouseButton(2))
        {
            targetX += Input.GetAxisRaw("Mouse X") * orbitSensitivity;
            if (horizontalClamping != 360f)
            {
                targetX = Mathf.Clamp(targetX, -horizontalClamping, horizontalClamping);
            }

            targetY += Input.GetAxisRaw("Mouse Y") * orbitSensitivity;
            targetY = Mathf.Clamp(targetY, -verticalClamping, verticalClamping);
        }

        x = Maths.Damp(x, targetX, orbitSmoothing * orbitSmoothing, true);
        y = Maths.Damp(y, targetY, orbitSmoothing * orbitSmoothing, true);

        transform.rotation = Quaternion.Euler(-y, x, 0f);


        if (allowZoom)
        {
            dist = Mathf.Clamp(dist + -Input.mouseScrollDelta.y * zoomSensitivity * 0.5f, minZoomDist, maxZoomDist);
        }
        z = Maths.Damp(z, dist, orbitSmoothing * orbitSmoothing, true);

        Vector3 p = targetFallback;
        if (target)
        {
            if (positionalSmoothing == 0f)
                pos = target.position;
            else
                pos = Maths.Damp(pos, target.position, positionalSmoothing * positionalSmoothing, true);
            p = pos;
        }

        transform.position = p + transform.forward * -z;
        
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(FullOrbitCamera))]
public class FullOrbitCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FullOrbitCamera foc = (FullOrbitCamera)target;

        Undo.RecordObject(target, "Full Orbit Camera custom inspector");

        bool old = foc.allowZoom;
        foc.allowZoom = EditorGUILayout.Toggle("Allow Zoom", foc.allowZoom);
        if(foc.allowZoom && !old)
        {
            foc.minZoomDist = Mathf.Min(foc.minZoomDist, foc.dist);
            foc.maxZoomDist = Mathf.Max(foc.maxZoomDist, foc.dist);
        }

        if (!foc.allowZoom)
        {
            foc.dist = Mathf.Max(0f,EditorGUILayout.FloatField("Distance", foc.dist));
        }
        else
        {
            foc.minZoomDist = Mathf.Max(0f, EditorGUILayout.FloatField("Minimum Distance", foc.minZoomDist));
            foc.maxZoomDist = Mathf.Max(foc.minZoomDist, EditorGUILayout.FloatField("Maximum Distance", foc.maxZoomDist));
            foc.dist = Mathf.Clamp(EditorGUILayout.Slider("Distance", foc.dist, foc.minZoomDist, foc.maxZoomDist), foc.minZoomDist, foc.maxZoomDist);
        }

        base.OnInspectorGUI();
    }
}
#endif

