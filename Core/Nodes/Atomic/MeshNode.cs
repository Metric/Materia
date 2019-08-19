using Materia.Geometry;
using Materia.Imaging.GLProcessing;
using Materia.Material;
using Materia.Nodes.Attributes;
using RSMI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Newtonsoft.Json;
using System.Threading;
using Materia.Math3D;
using Materia.MathHelpers;
using Materia.Archive;

namespace Materia.Nodes.Atomic
{
    public class MeshNode : ImageNode
    {
        public delegate void HdriChange();
        public static event HdriChange OnHdriChanged;

        public static GLTextuer2D Irradiance { get; set; }

        //we only trigger it on one of these
        //due to both usually being set at the same time
        protected static GLTextuer2D prefilter;
        public static GLTextuer2D Prefilter
        {
            get
            {
                return prefilter;
            }
            set
            {
                prefilter = value;
                if(OnHdriChanged != null)
                {
                    OnHdriChanged.Invoke();
                }
            }
        }

        private MTGArchive archive;

        public static GLTextuer2D DefaultBlack { get; set; }
        public static GLTextuer2D DefaultDarkGray { get; set; }
        public static GLTextuer2D DefaultWhite { get; set; }

        CancellationTokenSource ctk;

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
                TryAndProcess();
            }
        }

        [Editable(ParameterInputType.Toggle, "Resource")]
        public bool Resource
        {
            get; set;
        }

        MVector position;
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
                TryAndProcess();
            }
        }

        MVector rotation;
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
                TryAndProcess();
            }
        }

        float cameraZoom;

        MVector scale;
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
                TryAndProcess();
            }
        }

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
                TryAndProcess();
            }
        }

        float meshtileX;
        float meshtileY;

        [Editable(ParameterInputType.FloatInput, "Mesh Texture Tile X")]
        public float MeshTileX
        {
            get
            {
                return meshtileX;
            }
            set
            {
                meshtileX = value;
                TryAndProcess();
            }
        }

        [Editable(ParameterInputType.FloatInput, "Mesh Texture Tile Y")]
        public float MeshTileY
        {
            get
            {
                return meshtileY;
            }
            set
            {
                meshtileY = value;
                TryAndProcess();
            }
        }

        MeshProcessor processor;
        MeshRenderer mesh;
        NodeOutput Output;

        NodeInput inputAlbedo;
        NodeInput inputMetallic;
        NodeInput inputRoughness;
        NodeInput inputOcclusion;
        NodeInput inputHeight;
        NodeInput inputNormal;
        NodeInput inputThickness;

        static Matrix4 Proj = Matrix4.CreatePerspectiveFieldOfView(40 * ((float)Math.PI / 180.0f), 1, 0.03f, 1000.0f);
        static PBRMaterial mat = new PBRMaterial();

        public MeshNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Mesh";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            tileX = tileY = 1;

            meshtileX = meshtileY = 1;

            scale = new MVector(1, 1, 1);
            rotation = new MVector(0, 0, 0);
            position = new MVector(0, 0, 0);
            cameraZoom = 3;

            previewProcessor = new BasicImageRenderer();
            processor = new MeshProcessor();

            internalPixelType = p;

            Inputs = new List<NodeInput>();

            inputAlbedo = new NodeInput(NodeType.Color, this, "Albedo");
            inputHeight = new NodeInput(NodeType.Gray, this, "Height");
            inputMetallic = new NodeInput(NodeType.Gray, this, "Metallic");
            inputRoughness = new NodeInput(NodeType.Gray, this, "Roughness");
            inputOcclusion = new NodeInput(NodeType.Gray, this, "Occlusion");
            inputNormal = new NodeInput(NodeType.Color, this, "Normal");
            inputThickness = new NodeInput(NodeType.Gray, this, "Thickness");

            Inputs.Add(inputAlbedo);
            Inputs.Add(inputMetallic);
            Inputs.Add(inputRoughness);
            Inputs.Add(inputNormal);
            Inputs.Add(inputHeight);
            Inputs.Add(inputOcclusion);
            Inputs.Add(inputThickness);

            inputAlbedo.OnInputAdded += Input_OnInputAdded;
            inputAlbedo.OnInputChanged += Input_OnInputChanged;
            inputAlbedo.OnInputRemoved += Input_OnInputRemoved;

            inputNormal.OnInputAdded += Input_OnInputAdded;
            inputNormal.OnInputChanged += Input_OnInputChanged;
            inputNormal.OnInputRemoved += Input_OnInputRemoved;

            inputOcclusion.OnInputAdded += Input_OnInputAdded;
            inputOcclusion.OnInputChanged += Input_OnInputChanged;
            inputOcclusion.OnInputRemoved += Input_OnInputRemoved;

            inputRoughness.OnInputAdded += Input_OnInputAdded;
            inputRoughness.OnInputChanged += Input_OnInputChanged;
            inputRoughness.OnInputRemoved += Input_OnInputRemoved;

            inputMetallic.OnInputAdded += Input_OnInputAdded;
            inputMetallic.OnInputChanged += Input_OnInputChanged;
            inputMetallic.OnInputRemoved += Input_OnInputRemoved;

            inputHeight.OnInputAdded += Input_OnInputAdded;
            inputHeight.OnInputChanged += Input_OnInputChanged;
            inputHeight.OnInputRemoved += Input_OnInputRemoved;

            inputThickness.OnInputAdded += Input_OnInputAdded;
            inputThickness.OnInputChanged += Input_OnInputChanged;
            inputThickness.OnInputRemoved += Input_OnInputRemoved;

            Output = new NodeOutput(NodeType.Gray, this);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);

            OnHdriChanged += MeshNode_OnHdriChanged;
        }

        private void MeshNode_OnHdriChanged()
        {
            TryAndProcess();
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if(!Async)
            {
                ReadMeshFile();
                LoadMesh();
                Process();
                return;
            }

            //if (ctk != null)
            //{
            //    ctk.Cancel();
            //}

            //ctk = new CancellationTokenSource();

            //Task.Delay(25, ctk.Token).ContinueWith(t =>
            //{
            //    if (t.IsCanceled) return;

                if (ParentGraph != null)
                {
                    ParentGraph.Schedule(this);
                }
            //}, Context);
        }

        public override Task GetTask()
        {
            return Task.Factory.StartNew(() =>
            {
                ReadMeshFile();
            })
            .ContinueWith(t =>
            {
                LoadMesh();
                Process();
            }, Context);
        }

        private void ReadMeshFile()
        {
            if (mesh == null && meshes == null)
            {
                if(archive != null && !string.IsNullOrEmpty(relativePath) && Resource)
                {
                    archive.Open();
                    List<MTGArchive.ArchiveFile> files = archive.GetAvailableFiles();

                    var m = files.Find(f => f.path.Equals(relativePath));
                    if (m != null)
                    {
                        using (Stream ms = m.GetStream())
                        {
                            RSMI.Importer imp = new Importer();
                            meshes = imp.Parse(ms);
                            archive.Close();
                            return;
                        }
                    }

                    archive.Close();
                }

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    RSMI.Importer imp = new Importer();
                    meshes = imp.Parse(path);
                }
                else if (!string.IsNullOrEmpty(relativePath) && ParentGraph != null && !string.IsNullOrEmpty(ParentGraph.CWD) && File.Exists(System.IO.Path.Combine(ParentGraph.CWD, relativePath)))
                {
                    var p = System.IO.Path.Combine(ParentGraph.CWD, relativePath);

                    RSMI.Importer imp = new Importer();
                    meshes = imp.Parse(p);
                }
            }
        }

        private void LoadMesh()
        {
            if (string.IsNullOrEmpty(path))
            {
                if (mesh != null)
                {
                    mesh.Release();
                }

                mesh = null;
            }

            if (meshes != null && meshes.Count > 0)
            {
                mesh = new MeshRenderer(meshes[0]);
                meshes.Clear();
                meshes = null;
            }
        }

        List<RSMI.Containers.Mesh> meshes;
        void Process()
        {
            if (mesh == null) return;

            CreateBufferIfNeeded();

            mesh.Mat = mat;

            float rx = (float)this.rotation.X * ((float)Math.PI / 180.0f);
            float ry = (float)this.rotation.Y * ((float)Math.PI / 180.0f);
            float rz = (float)this.rotation.Z * ((float)Math.PI / 180.0f);

            Quaternion rot = Quaternion.FromEulerAngles(rx, ry, rz);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(rot);
            Matrix4 translation = Matrix4.CreateTranslation(position.X, position.Y, position.Z);
            Matrix4 scale = Matrix4.CreateScale(this.scale.X, this.scale.Y, this.scale.Z);

            Matrix4 view = rotation * Matrix4.CreateTranslation(0, 0, -cameraZoom);
            Vector3 pos = Vector3.Normalize((view * new Vector4(0, 0, 1, 1)).Xyz) * cameraZoom;

            GLTextuer2D albedo = (inputAlbedo.HasInput) ? (GLTextuer2D)inputAlbedo.Input.Data : null;
            GLTextuer2D metallic = (inputMetallic.HasInput) ? (GLTextuer2D)inputMetallic.Input.Data : null;
            GLTextuer2D roughness = (inputRoughness.HasInput) ? (GLTextuer2D)inputRoughness.Input.Data : null;
            GLTextuer2D normal = (inputNormal.HasInput) ? (GLTextuer2D)inputNormal.Input.Data : null;
            GLTextuer2D heightm = (inputHeight.HasInput) ? (GLTextuer2D)inputHeight.Input.Data : null;
            GLTextuer2D occlusion = (inputOcclusion.HasInput) ? (GLTextuer2D)inputOcclusion.Input.Data : null;
            GLTextuer2D thickness = (inputThickness.HasInput) ? (GLTextuer2D)inputThickness.Input.Data : null;

            mesh.IrradianceMap = Irradiance;
            mesh.PrefilterMap = Prefilter;

            if (albedo == null)
            {
                albedo = DefaultDarkGray;
            }
            if (metallic == null)
            {
                metallic = DefaultBlack;
            }
            if (roughness == null)
            {
                roughness = DefaultBlack;
            }
            if (heightm == null)
            {
                heightm = DefaultWhite;
            }
            if (occlusion == null)
            {
                occlusion = DefaultWhite;
            }
            if (normal == null)
            {
                normal = DefaultBlack;
            }
            if (thickness == null)
            {
                thickness = DefaultBlack;
            }

            mat.Albedo = albedo;
            mat.Height = heightm;
            mat.Normal = normal;
            mat.Metallic = metallic;
            mat.Occlusion = occlusion;
            mat.Roughness = roughness;
            mat.Thickness = thickness;

            mesh.View = view;
            mesh.CameraPosition = pos;
            mesh.Projection = Proj;

            mesh.Tiling = new Vector2(meshtileX, meshtileY);

            //TRS
            mesh.Model = scale * translation;

            //light position currently doesn't do anything
            //just setting values to a default
            mesh.LightPosition = new Vector3(0, 0, 0);
            mesh.LightColor = new Vector3(1, 1, 1);

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Mesh = mesh;
            processor.Process(width, height, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public class MeshNodeData : NodeData
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

            public float meshTileX;
            public float meshTileY;
        }

        public override void FromJson(string data, MTGArchive arch = null)
        {
            archive = arch;
            FromJson(data);
        }

        public override void FromJson(string data)
        {
            MeshNodeData d = JsonConvert.DeserializeObject<MeshNodeData>(data);
            SetBaseNodeDate(d);

            path = d.path;
            Resource = d.resource;
            relativePath = d.relativePath;

            position = new MVector(d.translateX, d.translateY, d.translateZ);
            scale = new MVector(d.scaleX, d.scaleY, d.scaleZ);
            rotation = new MVector(d.rotationX, d.rotationY, d.rotationZ);

            cameraZoom = d.cameraZoom;

            meshtileX = d.meshTileX;
            meshtileY = d.meshTileY;
        }

        public override string GetJson()
        {
            MeshNodeData d = new MeshNodeData();
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
            d.meshTileX = meshtileX;
            d.meshTileY = meshtileY;


            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public override void CopyResources(string CWD)
        {
            if (!Resource) return;

            CopyResourceTo(CWD, relativePath, path);
        }

        public override void Dispose()
        {
            OnHdriChanged -= MeshNode_OnHdriChanged;

            base.Dispose();

            if (mesh != null)
            {
                mesh.Release();
                mesh = null;
            }

            if (processor != null)
            {
                processor.Release();
            }
        }
    }
}
