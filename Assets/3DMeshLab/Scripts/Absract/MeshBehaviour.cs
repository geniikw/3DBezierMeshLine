using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


namespace GoodNightMypi.MeshObjectLab
{
    /// <summary>
    /// mesh를 코드로 만들때 쓰임.
    /// child는 Modify함수에서 Mesh를 구현해야함.
    /// 필요한 함수는 Add로 시작한다.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class MeshBehaviour : MonoBehaviour
    {
        public MeshFilter mf { get { return GetComponent<MeshFilter>(); } }
        public MeshRenderer mr { get { return GetComponent<MeshRenderer>(); } }
        /// <summary>
        /// StartEdit->true
        /// EndEdit->false
        /// </summary>
        private bool m_isEditing = false;

        private List<Vector3> vertexBuffer = new List<Vector3>();
        private List<int> triangleBuffer = new List<int>();
        private List<Vector2> uvBuffer = new List<Vector2>();
        private List<Vector3> normalsBuffer = new List<Vector3>();

        public Mesh mesh
        {
            get { return mf.sharedMesh; }
            set { mf.sharedMesh = value; }
        }

        /******private******/
        void CheckEdit()
        {
            if (!m_isEditing)
                throw new Exception("Function making a vertex is only used in Modify().");
        }
      
        #region AddVertexFunctions(Private)
        private void DrawTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            var idx = vertexBuffer.Count;
            vertexBuffer.AddRange(new Vector3[] { a, b, c });
            triangleBuffer.AddRange(new int[] { idx, idx + 1, idx + 2 });

            var normal = Vector3.Cross(a - b, b - c);
            normalsBuffer.AddRange(Enumerable.Repeat(normal, 3));
        }
        private void DrawQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var idx = vertexBuffer.Count;

