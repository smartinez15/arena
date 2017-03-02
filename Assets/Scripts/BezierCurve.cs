using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public Transform a;
    public Transform b;
    public Transform ah;
    public Transform bh;

    public GameObject line;

    [Range(0, 1)]
    public float t;

    public bool firstLines;
    public bool firstPoints;
    public bool secondLines;
    public bool secondPoints;
    public bool tangent;
    public bool point;
    public bool curve;

    Vector3 pa;
    Vector3 pb;
    Vector3 ab;
    Vector3 pa_ab;
    Vector3 ab_pb;
    Vector3 x;

    //Lines
    LineRenderer[] hLines = new LineRenderer[2];
    LineRenderer fLine;
    LineRenderer sLine;
    LineRenderer tanLine;
    LineRenderer curLine;

    //Points
    LineRenderer[] fPoints = new LineRenderer[3];
    LineRenderer[] sPoints = new LineRenderer[2];
    LineRenderer curPoint;


    public void GenerateCurve()
    {
        //creating curve holder Object
        string holderName = "Generated Curve";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform curveHolder = new GameObject(holderName).transform;
        curveHolder.parent = transform;

        Vector3[] sample = CalculateLine();

        Vector3[] pos2 = new Vector3[2];
        Vector3[] pos3 = new Vector3[3];

        for (int i = 0; i < hLines.Length; i++)
        {
            hLines[i] = (Instantiate(line) as GameObject).GetComponent<LineRenderer>();
            hLines[i].transform.SetParent(curveHolder);
            hLines[i].positionCount = 2;
        }
        pos2[0] = a.position;
        pos2[1] = ah.position;
        hLines[0].SetPositions(pos2);
        pos2[0] = b.position;
        pos2[1] = bh.position;
        hLines[1].SetPositions(pos2);


        if (firstLines)
        {
            fLine = (Instantiate(line) as GameObject).GetComponent<LineRenderer>();
            fLine.transform.SetParent(curveHolder);
            fLine.positionCount = 2;
            pos2[0] = ah.position;
            pos2[1] = bh.position;
            fLine.SetPositions(pos2);
        }

        if (firstPoints)
        {
            for (int i = 0; i < fPoints.Length; i++)
            {
                fPoints[i] = (Instantiate(line) as GameObject).GetComponent<LineRenderer>();
                fPoints[i].transform.SetParent(curveHolder);
                fPoints[i].positionCount = 2;
                fPoints[i].startWidth = 0.3f;
                fPoints[i].endWidth = 0.3f;
            }
            pos2[0] = pa;
            pos2[1] = pos2[0] - new Vector3(0, 0, 0.02f);
            fPoints[0].SetPositions(pos2);
            pos2[0] = ab;
            pos2[1] = pos2[0] - new Vector3(0, 0, 0.02f);
            fPoints[1].SetPositions(pos2);
            pos2[0] = pb;
            pos2[1] = pos2[0] - new Vector3(0, 0, 0.02f);
            fPoints[2].SetPositions(pos2);
        }

        if (secondLines)
        {
            sLine = (Instantiate(line) as GameObject).GetComponent<LineRenderer>();
            sLine.transform.SetParent(curveHolder);
            sLine.positionCount = 3;
            sLine.startWidth = 0.05f;
            sLine.endWidth = 0.05f;
            pos3[0] = pa;
            pos3[1] = ab;
            pos3[2] = pb;
            sLine.SetPositions(pos3);
        }

        if (secondPoints)
        {
            for (int i = 0; i < sPoints.Length; i++)
            {
                sPoints[i] = (Instantiate(line) as GameObject).GetComponent<LineRenderer>();
                sPoints[i].transform.SetParent(curveHolder);
                sPoints[i].positionCount = 2;
                sPoints[i].startWidth = 0.3f;
                sPoints[i].endWidth = 0.3f;
            }
            pos2[0] = pa_ab;
            pos2[1] = pos2[0] - new Vector3(0, 0, 0.02f);
            sPoints[0].SetPositions(pos2);
            pos2[0] = ab_pb;
            pos2[1] = pos2[0] - new Vector3(0, 0, 0.02f);
            sPoints[1].SetPositions(pos2);
        }

        if (tangent)
        {
            tanLine = (Instantiate(line) as GameObject).GetComponent<LineRenderer>();
            tanLine.transform.SetParent(curveHolder);
            tanLine.positionCount = 2;
            tanLine.startWidth = 0.02f;
            tanLine.endWidth = 0.02f;
            pos2[0] = pa_ab;
            pos2[1] = ab_pb;
            tanLine.SetPositions(pos2);
        }

        if (point)
        {
            curPoint = (Instantiate(line) as GameObject).GetComponent<LineRenderer>();
            curPoint.transform.SetParent(curveHolder);
            curPoint.positionCount = 2;
            curPoint.startWidth = 0.3f;
            curPoint.endWidth = 0.3f;
            pos2[0] = x;
            pos2[1] = pos2[0] - new Vector3(0, 0, 0.02f);
            curPoint.SetPositions(pos2);
            curPoint.startColor = Color.blue;
            curPoint.endColor = Color.blue;
        }

        if (curve)
        {
            curLine = (Instantiate(line) as GameObject).GetComponent<LineRenderer>();
            curLine.transform.SetParent(curveHolder);
            curLine.positionCount = 2;
            curLine.startWidth = 0.3f;
            curLine.endWidth = 0.3f;
            curLine.startColor = Color.blue;
            curLine.endColor = Color.blue;

            int samplePoints = Mathf.CeilToInt(t / 0.02f);
            curLine.positionCount = samplePoints;
            curLine.SetPositions(sample);
        }
    }

    Vector3[] CalculateLine()
    {
        int samplePoints = Mathf.CeilToInt(t / 0.02f);
        Vector3[] sample = new Vector3[samplePoints];
        for (int i = 0; i < samplePoints; i++)
        {
            float it = i * 0.02f;
            pa = Vector3.Lerp(a.position, ah.position, it);
            ab = Vector3.Lerp(ah.position, bh.position, it);
            pb = Vector3.Lerp(bh.position, b.position, it);

            pa_ab = Vector3.Lerp(pa, ab, it);
            ab_pb = Vector3.Lerp(ab, pb, it);

            x = Vector3.Lerp(pa_ab, ab_pb, it);

            sample[i] = x;
        }

        pa = Vector3.Lerp(a.position, ah.position, t);
        ab = Vector3.Lerp(ah.position, bh.position, t);
        pb = Vector3.Lerp(bh.position, b.position, t);

        pa_ab = Vector3.Lerp(pa, ab, t);
        ab_pb = Vector3.Lerp(ab, pb, t);

        x = Vector3.Lerp(pa_ab, ab_pb, t);

        return sample;
    }
}
