using System.Collections.Generic;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Geometry
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

            for(int i = 0; i < vertices.Count; ++i)
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
    }
}
