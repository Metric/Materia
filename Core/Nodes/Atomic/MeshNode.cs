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

        public static GLTextuer2D DefaultBlack { get; set; }
        public static GLTextuer2D DefaultDarkGray { get; set; }
        public static GLTextuer2D DefaultWhite { get; set; }

        CancellationTokenSource ctk;

        string relativePath;

        string path;
        [Section(Section = "Content")]
        [Title(Title = "Mesh File")]
        [FileSelector(Filter = "FBX & OBJ (*.fbx;*.obj)|*.fbx;*.obj")]
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

        [Section(Section = "Content")]
        public bool Resource
        {
            get; set;
        }

        float xOffset;
        float yOffset;
        float zOffset;

        [Title(Title = "Offset X")]
        public float TranslateX
        {
            get
            {
                return xOffset;
            }
            set
            {
                xOffset = value;
                TryAndProcess();
            }
        }

        [Title(Title = "Offset Y")]
        public float TranslateY
        {
            get
            {
                return yOffset;
            }
            set
            {
                yOffset = value;
                TryAndProcess();
            }
        }

        [Title(Title = "Offset Z")]
        public float TranslateZ
        {
            get
            {
                return zOffset;
            }
            set
            {
                zOffset = value;
                TryAndProcess();
            }
        }

        int rotationX;
        int rotationY;
        int rotationZ;

        [Slider(IsInt = true, Max = 360, Min = 0, Snap = false, Ticks = new float[0])]
        [Title(Title = "Rotate X")]
        public int RotationX
        {
            get
            {
                return rotationX;
            }
            set
            {
                rotationX = value;
                TryAndProcess();
            }
        }

        [Slider(IsInt = true, Max = 360, Min = 0, Snap = false, Ticks = new float[0])]
        [Title(Title = "Rotate Y")]
        public int RotationY
        {
            get
            {
                return rotationY;
            }
            set
            {
                rotationY = value;
                TryAndProcess();
            }
        }

        [Slider(IsInt = true, Max = 360, Min = 0, Snap = false, Ticks = new float[0])]
        [Title(Title = "Rotate Z")]
        public int RotationZ
        {
            get
            {
                return rotationZ;
            }
            set
            {
                rotationZ = value;
                TryAndProcess();
            }
        }

        float scaleX;
        float scaleY;
        float scaleZ;

        float cameraZoom;

        [Title(Title = "Scale X")]
        public float ScaleX
        {
            get
            {
                return scaleX;
            }
            set
            {
                scaleX = value;
                TryAndProcess();
            }
        }

        [Title(Title = "Scale Y")]
        public float ScaleY
        {
            get
            {
                return scaleY;
            }
            set
            {
                scaleY = value;
                TryAndProcess();
            }
        }

        [Title(Title = "Scale Z")]
        public float ScaleZ
        {
            get
            {
                return scaleZ;
            }
            set
            {
                scaleZ = value;
                TryAndProcess();
            }
        }

        [Title(Title = "Camera Z")]
        public float CameraZoom
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

        [Slider(IsInt = false, Max = 64, Min = 1f, Snap = false, Ticks = new float[0])]
        [Title(Title = "Mesh Texture Tile X")]
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

        [Slider(IsInt = false, Max = 64, Min = 1f, Snap = false, Ticks = new float[0])]
        [Title(Title = "Mesh Texture Tile Y")]
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

            scaleZ = scaleY = scaleX = 1;
            rotationX = RotationY = rotationZ = 0;
            xOffset = yOffset = zOffset = 0;
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
                Process();
                return;
            }

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(100, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                RunInContext(() =>
                {
                    Process();
                });
            });
        }

        void Process()
        {
            if (string.IsNullOrEmpty(path))
            {
                if (mesh != null)
                {
                    mesh.Release();
                }

                mesh = null;
            }

            if (mesh == null)
            {
                if (Async)
                {
                    Task.Run(() =>
                    {
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            RSMI.Importer imp = new Importer();
                            var meshes = imp.Parse(path);

                            if (meshes != null && meshes.Count > 0)
                            {
                                //must be created on the main thread
                                //as it creates the opengl related buffers
                                RunInContext(() =>
                                {
                                    mesh = new MeshRenderer(meshes[0]);
                                });
                            }
                        }
                        else if (!string.IsNullOrEmpty(relativePath) && ParentGraph != null && !string.IsNullOrEmpty(ParentGraph.CWD) && File.Exists(System.IO.Path.Combine(ParentGraph.CWD, relativePath)))
                        {
                            var p = System.IO.Path.Combine(ParentGraph.CWD, relativePath);

                            RSMI.Importer imp = new Importer();
                            var meshes = imp.Parse(p);

                            if (meshes != null && meshes.Count > 0)
                            {
                                //must be created on the main thread
                                //as it creates the opengl related buffers
                                RunInContext(() =>
                                {
                                    mesh = new MeshRenderer(meshes[0]);
                                });
                            }
                        }
                    }).ContinueWith(t =>
                    {
                        RunInContext(() =>
                        {
                            FinalProcess();
                        });
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        RSMI.Importer imp = new Importer();
                        var meshes = imp.Parse(path);

                        if (meshes != null && meshes.Count > 0)
                        {
                            mesh = new MeshRenderer(meshes[0]);
                        }
                    }
                    else if (!string.IsNullOrEmpty(relativePath) && ParentGraph != null && !string.IsNullOrEmpty(ParentGraph.CWD) && File.Exists(System.IO.Path.Combine(ParentGraph.CWD, relativePath)))
                    {
                        var p = System.IO.Path.Combine(ParentGraph.CWD, relativePath);

                        RSMI.Importer imp = new Importer();
                        var meshes = imp.Parse(p);

                        if (meshes != null && meshes.Count > 0)
                        {
                            mesh = new MeshRenderer(meshes[0]);
                        }
                    }
                    FinalProcess();
                }
            }
            else
            {
                FinalProcess();
            }
        }

        void FinalProcess()
        {
            if (mesh == null) return;

            CreateBufferIfNeeded();

            mesh.Mat = mat;

            float rx = (float)rotationX * ((float)Math.PI / 180.0f);
            float ry = (float)rotationY * ((float)Math.PI / 180.0f);
            float rz = (float)rotationZ * ((float)Math.PI / 180.0f);

            Quaternion rot = Quaternion.FromEulerAngles(rx, ry, rz);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(rot);
            Matrix4 translation = Matrix4.CreateTranslation(xOffset, yOffset, zOffset);
            Matrix4 scale = Matrix4.CreateScale(scaleX, scaleY, scaleZ);

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

            public int rotationX;
            public int rotationY;
            public int rotationZ;

            public float cameraZoom;

            public float meshTileX;
            public float meshTileY;
        }

        public override void FromJson(string data)
        {
            MeshNodeData d = JsonConvert.DeserializeObject<MeshNodeData>(data);
            SetBaseNodeDate(d);

            path = d.path;
            Resource = d.resource;
            relativePath = d.relativePath;

            xOffset = d.translateX;
            yOffset = d.translateY;
            zOffset = d.translateZ;

            scaleX = d.scaleX;
            scaleY = d.scaleY;
            scaleZ = d.scaleZ;

            rotationX = d.rotationX;
            rotationY = d.rotationY;
            rotationZ = d.rotationZ;

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

            d.translateX = xOffset;
            d.translateY = yOffset;
            d.translateZ = zOffset;

            d.scaleX = scaleX;
            d.scaleY = scaleY;
            d.scaleZ = scaleZ;

            d.rotationX = rotationX;
            d.rotationY = rotationY;
            d.rotationZ = rotationZ;

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

            if (string.IsNullOrEmpty(relativePath) || string.IsNullOrEmpty(path))
            {
                return;
            }

            string cpath = System.IO.Path.Combine(CWD, relativePath);
            string opath = System.IO.Path.Combine(ParentGraph.CWD, relativePath);
            if (!Directory.Exists(cpath))
            {
                Directory.CreateDirectory(cpath);
            }


            if (File.Exists(path))
            {
                File.Copy(path, cpath);
            }
            else if (File.Exists(opath) && !opath.ToLower().Equals(cpath.ToLower()))
            {
                File.Copy(opath, cpath);
            }
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
