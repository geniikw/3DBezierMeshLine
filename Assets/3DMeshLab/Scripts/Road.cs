using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

using GoodNightMypi.MeshObjectLab;

public class Road : MeshBehaviour, ISerializationCallbackReceiver {
       
    public List<CurveInfo> points = new List<CurveInfo>();

    public float zWidth = 1f;

    [SerializeField][HideInInspector]
    List<bool> checkPoint = new List<bool>();

    List<Vector3> listAccumulate = new List<Vector3>();
    Vector3 GetWorldPosition(int index)
    {
        return listAccumulate[index] + transform.position;
    }

    public int checkPointDivideCount = 20;
    
    void CheckListIndex(int index)
    {
        if (points.Count != listAccumulate.Count)
        {
            this.enabled = false;
            throw new Exception("index error Not equal  wp : " + listAccumulate.Count + " listPoint : " + points.Count);
        }

        if (index >= points.Count - 1 || index < 0)
        {
            this.enabled = false;
            throw new Exception("index error  index : " + index + "max : " + points.Count);
        }
    }
    
    Vector3 GetCurvePosition(int index, float t)
    {
        Vector3 p0, p1, p2, p3;
        Get4CurvePoint(index, out p0, out p1, out p2, out p3);

        return Curve.Cubic(p0, p1, p2, p3, t);
    }

    Vector3 GetCurveDirection(int index, float t)
    {
        Vector3 p0, p1, p2, p3;
        Get4CurvePoint(index, out p0, out p1, out p2, out p3);
        return Curve.CubicDirection(p0, p1, p2, p3, t);
    }

    private void Get4CurvePoint(int index, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
    {
        CheckListIndex(index);
        p0 = GetWorldPosition(index);
        p1 = GetWorldPosition(index) + points[index].np;
        p2 = GetWorldPosition(index + 1) + points[index + 1].pp;
        p3 = GetWorldPosition(index + 1);
    }

    protected override void Modify()
    {
        if (points.Count <= 1)
            throw new Exception("2개이상의 점이 필요함.");

        Vector3 pointBuffer = points[0].offset;
        Vector3 endVertex1 = default(Vector3);
        Vector3 endVertex2 = default(Vector3);

        for (int n = 0; n < points.Count-1; n++)
        {   
            var sInfo = points[n];
            var eInfo = points[n + 1];
            
            var p0 = pointBuffer;
            var p1 = p0 + sInfo.np;
             
            var p3 = p0 + eInfo.offset;
            var p2 = p3 + eInfo.pp;
            pointBuffer = p3;
            
            var buffer = AddCubicBezierLine(p0, p1, p2, p3, zWidth, 
                sInfo.width, eInfo.width, sInfo.divideCount, 
                endVertex1, endVertex2);

            endVertex1 = buffer[0];
            endVertex2 = buffer[1];
        }
    }
    
    void InitData()
    {
        ///listposition
        var currentPosition = Vector3.zero;
        listAccumulate.Clear();
        foreach (var point in points)
        {
            currentPosition += point.offset;
            listAccumulate.Add(currentPosition);
        }
    }
    
    private void Reset()
    {
        points.Add(new CurveInfo(Vector3.zero));
        points.Add(new CurveInfo(Vector3.right));
        mr.material = new Material(Shader.Find("Standard"));
        ModifyMesh();

    }

    public void OnBeforeSerialize()
    {
       
    }

    public void OnAfterDeserialize()
    {
        InitData();
    }
}

[Serializable]
public struct CurveInfo
{
    public Vector3 offset;

    [SerializeField]
    Vector2 nextCurveOffset;
    [SerializeField]
    Vector2 previousCurveOffset;

    public Vector3 np {
        get { return new Vector3(nextCurveOffset.x, 0, nextCurveOffset.y); }
    }
    public Vector3 pp {
        get { return new Vector3(previousCurveOffset.x, 0, previousCurveOffset.y); }
    }
    [Range(2,100)]
    public int divideCount;
    [Range(0.1f,3f)]
    public float width;

    public CurveInfo(Vector3 offset)
    {
        this.offset = offset;
        divideCount = 1;
        width = 1f;
        nextCurveOffset = Vector2.zero;
        previousCurveOffset = Vector2.zero;
    }

}
