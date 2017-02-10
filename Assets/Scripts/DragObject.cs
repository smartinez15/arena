using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{
    BezierCurve curve;
    float distance = 10;
    void Start()
    {
        curve = FindObjectOfType<BezierCurve>();
    }
    void OnMouseDrag()
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
        Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = objPosition;
        if (curve != null)
        {
            curve.GenerateCurve();
        }
    }
}
