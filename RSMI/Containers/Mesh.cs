using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace RSMI.Containers
{
    public class Mesh
    {
        public List<Triangle> triangles;
        public List<Vector3> vertices;
        public List<Vector3> normals;
        public List<Vector2> uv;
        public List<Vector4> tangents;
        public List<int> indices;

        public bool IsValid
        {
            get
            {
                return vertices.Count == normals.Count && vertices.Count == tangents.Count;
            }
        }

        public Mesh()
        {
            triangles = new List<Triangle>();
            vertices = new List<Vector3>();
            normals = new List<Vector3>();
            uv = new List<Vector2>();
            tangents = new List<Vector4>();
            indices = new List<int>();
        }

        public Vector3 GetPosition(int face, int vert)
        {
            Vector3 v = Vector3.Zero;
            if (face >= 0 && face < triangles.Count)
            {
                Triangle t = triangles[face];

                switch (vert)
                {
                    case 0:
                        v = vertices[t.v0];
                        break;
                    case 1:
                        v = vertices[t.v1];
                        break;
                    case 2:
                        v = vertices[t.v2];
                        break;
                }

            }

            return v;
        }

        public Vector3 GetNormal(int face, int vert)
        {
            Vector3 v = Vector3.Zero;
            if (face >= 0 && face < triangles.Count)
            {
                Triangle t = triangles[face];

                switch (vert)
                {
                    case 0:
                        v = normals[t.n0];
                        break;
                    case 1:
                        v = normals[t.n1];
                        break;
                    case 2:
                        v = normals[t.n2];
                        break;
                }

            }

            return v;
        }

        public Vector2 GetUV(int face, int vert)
        {
            Vector2 v = Vector2.Zero;
            if (face >= 0 && face < triangles.Count)
            {
                Triangle t = triangles[face];

                switch (vert)
                {
                    case 0:
                        v = uv[t.u0];
                        break;
                    case 1:
                        v = uv[t.u1];
                        break;
                    case 2:
                        v = uv[t.u2];
                        break;
                }

            }

            return v;
        }

        public void SetTangent(float[] tan, float w, int face, int vert)
        {
            if(face >= 0 && face < triangles.Count)
            {
                Triangle f = triangles[face];
                int index = -1;

                switch(vert)
                {
                    case 0:
                        index = f.v0;
                        break;
                    case 1:
                        index = f.v1;
                        break;
                    case 2:
                        index = f.v2;
                        break;
                }

                if(index > -1)
                {
                    tangents[index] = new Vector4(tan[0], tan[1], tan[2], w);
                }
            }
        }

        public float[] Compact()
        {
            List<float> buffer = new List<float>();

            for(int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                Vector2 u = uv[i];
                Vector3 n = normals[i];
                Vector4 t = tangents[i];

                buffer.Add(v.X);
                buffer.Add(v.Y);
                buffer.Add(v.Z);
                buffer.Add(u.X);
                buffer.Add(u.Y);
                buffer.Add(n.X);
                buffer.Add(n.Y);
                buffer.Add(n.Z);
                buffer.Add(t.X);
                buffer.Add(t.Y);
                buffer.Add(t.Z);
                buffer.Add(t.W);
            }

            return buffer.ToArray();
        }

        public void AddTriangle(List<Triangle> ts)
        {
            foreach(Triangle t in ts)
            {
                AddTriangle(t);
            }
        }

        public void AddTriangle(Triangle[] ts)
        {
            foreach(Triangle t in ts)
            {
                AddTriangle(t);
            }
        }

        public void AddTriangle(Triangle t)
        {
            triangles.Add(t);
        }

        public void CalculateTangents()
        {
            if (uv.Count == 0 || normals.Count == 0) return;

            Vector4[] tans = new Vector4[vertices.Count];
            for(int i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];

                Vector3 v0 = vertices[t.v0];
                Vector3 v1 = vertices[t.v1];
                Vector3 v2 = vertices[t.v2];

                Vector3 n0 = normals[t.n0];
                Vector3 n1 = normals[t.n1];
                Vector3 n2 = normals[t.n2];

                Vector2 tex0 = uv[t.u0];
                Vector2 tex1 = uv[t.u1];
                Vector2 tex2 = uv[t.u2];

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;

                Vector2 uv1 = tex1 - tex0;
                Vector2 uv2 = tex2 - tex0;

                float r = 1.0f / (uv1.X * uv2.Y - uv1.Y * uv2.X);

                Vector3 tan = new Vector3(
                                            ((edge1.X * uv2.Y) - (edge2.X * uv1.Y)) * r,
                                            ((edge1.Y * uv2.Y) - (edge2.Y * uv1.Y)) * r,
                                            ((edge1.Z * uv2.Y) - (edge2.Z * uv1.Y)) * r
                                          );

                Vector3 bitan = new Vector3(
                                            ((edge1.X * uv2.X) - (edge2.X * uv1.X)) * r,
                                            ((edge1.Y * uv2.X) - (edge2.Y * uv1.X)) * r,
                                            ((edge1.Z * uv2.X) - (edge2.Z * uv1.X)) * r
                                            );

                //calculate each tangent for each normal
                Vector4 t0 = CalculateTangent(tan, bitan, n0);
                Vector4 t1 = CalculateTangent(tan, bitan, n1);
                Vector4 t2 = CalculateTangent(tan, bitan, n2);

                tans[t.v0] = t0;
                tans[t.v1] = t1;
                tans[t.v2] = t2;

                t.t0 = t.v0;
                t.t1 = t.v1;
                t.t2 = t.v2;
            }

            tangents = new List<Vector4>(tans);
        }

        Vector4 CalculateTangent(Vector3 tan, Vector3 bitan, Vector3 n)
        {
            Vector3 tn0 = tan - (n * Vector3.Dot(n, tan));
            tn0.Normalize();

            Vector3 c = Vector3.Cross(n, tan);
            float w = (Vector3.Dot(c, bitan) < 0) ? -1.0f : 1.0f;
            return new Vector4(tn0.X, tn0.Y, tn0.Z, w);
        }
    }
}
