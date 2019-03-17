using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSMI.Containers;
using Assimp.Configs;
using Assimp;

namespace RSMI
{
    public class Importer : AbstractImporter
    {
        public override List<Containers.Mesh> Parse(string path)
        {
            List<Containers.Mesh> meshes = new List<Containers.Mesh>();
            AssimpContext ctx = new AssimpContext();

            var scene = ctx.ImportFile(path, PostProcessSteps.Triangulate);

            if(scene.HasMeshes)
            {
                foreach(Assimp.Mesh m in scene.Meshes)
                {
                    if(m.HasNormals && m.HasTextureCoords(0) && m.HasVertices)
                    {
                        Containers.Mesh ms = new Containers.Mesh();

                        foreach (Vector3D v in m.Normals)
                        {
                            ms.normals.Add(new OpenTK.Vector3(v.X, v.Y, v.Z));
                            //we also add placeholders for tangents
                            ms.tangents.Add(new OpenTK.Vector4(0, 0, 0, 1));
                        }

                        foreach(Vector3D v in m.Vertices)
                        {
                            ms.vertices.Add(new OpenTK.Vector3(v.X, v.Y, v.Z));
                        }

                        foreach(Vector3D v in m.TextureCoordinateChannels[0])
                        {
                            ms.uv.Add(new OpenTK.Vector2(v.X, v.Y));
                        }

                        
                        ms.indices = new List<int>(m.GetIndices());


                        //store faces for use with Mikktspace generation
                        //and for displaying UVs
                        foreach(Face f in m.Faces)
                        {
                            if (f.IndexCount == 3)
                            {
                                Triangle t = new Triangle();

                                t.n0 = t.v0 = t.u0 = f.Indices[0];
                                t.n1 = t.v1 = t.u1 = f.Indices[1];
                                t.n2 = t.v2 = t.u2 = f.Indices[2];

                                ms.AddTriangle(t);
                            }
                        }

                        //generate tangents using mikktspace
                        Mikkt.GenTangents(ms);

                        meshes.Add(ms);
                    }
                }
            }

            ctx.Dispose();

            return meshes;
        }
    }
}
