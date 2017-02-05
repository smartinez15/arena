using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public LayerMask targetMask;
    public SpriteRenderer dot;
    public Color highlight;
    Color originalColor;

    void Start()
    {
        Cursor.visible = false;
        originalColor = dot.color;
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * 40 * Time.deltaTime);
    }

    public void DetectTarget(Ray ray)
    {
        if (Physics.Raycast(ray, 100, targetMask))
        {
            dot.color = highlight;
        }
        else
        {
            dot.color = originalColor;
        }
    }
}
