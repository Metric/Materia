using System;
using System.Collections.Generic;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Geometry;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Importer;
using System.IO;
using Materia.Rendering.Material;
using Materia.Rendering.Mathematics;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public class MeshDepthNode : ImageNode
    {
        private Archive archive;

        string relativePath;
        string path;
        [Editable(ParameterInputType.MeshFile, "Mesh File", "Content")]
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
                if (string.IsNullOrEmpty(path))
                {
                    relativePath = "";
                }
                else
                {
                    relativePath = System.IO.Path.Combine("resources", System.IO.Path.GetFileName(path));
                }
                TriggerValueChange();
            }
        }

        [Editable(ParameterInputType.Toggle, "Resource", "Content")]
        public bool Resource
        {
            get; set;
        }

        MVector position;
        [Promote(NodeType.Float3)]
        [Editable(ParameterInputType.Float3Input, "Position")]
        public MVector Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                TriggerValueChange();
            }
        }

        MVector rotation;
        [Promote(NodeType.Float3)]
        [Editable(ParameterInputType.Float3Slider, "Rotation", "Default", 0, 360)]
        public MVector Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                TriggerValueChange();
            }
        }

        MVector scale;
        [Promote(NodeType.Float3)]
        [Editable(ParameterInputType.Float3Input, "Scale")]
        public MVector Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                TriggerValueChange();
            }
        }

        float cameraZoom;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatInput, "Camera Z")]
        public float CameraZ
        {
            get
            {
                return cameraZoom;
            }
            set
            {
                cameraZoom = value;
                TriggerValueChange();
            }
        }

        MeshDepthProcessor processor;
        MeshRenderer mesh;
        NodeOutput Output;

        static Matrix4 Proj = Matrix4.CreatePerspectiveFieldOfView(40 * ((float)Math.PI / 180.0f), 1, 0.03f, 1000.0f);
        static PBRMaterial mat = new PBRDepth();

        public MeshDepthNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Mesh Depth";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            tileX = tileY = 1;

            scale = new MVector(1, 1, 1);
            position = new MVector(0, 0, 0);
            rotation = new MVector(0, 0, 0);
            cameraZoom = 3;

            processor = new MeshDepthProcessor();

            internalPixelType = p;

            Output = new NodeOutput(NodeType.Gray, this);
            Outputs.Add(Output);
        }

        private void ReadMeshFile()
        {
            if (mesh == null && meshes == null)
            {
                if (archive != null && !string.IsNullOrEmpty(relativePath) && Resource)
                {
                    archive.Open();
                    List<Archive.ArchiveFile> files = archive.GetAvailableFiles();

                    var m = files.Find(f => f.path.Equals(relativePath));
                    if (m != null)
                    {
                        using (Stream ms = m.GetStream())
                        {
                            Importer imp = new Importer();
                            meshes = imp.Parse(ms);
                            archive.Close();
                            return;
                        }
                    }

                    archive.Close();
                }


                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    Importer imp = new Importer();
                    meshes = imp.Parse(path);
                }
                else if (!string.IsNullOrEmpty(relativePath) && ParentGraph != null && !string.IsNullOrEmpty(ParentGraph.CWD) && File.Exists(System.IO.Path.Combine(ParentGraph.CWD, relativePath)))
                {
                    var p = System.IO.Path.Combine(ParentGraph.CWD, relativePath);

                    Importer imp = new Importer();
                    meshes = imp.Parse(p);
                }
            }
        }

        private void LoadMesh()
        {
            if(string.IsNullOrEmpty(path))
            {
                if(mesh != null)
                {
                    mesh.Dispose();
                }

                mesh = null;
            }

            if(meshes != null && meshes.Count > 0)
            {
                mesh = new MeshRenderer(meshes[0]);
                meshes.Clear();
                meshes = null;
            }
        }

        public override void TryAndProcess()
        {
            ReadMeshFile();
            LoadMesh();
            Process();
        }

        List<Mesh> meshes;
        void Process()
        {
            if (processor == null) return;
            if (mesh == null) return;

            CreateBufferIfNeeded();

            mesh.Mat = mat;

            MVector prot = GetParameter("Rotation", rotation) * MathHelper.Deg2Rad;
            MVector pscale = GetParameter("Scale", scale);
            MVector pposition = GetParameter("Position", position);
            float pzoom = GetParameter("CameraZ", cameraZoom);

            Quaternion rot = Quaternion.FromEulerAngles(prot.ToVector3());
            Matrix4 irot = Matrix4.CreateFromQuaternion(rot);
            Matrix4 itrans = Matrix4.CreateTranslation(pposition.ToVector3());
            Matrix4 iscale = Matrix4.CreateScale(pscale.ToVector3());

            Matrix4 view = Matrix4.CreateTranslation(new Vector3(0, 0, pzoom));

            mesh.View = view;
            mesh.CameraPosition = new Vector3(0, 0, pzoom);
            mesh.Projection = Proj;

            //TRS
            mesh.Model = iscale * irot * itrans;

            //light position currently doesn't do anything
            //just setting values to a default
            mesh.LightPosition = new Vector3(0, 0, 0);
            mesh.LightColor = new Vector3(1, 1, 1);

            processor.Tiling = GetTiling();
            processor.Mesh = mesh;

            processor.Process(buffer);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class MeshDepthNodeData : NodeData
        {
            public string path;
            public string relativePath;
            public bool resource;

            public float translateX;
            public float translateY;
            public float translateZ;

            public float scaleX;
            public float scaleY;
            public float scaleZ;

            public float rotationX;
            public float rotationY;
            public float rotationZ;

            public float cameraZoom;
        }

        public override void FromJson(string data, Archive arch = null)
        {
            archive = arch;
            FromJson(data);
        }

        public override void FromJson(string data)
        {
            MeshDepthNodeData d = JsonConvert.DeserializeObject<MeshDepthNodeData>(data);
            SetBaseNodeDate(d);

            path = d.path;
            Resource = d.resource;
            relativePath = d.relativePath;

            position = new MVector(d.translateX, d.translateY, d.translateZ);
            scale = new MVector(d.scaleX, d.scaleY, d.scaleZ);
            rotation = new MVector(d.rotationX, d.rotationY, d.rotationZ);

            cameraZoom = d.cameraZoom;
        }

        public override string GetJson()
        {
            MeshDepthNodeData d = new MeshDepthNodeData();
            FillBaseNodeData(d);

            d.path = path;
            d.relativePath = relativePath;
            d.resource = Resource;

            d.translateX = position.X;
            d.translateY = position.Y;
            d.translateZ = position.Z;

            d.scaleX = scale.X;
            d.scaleY = scale.Y;
            d.scaleZ = scale.Z;

            d.rotationX = rotation.X;
            d.rotationY = rotation.Y;
            d.rotationZ = rotation.Z;

            d.cameraZoom = cameraZoom;

            return JsonConvert.SerializeObject(d);
        }

        public override void CopyResources(string CWD)
        {
            if (!Resource) return;

            CopyResourceTo(CWD, relativePath, path);
        }

        public override void Dispose()
        {
            base.Dispose();

            mesh?.Dispose();
            mesh = null;

            processor?.Dispose();
            processor = null;
        }
    }
}