            vertexBuffer.AddRange(new Vector3[] { a, b, c, d });
            uvBuffer.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) });

            triangleBuffer.AddRange(new int[] { idx, idx + 1, idx + 2 });
            triangleBuffer.AddRange(new int[] { idx, idx + 2, idx + 3 });

            var normal = Vector3.Cross(a - b, b - c);
            normalsBuffer.AddRange(Enumerable.Repeat(normal, 4));
        }
        #endregion

        #region AddPrimitiveFunctions(protected)

        /******protected******/
        /// <param name="a">+++</param>
        /// <param name="b">+-+</param>
        /// <param name="c">--+</param>
        /// <param name="d">-++</param>
        /// <param name="e">++-</param>
        /// <param name="f">+--</param>
        /// <param name="g">---</param>
        /// <param name="h">-+-</param>
        protected void AddCube(Vector3 a, Vector3 b, Vector3 c, Vector3 d,
                                Vector3 e, Vector3 f, Vector3 g, Vector3 h, bool front = true, bool back = true)
        {
            CheckEdit();
            if (front)
                DrawQuad(e, f, g, h);
            if (back)
                DrawQuad(d, c, b, a);

            DrawQuad(a, b, f, e);
            DrawQuad(c, d, h, g);
            DrawQuad(d, a, e, h);
            DrawQuad(b, c, g, f);
        }

        protected void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            CheckEdit();
            DrawQuad(a, b, c, d);
        }

        protected void AddQuad(Vector3 center, float halfSide, bool reverse = false)
        {
            CheckEdit();
            var a = center + new Vector3(halfSide, halfSide, 0);
            var b = center + new Vector3(halfSide, -halfSide, 0);
            var c = center + new Vector3(-halfSide, -halfSide, 0);
            var d = center + new Vector3(-halfSide, halfSide, 0);

            if (reverse)
                DrawQuad(d, c, b, a);
            else
                DrawQuad(a, b, c, d);

        }

        protected void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            CheckEdit();

            DrawTriangle(a, b, c);
        }

        protected void AddCube(Vector3 center, float side, bool front = true, bool back = true)
        {
            CheckEdit();

            List<Vector3> vtx = new List<Vector3>();
            var hs = side / 2f;
            vtx.Add(new Vector3(hs, hs, hs) + center);  //0
            vtx.Add(new Vector3(hs, -hs, hs) + center); //1

            vtx.Add(new Vector3(-hs, -hs, hs) + center);//2
            vtx.Add(new Vector3(-hs, hs, hs) + center); //3

            vtx.Add(new Vector3(hs, hs, -hs) + center); //4
            vtx.Add(new Vector3(hs, -hs, -hs) + center);//5

            vtx.Add(new Vector3(-hs, -hs, -hs) + center);//6
            vtx.Add(new Vector3(-hs, hs, -hs) + center);//7

            AddCube(vtx[0], vtx[1], vtx[2], vtx[3], vtx[4],
                     vtx[5], vtx[6], vtx[7], front, back);
        }

        protected void AddCube(Vector3 s, Vector3 e,
                                   float w, bool front = false, bool back = false)
        {
            CheckEdit();
            List<Vector3> vtx = new List<Vector3>();
            var yAngle = new Vector3(0, 1, 0);
            var xAngle = Vector3.Cross(s - e, Vector3.up).normalized;

            //vtx.Add(e + new Vector3(width, width));//++
            //vtx.Add(e + new Vector3(width, -width));//+-

            //vtx.Add(e + new Vector3(-width, -width));//--
            //vtx.Add(e + new Vector3(-width, width));//-+

            //vtx.Add(s + new Vector3(width, width));//++
            //vtx.Add(s + new Vector3(width, -width));//+-

            //vtx.Add(s + new Vector3(-width, -width));
            //vtx.Add(s + new Vector3(-width, width));

            vtx.Add(e + xAngle * w + yAngle * w);//++yAngle
            vtx.Add(e + xAngle * w - yAngle * w);//++yAngle

            vtx.Add(e - xAngle * w - yAngle * w);//++yAngle
            vtx.Add(e - xAngle * w + yAngle * w);//++yAngle

            vtx.Add(s + xAngle * w + yAngle * w);//++yAngle
            vtx.Add(s + xAngle * w - yAngle * w);//++yAngle

            vtx.Add(s - xAngle * w - yAngle * w);//++yAngle
            vtx.Add(s - xAngle * w + yAngle * w);//++yAngle

            AddCube(vtx[0], vtx[1], vtx[2], vtx[3], vtx[4],
                     vtx[5], vtx[6], vtx[7], front, back);
        }

              
        //x,z 평면에 베지어 라인을 그린다. 
        //맨 마지막 vertex 위치 2개를 리턴한다.
        protected Vector3[] AddCubicBezierLine(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
                        float zWidth = 1f, float sWidth = 1f, float eWidth = 1f, int divide = 1, Vector3 prvVector1 = default(Vector3), Vector3 prvVector2 = default(Vector3))
        {
            CheckEdit();
            if (divide <= 0)
                throw new Exception("divide 값은 0보다 커야함. divide : " + divide);

            Vector3 vBuffer1 = prvVector1;
            Vector3 vBuffer2 = prvVector2;

            for (int n = 0; n < divide; n++)
            {

                float st = n / (float)divide;
                float et = (n + 1) / (float)divide;

                var sw = Mathf.Lerp(sWidth, eWidth, st);
                var ew = Mathf.Lerp(sWidth, eWidth, et);

                var s = Curve.Cubic(p0, p1, p2, p3, st);
                var e = Curve.Cubic(p0, p1, p2, p3, et);

                var wV = Curve.GetXZOthogonalVector(s, e);

                var dV = Vector3.down * zWidth;
                if (n == 0 && (prvVector1 == default(Vector3) && prvVector2 == default(Vector3)))
                {
                    vBuffer1 = s - wV * sw;
                    vBuffer2 = s + wV * sw;
                    //start front
                    DrawQuad(vBuffer1, vBuffer1 + dV, vBuffer2 + dV, vBuffer2);
                }
                var next1 = e + ew * wV;
                var next2 = e - ew * wV;
                //up
                DrawQuad(vBuffer1, vBuffer2, next1, next2);


                //right left
                DrawQuad(vBuffer2, vBuffer2 + dV, next1 + dV, next1);
                DrawQuad(vBuffer1, next2, next2 + dV, vBuffer1 + dV);

                //down
                DrawQuad(vBuffer1 + dV, next2 + dV, next1 + dV, vBuffer2 + dV);

                //end back
                if (n == divide - 1)
                {
                    DrawQuad(next2, next1, next1 + dV, next2 + dV);
                }

                vBuffer1 = next2;
                vBuffer2 = next1;
            }

            return new Vector3[] { vBuffer1, vBuffer2 };
        }

        #endregion
        protected abstract void Modify();

        /******public******/
        public Vector3[] GetLastVertexPosition(int count)
        {
            return vertexBuffer.GetRange(vertexBuffer.Count - count, count).ToArray();
        }
        
        public void ModifyMesh()
        {
            m_isEditing = true;
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = transform.name;
            }

            mesh.Clear();
            vertexBuffer.Clear();
            triangleBuffer.Clear();
            uvBuffer.Clear();
            normalsBuffer.Clear();

            Modify();

            mesh.vertices = vertexBuffer.ToArray();
            mesh.triangles = triangleBuffer.ToArray();
            mesh.uv = uvBuffer.ToArray();
            mesh.normals = normalsBuffer.ToArray();

            m_isEditing = false;
        }

        public void CreateMeshCollider()
        {
            var meshCol = GetComponent<MeshCollider>();
            if (meshCol != null)
            {
                meshCol.sharedMesh = mesh;
            }
        }
             
    }
}