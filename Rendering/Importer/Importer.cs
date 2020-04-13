using System.Collections.Generic;
using Assimp;
using System.IO;

namespace Materia.Rendering.Importer
{
    public class Importer : IMeshImporter
    {
        public List<Geometry.Mesh> Parse(Stream stream)
        {
            List<Geometry.Mesh> meshes = new List<Geometry.Mesh>();
            using (AssimpContext ctx = new AssimpContext())
            {
                var scene = ctx.ImportFileFromStream(stream, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals);

                if (scene != null)
                {
                    meshes = Process(scene);
                }

                return meshes;
            }
        }

        public List<Geometry.Mesh> Parse(string path)
        {
            List<Geometry.Mesh> meshes = new List<Geometry.Mesh>();
            using (AssimpContext ctx = new AssimpContext())
            {
                var scene = ctx.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals);

                if (scene != null)
                {
                    meshes = Process(scene);
                }

                return meshes;
            }
        }

        protected List<Geometry.Mesh> Process(Scene scene)
        {
            List<Geometry.Mesh> meshes = new List<Geometry.Mesh>();
            if (scene.HasMeshes)
            {
                foreach (Assimp.Mesh m in scene.Meshes)
                {
                    if (m.HasNormals && m.HasTextureCoords(0) && m.HasVertices)
                    {
                        Geometry.Mesh ms = new Geometry.Mesh();

                        foreach (Vector3D v in m.Normals)
                        {
                            ms.normals.Add(new Mathematics.Vector3(v.X, v.Y, v.Z));
                            //we also add placeholders for tangents
                            ms.tangents.Add(new Mathematics.Vector4(0, 0, 0, 1));
                        }

                        foreach (Vector3D v in m.Vertices)
                        {
                            ms.vertices.Add(new Mathematics.Vector3(v.X, v.Y, v.Z));
                        }

                        foreach (Vector3D v in m.TextureCoordinateChannels[0])
                        {
                            ms.uv.Add(new Mathematics.Vector2(v.X, v.Y));
                        }


                        ms.indices = new List<int>(m.GetIndices());


                        //store faces for use with Mikktspace generation
                        //and for displaying UVs
                        foreach (Face f in m.Faces)
                        {
                            if (f.IndexCount == 3)
                            {
                                Geometry.Triangle t = new Geometry.Triangle();

                                t.n0 = t.v0 = t.u0 = f.Indices[0];
                                t.n1 = t.v1 = t.u1 = f.Indices[1];
                                t.n2 = t.v2 = t.u2 = f.Indices[2];

                                ms.AddTriangle(t);
                            }
                        }

                        //generate tangents using mikktspace
                        Geometry.Mikkt.GenTangents(ms);

                        meshes.Add(ms);
                    }
                }
            }
            return meshes;
        }
    }
}
