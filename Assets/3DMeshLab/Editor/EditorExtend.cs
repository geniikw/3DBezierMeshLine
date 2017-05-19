using UnityEngine;
using System.Collections;
using UnityEditor;
using GoodNightMypi.MeshObjectLab;

[CustomEditor(typeof(MeshBehaviour), true)]
public class MeshBehaviourEditor : Editor {

    MeshBehaviour owner { get { return target as MeshBehaviour; } }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Sync Mesh Collider"))
        {
            Undo.RecordObject(owner, "Sync Mesh Collider");
            owner.CreateMeshCollider();
        }
        if (GUILayout.Button("Generate"))
        {
            Undo.RecordObject(owner, "Generate");
            var mesh = owner.mf.sharedMesh;

            var path = EditorUtility.SaveFilePanelInProject("Save Procedural Mesh", "Procedural Mesh", "asset", "");
            AssetDatabase.CreateAsset(mesh, path);

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = mesh;
        }
    }
}


[ExecuteInEditMode]
[CustomEditor(typeof(Road))]
public class RoadEditor : MeshBehaviourEditor
{
    Road owner { get { return target as Road; } }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            owner.ModifyMesh();
            owner.CreateMeshCollider();
        }
    }

    void OnSceneGUI()
    {
        Transform ot = owner.transform;
        Vector3 cp = ot.position;
        int index = 0;

        EditorGUI.BeginChangeCheck();

        foreach (var p in owner.points)
        {
            cp += p.offset;

            Vector3 changeOffset = Vector3.zero;

            if (index != 0)
                changeOffset = Handles.PositionHandle(cp, Quaternion.identity) - cp + p.offset;


            if (p.offset != changeOffset)
            {
                serializedObject.FindProperty("points").GetArrayElementAtIndex(index).FindPropertyRelative("offset").vector3Value = changeOffset;
                serializedObject.ApplyModifiedProperties();
            }

            if (p.np.magnitude > 0.1f)
            {
                Handles.Label(cp + p.np, index + "next point");
                //Handles.DrawDottedLine(cp + p.np, cp, 1f);
                DrawDottedLineColor(cp + p.np, cp, Color.red);
                var changeNext = Handles.PositionHandle(cp + p.np, Quaternion.identity) - cp;

                if (p.np != changeNext)
                {
                    serializedObject.FindProperty("points").GetArrayElementAtIndex(index).FindPropertyRelative("nextCurveOffset").vector2Value
                        = new Vector2(changeNext.x, changeNext.z);
                    serializedObject.ApplyModifiedProperties();
                }

            }

            if (p.pp.magnitude > 0.1f)
            {
                Handles.Label(cp + p.pp, index + "prv point");

                //Handles.DrawDottedLine(cp + p.pp, cp, 1f);

                DrawDottedLineColor(cp + p.pp, cp, Color.blue);
                var changePrv = Handles.PositionHandle(cp + p.pp, Quaternion.identity) - cp;
                if (p.pp != changePrv)
                {
                    serializedObject.FindProperty("points").GetArrayElementAtIndex(index).FindPropertyRelative("previousCurveOffset").vector2Value
                        = new Vector2(changePrv.x, changePrv.z);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            Handles.Label(cp, index++ + " point");
        }

        if (EditorGUI.EndChangeCheck())
        {
            owner.ModifyMesh();
            owner.CreateMeshCollider();
        }


    }

    private static void DrawDottedLineColor(Vector3 p0, Vector3 p1, Color color)
    {
        var buffer = Handles.color;
        Handles.color = color;
        Handles.DrawDottedLine(p0, p1, 1f);
        Handles.color = buffer;
    }
}
