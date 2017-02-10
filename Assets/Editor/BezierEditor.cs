using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BezierCurve curve = target as BezierCurve;
        if (DrawDefaultInspector())
        {
            curve.GenerateCurve();
        }

        if (GUILayout.Button("Generate"))
        {
            curve.GenerateCurve();
        }
    }
}
