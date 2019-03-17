using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging.GLProcessing;
using Materia.Textures;
using Materia.Geometry;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;
using RSMI;
using OpenTK;
using System.IO;
using Materia.Material;

namespace Materia.Nodes.Atomic
{
    public class MeshDepthNode : ImageNode
    {
        string relativePath;

        string path;
        [FileSelector(Filter = "FBX & OBJ (*.fbx;*.obj)|*.fbx;*.obj")]
        [Title(Title = "Mesh File")]
        [Section(Section = "Content")]
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

        MeshDepthProcessor processor;
        MeshRenderer mesh;
        NodeOutput Output;

        static Matrix4 Proj = Matrix4.CreatePerspectiveFieldOfView(40 * ((float)Math.PI / 180.0f), 1, 0.03f, 1000.0f);
        static PBRMaterial mat = new PBRDepth();

        public MeshDepthNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Mesh Depth";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            tileX = tileY = 1;

            scaleZ = scaleY = scaleX = 1;
            rotationX = RotationY = rotationZ = 0;
            xOffset = yOffset = zOffset = 0;
            cameraZoom = 3;

            previewProcessor = new BasicImageRenderer();
            processor = new MeshDepthProcessor();

            internalPixelType = p;

            Inputs = new List<NodeInput>();

            Output = new NodeOutput(NodeType.Gray, this);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if(string.IsNullOrEmpty(path))
            {
                if(mesh != null)
                {
                    mesh.Release();
                }

                mesh = null;
            }

            if (mesh == null) {
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
                            App.Current.Dispatcher.Invoke(() =>
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
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                mesh = new MeshRenderer(meshes[0]);
                            });
                        }
                    }
                }).ContinueWith(t =>
                {
                    FinalProcess();
                });
            }
            else
            {
                FinalProcess();
            }
        }

        void FinalProcess()
        {
            App.Current.Dispatcher.Invoke(() =>
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


                mesh.View = view;
                mesh.CameraPosition = pos;
                mesh.Projection = Proj;

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
            });
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            
        }

        public override string GetJson()
        {
            ///throw new NotImplementedException();

            return "";
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
            base.Dispose();

            if (mesh != null)
            {
                mesh.Release();
                mesh = null;
            }

            if(processor != null)
            {
                processor.Release();
            }
        }
    }
}
