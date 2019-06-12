using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TK = OpenTK;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Materia.Textures;
using DDSReader;
using System.Collections.ObjectModel;
using Materia.Geometry;
using Materia.Imaging;
using RSMI.Containers;
using Materia.MathHelpers;
using Materia.Hdri;
using Materia.Settings;
using Materia.Buffers;
using NLog;

namespace Materia.UI
{
    public enum PreviewGeometryType
    {
        Cube,
        Sphere,
        Cylinder,
        RoundedCube,
        Plane,
        Custom
    }

    public enum PreviewCameraPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back,
        Perspective
    }

    public enum PreviewCameraMode
    {
        Perspective,
        Orthographic
    }

    /// <summary>
    /// Interaction logic for UI3DPreview.xaml
    /// </summary>
    public partial class UI3DPreview : UserControl
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        GLControl glview;

        public GLTextuer2D defaultBlack { get; protected set; }
        public GLTextuer2D defaultGray { get; protected set; }
        public GLTextuer2D defaultWhite { get; protected set; }
        public GLTextuer2D defaultDarkGray { get; protected set; }

        Light light;
        Camera camera;
        Transform previewObject;
        Material.PBRLight lightMat;

        MeshRenderer cube;
        MeshRenderer sphere;
        MeshRenderer cylinder;
        MeshRenderer plane;
        MeshRenderer cubeRounded;
        MeshRenderer custom;

        Mesh cubeMesh;
        Mesh sphereMesh;
        Mesh cylinderMesh;
        Mesh planeMesh;
        Mesh cubeRoundedMesh;
        Mesh customMesh;

        Material.PBRMaterial mat;

        UINode occlusionNode;
        UINode albedoNode;
        UINode metallicNode;
        UINode roughnessNode;
        UINode normalNode;
        UINode heightNode;
        UINode thicknessNode;

        MaterialSettings materialSettings;
        LightingSettings lightSettings;

        PreviewGeometryType previewType;
        PreviewCameraPosition previewPosition;
        PreviewCameraMode previewCameraMode;

        Point mouseStart;

        public static UI3DPreview Instance { get; protected set; }

        public UI3DPreview()
        {
            HdriManager.Scan();
            HdriManager.OnHdriLoaded += HdriManager_OnHdriLoaded;
            InitializeComponent();
            Instance = this;

            materialSettings = new MaterialSettings();
            materialSettings.Load();
            materialSettings.OnMaterialUpdated += MaterialSettings_OnMaterialUpdated;

            lightSettings = new LightingSettings();
            lightSettings.OnLightingUpdated += LightSettings_OnLightingUpdated;

            InitGL();
            Log.Info("3d view inited");
        }

        private void LightSettings_OnLightingUpdated(LightingSettings sender)
        {
            if(sender == lightSettings)
            {
                if (light != null)
                {
                    //update light object
                    light.LocalPosition = new Vector3(lightSettings.Position.X, lightSettings.Position.Y, lightSettings.Position.Z);
                    light.Color = new Vector3(lightSettings.Color.X, lightSettings.Color.Y, lightSettings.Color.Z);
                    light.Power = lightSettings.Power;

                    Invalidate();
                }
            }
        }

        private void MaterialSettings_OnMaterialUpdated(MaterialSettings sender)
        {
            if(sender == materialSettings)
            {
                Invalidate();
            }
        }

        private void Camera_OnCameraChanged(Camera sender)
        {
            if (sender == camera)
            {
                Invalidate();
            }
        }

        private void HdriManager_OnHdriLoaded(GLTextuer2D irradiance, GLTextuer2D prefiltered)
        {
            Invalidate();
        }

        private void InitGL()
        {
            if (glview == null)
            {
                previewType = PreviewGeometryType.Cube;
                previewCameraMode = PreviewCameraMode.Perspective;
                previewPosition = PreviewCameraPosition.Perspective;

                glview = new GLControl(GraphicsMode.Default);
                glview.Load += Glview_Load;
                glview.Paint += Glview_Paint;
                glview.MouseWheel += Glview_MouseWheel;
                glview.MouseMove += Glview_MouseMove;
                glview.MouseDown += Glview_MouseDown;

                FHost.Child = glview;
                previewObject = new Transform();

                camera = new Camera();
                camera.LocalEulerAngles = new Vector3(25, 45, 0);
                camera.LocalPosition = new Vector3(0, 0, 3);
                camera.OnCameraChanged += Camera_OnCameraChanged;

                light = new Light();
                light.LocalPosition = new Vector3(0, 2, 2);
                light.Color = new Vector3(1, 1, 1);
                light.LocalScale = new Vector3(0.01f, 0.01f, 0.01f);
            }
        }

        private void Glview_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouseStart = new Point(e.Location.X, e.Location.Y);
        }

        private void Glview_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Point p = new Point(e.Location.X, e.Location.Y);

                Quaternion n = camera.LocalRotation;

                Vector3 up = n * new Vector3(0, 1, 0);

                Vector3 euler = camera.LocalEulerAngles;

                euler.X += ((float)p.Y - (float)mouseStart.Y) * 0.25f;
                euler.Y += ((float)p.X - (float)mouseStart.X) * 0.25f * Math.Sign(up.Y);

                euler.X %= 360;
                euler.Y %= 360;

                camera.LocalEulerAngles = euler;

                mouseStart = p;

                Invalidate();
            }
            else if(e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                Point p = new Point(e.Location.X, e.Location.Y);

                Vector3 right = camera.Right;
                Vector3 up = camera.Up;

                float dx = ((float)p.X - (float)mouseStart.X) * 0.0005f;
                float dy = ((float)p.Y - (float)mouseStart.Y) * -0.0005f;

                Vector3 t = right * dx + up * dy;

                previewObject.LocalPosition += t;

                Invalidate();
            }
        }

        private void Glview_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                camera.LocalPosition += new Vector3(0, 0, -0.25f);
            }
            else if (e.Delta < 0)
            {
                camera.LocalPosition += new Vector3(0, 0, 0.25f);
            }

            Invalidate();
        }

        public void TryAndRemovePreviewNode(UINode n)
        {
            if(albedoNode == n)
            {
                albedoNode.Node.OnUpdate -= Node_OnUpdate;
                albedoNode = null;
                SetAlbedo(null);
            }
            else if(normalNode == n)
            {
                normalNode.Node.OnUpdate -= Node_OnUpdate;
                normalNode = null;
                SetNormal(null);
            }
            else if(metallicNode == n)
            {
                metallicNode.Node.OnUpdate -= Node_OnUpdate;
                metallicNode = null;
                SetMetallic(null);
            }
            else if(roughnessNode == n)
            {
                roughnessNode.Node.OnUpdate -= Node_OnUpdate;
                roughnessNode = null;
                SetRoughness(null);
            }
            else if(heightNode == n)
            {
                heightNode.Node.OnUpdate -= Node_OnUpdate;
                heightNode = null;
                SetHeight(null);
            }
            else if(occlusionNode == n)
            {
                occlusionNode.Node.OnUpdate -= Node_OnUpdate;
                occlusionNode = null;
                SetOcclusion(null);
            }
            else if(thicknessNode == n)
            {
                thicknessNode.Node.OnUpdate -= Node_OnUpdate;
                thicknessNode = null;
                SetThickness(null);
                
            }
        }

        public void SetAlbedoNode(UINode n)
        {
            if(albedoNode != null)
            {
                albedoNode.Node.OnUpdate -= Node_OnUpdate;
            }

            albedoNode = n;
            albedoNode.Node.OnUpdate += Node_OnUpdate;
            SetAlbedo(albedoNode.Node.GetActiveBuffer());
        }

        public void SetNormalNode(UINode n)
        {
            if (normalNode != null)
            {
                normalNode.Node.OnUpdate -= Node_OnUpdate;
            }

            normalNode = n;
            normalNode.Node.OnUpdate += Node_OnUpdate;
            SetNormal(normalNode.Node.GetActiveBuffer());
        }

        public void SetMetallicNode(UINode n)
        {
            if (metallicNode != null)
            {
                metallicNode.Node.OnUpdate -= Node_OnUpdate;
            }

            metallicNode = n;
            metallicNode.Node.OnUpdate += Node_OnUpdate;
            SetMetallic(metallicNode.Node.GetActiveBuffer());
        }

        public void SetOcclusionNode(UINode n)
        {
            if (occlusionNode != null)
            {
                occlusionNode.Node.OnUpdate -= Node_OnUpdate;
            }

            occlusionNode = n;
            occlusionNode.Node.OnUpdate += Node_OnUpdate;
            SetOcclusion(occlusionNode.Node.GetActiveBuffer());
        }

        public void SetHeightNode(UINode n)
        {
            if (heightNode != null)
            {
                heightNode.Node.OnUpdate -= Node_OnUpdate;
            }

            heightNode = n;
            heightNode.Node.OnUpdate += Node_OnUpdate;
            SetHeight(heightNode.Node.GetActiveBuffer());
        }

        public void SetRoughnessNode(UINode n)
        {
            if (roughnessNode != null)
            {
                roughnessNode.Node.OnUpdate -= Node_OnUpdate;
            }

            roughnessNode = n;
            roughnessNode.Node.OnUpdate += Node_OnUpdate;
            SetRoughness(roughnessNode.Node.GetActiveBuffer());
        }

        public void SetThicknessNode(UINode n)
        {
            if(thicknessNode != null)
            {
                thicknessNode.Node.OnUpdate -= Node_OnUpdate;
            }

            thicknessNode = n;
            thicknessNode.Node.OnUpdate += Node_OnUpdate;
            SetThickness(thicknessNode.Node.GetActiveBuffer());
        }

        private void Node_OnUpdate(Nodes.Node n)
        {
            if(albedoNode != null && albedoNode.Node == n)
            {
                SetAlbedo(n.GetActiveBuffer());
            }
            else if(metallicNode != null && metallicNode.Node == n)
            {
                SetMetallic(n.GetActiveBuffer());
            }
            else if(roughnessNode != null && roughnessNode.Node == n)
            {
                SetRoughness(n.GetActiveBuffer());
            }
            else if(normalNode != null && normalNode.Node == n)
            {
                SetNormal(n.GetActiveBuffer());
            }
            else if(heightNode != null && heightNode.Node == n)
            {
                SetHeight(n.GetActiveBuffer());
            }
            else if(occlusionNode != null && occlusionNode.Node == n)
            {
                SetOcclusion(n.GetActiveBuffer());
            }
            else if(thicknessNode != null && thicknessNode.Node == n)
            {
                SetThickness(n.GetActiveBuffer());
            }
        }

        void SetAlbedo(GLTextuer2D t)
        {
            if (mat == null) return;

            if (t == null || t.Id == 0)
            {
                mat.Albedo = defaultDarkGray;
            }
            else
            {
                mat.Albedo = t;
            }

            Invalidate();
        }

        void SetNormal(GLTextuer2D t)
        {
            if (mat == null) return;

            if (t == null || t.Id == 0)
            {
                mat.Normal = defaultBlack;
            }
            else
            {
                mat.Normal = t;
            }

            Invalidate();
        }

        void SetMetallic(GLTextuer2D t)
        {
            if (mat == null) return;

            if (t == null || t.Id == 0)
            {
                mat.Metallic = defaultBlack;
            }
            else
            {
                mat.Metallic = t;
            }

            Invalidate();
        }

        void SetRoughness(GLTextuer2D t)
        {
            if (mat == null) return;

            if (t == null || t.Id == 0)
            {
                mat.Roughness = defaultBlack;
            }
            else
            {
                mat.Roughness = t;
            }

            Invalidate();
        }

        void SetHeight(GLTextuer2D t)
        {
            if (mat == null) return;

            if (t == null || t.Id == 0)
            {
                mat.Height = defaultWhite;
            }
            else
            {
                mat.Height = t;
            }

            Invalidate();
        }

        void SetOcclusion(GLTextuer2D t)
        {
            if (mat == null) return;

            if (t == null || t.Id == 0)
            {
                mat.Occlusion = defaultWhite;
            }
            else
            {
                mat.Occlusion = t;
            }

            Invalidate();
        }

        void SetThickness(GLTextuer2D t)
        {
            if (mat == null) return;

            if(t == null || t.Id == 0)
            {
                mat.Thickness = defaultBlack;
            }
            else
            {
                mat.Thickness = t;
            }

            Invalidate();
        }

        void LoadDefaultTextures()
        {
            lightMat = new Material.PBRLight();

            if (defaultBlack == null)
            {
                FloatBitmap black = new FloatBitmap(128, 128);
                Nodes.Helpers.Utils.Fill(black, 0, 0, 0f, 0f, 0f, 1);
                defaultBlack = new GLTextuer2D(PixelInternalFormat.Rgba8);
                defaultBlack.Bind();
                defaultBlack.SetData(black.Image, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, 128, 128);
                defaultBlack.GenerateMipMaps();
                defaultBlack.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
                defaultBlack.SetWrap((int)TextureWrapMode.Repeat);
                GLTextuer2D.Unbind();
            }
            if (defaultWhite == null)
            {
                FloatBitmap black = new FloatBitmap(128, 128);
                Nodes.Helpers.Utils.Fill(black, 0, 0, 1f, 1f, 1f, 1);
                defaultWhite = new GLTextuer2D(PixelInternalFormat.Rgba8);
                defaultWhite.Bind();
                defaultWhite.SetData(black.Image, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, 128, 128);
                defaultWhite.GenerateMipMaps();
                defaultWhite.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
                defaultWhite.SetWrap((int)TextureWrapMode.Repeat);
                GLTextuer2D.Unbind();
            }
            if (defaultGray == null)
            {
                FloatBitmap black = new FloatBitmap(128, 128);
                Nodes.Helpers.Utils.Fill(black, 0, 0, 0.5f, 0.5f, 0.5f, 1);
                defaultGray = new GLTextuer2D(PixelInternalFormat.Rgba8);
                defaultGray.Bind();
                defaultGray.SetData(black.Image, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, 128, 128);
                defaultGray.GenerateMipMaps();
                defaultGray.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
                defaultGray.SetWrap((int)TextureWrapMode.Repeat);
                GLTextuer2D.Unbind();
            }

            if (defaultDarkGray == null)
            {
                FloatBitmap black = new FloatBitmap(128, 128);
                Nodes.Helpers.Utils.Fill(black, 0, 0, 0.25f, 0.25f, 0.25f, 1);
                defaultDarkGray = new GLTextuer2D(PixelInternalFormat.Rgba8);
                defaultDarkGray.Bind();
                defaultDarkGray.SetData(black.Image, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, 128, 128);
                defaultDarkGray.GenerateMipMaps();
                defaultDarkGray.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
                defaultDarkGray.SetWrap((int)TextureWrapMode.Repeat);
                GLTextuer2D.Unbind();
            }
        }

        void LoadMeshes()
        {
            LoadDefaultTextures();

            try
            {
                string pcube = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Geometry", "cube.obj");
                RSMI.Importer importer = new RSMI.Importer();
                List<RSMI.Containers.Mesh> meshes = importer.Parse(pcube);

                if(mat == null)
                {
                    mat = new Material.PBRMaterial
                    {
                        Metallic = defaultBlack,
                        Albedo = defaultDarkGray,
                        Occlusion = defaultWhite,
                        Roughness = defaultBlack,
                        Height = defaultWhite
                    };
                }

                if (meshes.Count > 0)
                {
                    cubeMesh = meshes[0];
                    cube = new MeshRenderer(meshes[0]);
                    cube.Mat = mat;

                    if (UIPreviewPane.Instance != null)
                    {
                        UIPreviewPane.Instance.SetMesh(cube);
                    }
                }

                string psphere = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Geometry", "sphere.obj");
                importer = new RSMI.Importer();
                meshes = importer.Parse(psphere);

                if(meshes.Count > 0)
                {
                    sphereMesh = meshes[0];
                    sphere = new MeshRenderer(meshes[0]);
                    sphere.Mat = mat;
                }

                string prounded = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Geometry", "cube-rounded.obj");
                importer = new RSMI.Importer();
                meshes = importer.Parse(prounded);

                if(meshes.Count > 0)
                {
                    cubeRoundedMesh = meshes[0];
                    cubeRounded = new MeshRenderer(meshes[0]);
                    cubeRounded.Mat = mat;
                }

                //NOTE TO SELF: Recreate the cylnder 3D object with better UV
                string pcyl = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Geometry", "cylinder.obj");
                importer = new RSMI.Importer();
                meshes = importer.Parse(pcyl);

                if (meshes.Count > 0)
                {
                    cylinderMesh = meshes[0];
                    cylinder = new MeshRenderer(meshes[0]);
                    cylinder.Mat = mat;
                }

                string pplane = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Geometry", "plane.obj");
                importer = new RSMI.Importer();
                meshes = importer.Parse(pplane);

                if (meshes.Count > 0)
                {
                    planeMesh = meshes[0];
                    plane = new MeshRenderer(meshes[0]);
                    plane.Mat = mat;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Invalidate()
        {
            if (glview != null)
            {
                glview.Invalidate();
            }
        }

        void CheckMaterials()
        {
            if (mat == null) return;

            if(mat.Albedo == null || mat.Albedo.Id == 0)
            {
                mat.Albedo = defaultDarkGray;
            }
            if(mat.Normal == null || mat.Normal.Id == 0)
            {
                mat.Normal = defaultBlack;
            }
            if(mat.Height == null || mat.Height.Id == 0)
            {
                mat.Height = defaultWhite;
            }
            if(mat.Occlusion == null || mat.Occlusion.Id == 0)
            {
                mat.Occlusion = defaultWhite;
            }
            if(mat.Roughness == null || mat.Roughness.Id == 0)
            {
                mat.Roughness = defaultBlack;
            }
            if(mat.Metallic == null || mat.Metallic.Id == 0)
            {
                mat.Metallic = defaultBlack;
            }
        }

        private void Glview_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (glview == null) return;

            ViewContext.VerifyContext(glview);
            ViewContext.Context.MakeCurrent(glview.WindowInfo);

            camera.Aspect = (float)glview.Width / (float)glview.Height;

            Matrix4 proj;

            if (previewCameraMode == PreviewCameraMode.Orthographic)
            {
                proj = camera.Orthographic;
            }
            else
            {
                proj = camera.Perspective;
            }

            //UpdateSSS
            mat.SSSAmbient = materialSettings.SSSAmbient;
            mat.SSSDistortion = materialSettings.SSSDistortion;
            mat.SSSPower = materialSettings.SSSPower;

            CheckMaterials();
            
            GL.Viewport(0, 0, glview.Width, glview.Height);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            DrawGeometry(ref proj, camera, mat);

            glview.SwapBuffers();
        }

        private void DrawGeometry(ref Matrix4 proj, Camera camera, Material.PBRMaterial m)
        {
            //draw our only light source
            if(sphere != null)
            {
                sphere.Mat = lightMat;
                sphere.CameraPosition = camera.EyePosition;
                sphere.IrradianceMap = HdriManager.Irradiance;
                sphere.PrefilterMap = HdriManager.Prefiltered;
                sphere.Projection = proj;
                sphere.Model = light.WorldMatrix;
                sphere.View = camera.View;
                sphere.LightColor = light.Color;
                sphere.LightPosition = light.Position;
                sphere.LightPower = light.Power;
                sphere.Near = camera.Near;
                sphere.Far = camera.Far;

                sphere.DrawAsLight();
            }

            if (previewType == PreviewGeometryType.Cube)
            {
                if (cube != null)
                {
                    cube.Mat = m;
                    cube.CameraPosition = camera.EyePosition;
                    cube.IrradianceMap = HdriManager.Irradiance;
                    cube.PrefilterMap = HdriManager.Prefiltered;
                    cube.Projection = proj;
                    cube.Model = previewObject.WorldMatrix;
                    cube.View = camera.View;
                    cube.LightColor = light.Color;
                    cube.LightPosition = light.Position;
                    cube.LightPower = light.Power;
                    cube.Near = camera.Near;
                    cube.Far = camera.Far;
                    cube.IOR = materialSettings.IndexOfRefraction;
                    cube.HeightScale = materialSettings.HeightScale;
                    cube.ClipHeight = materialSettings.Clip;
                    cube.ClipHeightBias = materialSettings.HeightClipBias;

                    if (m is Material.PBRDepth)
                    {
                        cube.DrawForDepth();
                    }
                    else
                    {
                        cube.Draw();
                    }
                }
            }
            else if (previewType == PreviewGeometryType.Sphere)
            {
                if (sphere != null)
                {
                    sphere.Mat = m;
                    sphere.CameraPosition = camera.EyePosition;
                    sphere.IrradianceMap = HdriManager.Irradiance;
                    sphere.PrefilterMap = HdriManager.Prefiltered;
                    sphere.Projection = proj;
                    sphere.Model = previewObject.WorldMatrix;
                    sphere.View = camera.View;
                    sphere.LightColor = light.Color;
                    sphere.LightPosition = light.Position;
                    sphere.LightPower = light.Power;
                    sphere.Near = camera.Near;
                    sphere.Far = camera.Far;
                    sphere.IOR = materialSettings.IndexOfRefraction;
                    sphere.HeightScale = materialSettings.HeightScale;
                    sphere.ClipHeight = materialSettings.Clip;
                    sphere.ClipHeightBias = materialSettings.HeightClipBias;

                    if (m is Material.PBRDepth)
                    {
                        sphere.DrawForDepth();
                    }
                    else
                    {
                        sphere.Draw();
                    }
                }
            }
            else if (previewType == PreviewGeometryType.Cylinder)
            {
                if (cylinder != null)
                {
                    cylinder.Mat = m;
                    cylinder.CameraPosition = camera.EyePosition;
                    cylinder.IrradianceMap = HdriManager.Irradiance;
                    cylinder.PrefilterMap = HdriManager.Prefiltered;
                    cylinder.Projection = proj;
                    cylinder.Model = previewObject.WorldMatrix;
                    cylinder.View = camera.View;
                    cylinder.LightColor = light.Color;
                    cylinder.LightPosition = light.Position;
                    cylinder.LightPower = light.Power;
                    cylinder.Near = camera.Near;
                    cylinder.Far = camera.Far;
                    cylinder.IOR = materialSettings.IndexOfRefraction;
                    cylinder.HeightScale = materialSettings.HeightScale;
                    cylinder.ClipHeight = materialSettings.Clip;
                    cylinder.ClipHeightBias = materialSettings.HeightClipBias;

                    if (m is Material.PBRDepth)
                    {
                        cylinder.DrawForDepth();
                    }
                    else
                    {
                        cylinder.Draw();
                    }
                }
            }
            else if (previewType == PreviewGeometryType.Plane)
            {
                if (plane != null)
                {
                    plane.Mat = m;
                    plane.CameraPosition = camera.EyePosition;
                    plane.IrradianceMap = HdriManager.Irradiance;
                    plane.PrefilterMap = HdriManager.Prefiltered;
                    plane.Projection = proj;
                    plane.Model = previewObject.WorldMatrix;
                    plane.View = camera.View;
                    plane.LightColor = light.Color;
                    plane.LightPosition = light.Position;
                    plane.LightPower = light.Power;
                    plane.Near = camera.Near;
                    plane.Far = camera.Far;
                    plane.IOR = materialSettings.IndexOfRefraction;
                    plane.HeightScale = materialSettings.HeightScale;
                    plane.ClipHeight = materialSettings.Clip;
                    plane.ClipHeightBias = materialSettings.HeightClipBias;

                    if (m is Material.PBRDepth)
                    {
                        plane.DrawForDepth();
                    }
                    else
                    {
                        plane.Draw();
                    }
                }
            }
            else if (previewType == PreviewGeometryType.RoundedCube)
            {
                if (cubeRounded != null)
                {
                    cubeRounded.Mat = m;
                    cubeRounded.CameraPosition = camera.EyePosition;
                    cubeRounded.IrradianceMap = HdriManager.Irradiance;
                    cubeRounded.PrefilterMap = HdriManager.Prefiltered;
                    cubeRounded.Projection = proj;
                    cubeRounded.Model = previewObject.WorldMatrix;
                    cubeRounded.View = camera.View;
                    cubeRounded.LightColor = light.Color;
                    cubeRounded.LightPosition = light.Position;
                    cubeRounded.LightPower = light.Power;
                    cubeRounded.Near = camera.Near;
                    cubeRounded.Far = camera.Far;
                    cubeRounded.IOR = materialSettings.IndexOfRefraction;
                    cubeRounded.HeightScale = materialSettings.HeightScale;
                    cubeRounded.ClipHeight = materialSettings.Clip;
                    cubeRounded.ClipHeightBias = materialSettings.HeightClipBias;

                    if (m is Material.PBRDepth)
                    {
                        cubeRounded.DrawForDepth();
                    }
                    else
                    {
                        cubeRounded.Draw();
                    }
                }
            }
            else if(previewType == PreviewGeometryType.Custom)
            {
                if(custom != null)
                {
                    custom.Mat = m;
                    custom.CameraPosition = camera.EyePosition;
                    custom.IrradianceMap = HdriManager.Irradiance;
                    custom.PrefilterMap = HdriManager.Prefiltered;
                    custom.Projection = proj;
                    custom.Model = previewObject.WorldMatrix;
                    custom.View = camera.View;
                    custom.LightColor = light.Color;
                    custom.LightPosition = light.Position;
                    custom.LightPower = light.Power;
                    custom.Near = camera.Near;
                    custom.Far = camera.Far;
                    custom.IOR = materialSettings.IndexOfRefraction;
                    custom.HeightScale = materialSettings.HeightScale;
                    custom.ClipHeight = materialSettings.Clip;
                    custom.ClipHeightBias = materialSettings.HeightClipBias;

                    if (m is Material.PBRDepth)
                    {
                        custom.DrawForDepth();
                    }
                    else
                    {
                        custom.Draw();
                    }
                }
            }
        }

        private void Glview_Load(object sender, EventArgs e)
        {
            ViewContext.VerifyContext(glview);
            ViewContext.Context.MakeCurrent(glview.WindowInfo);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            LoadMeshes();

            Task.Run(async () =>
            {
                //initial hdri load
                await HdriManager.Load();
            });
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (glview != null)
            {
                glview.Invalidate();
            }
        }

        public void Release()
        {
            if(materialSettings != null)
            {
                materialSettings.Save();
            }

            if(lightMat != null)
            {
                lightMat.Release();
                lightMat = null;
            }

            if (defaultBlack != null)
            {
                defaultBlack.Release();
                defaultBlack = null;
            }

            if (defaultWhite != null)
            {
                defaultWhite.Release();
                defaultWhite = null;
            }

            if (defaultGray != null)
            {
                defaultGray.Release();
                defaultGray = null;
            }

            if (defaultDarkGray != null)
            {
                defaultDarkGray.Release();
                defaultDarkGray = null;
            }

            HdriManager.Release();

            if (mat != null)
            {
                mat.Release();
                mat = null;
            }

            if (sphere != null)
            {
                sphere.Release();
                sphere = null;
            }

            if (cube != null)
            {
                cube.Release();
                cube = null;
            }

            if(cylinder != null)
            {
                cylinder.Release();
                cylinder = null;
            }

            if(cubeRounded != null)
            {
                cubeRounded.Release();
                cubeRounded = null;
            }

            if(plane != null)
            {
                plane.Release();
                plane = null;
            }

            if(custom != null)
            {
                custom.Release();
                custom = null;
            }

            if(glview != null)
            {
                FHost.Child = null;
                glview.Dispose();
                glview = null;
            }
        }

        private void ShowCustomMeshDialog()
        {
            System.Windows.Forms.OpenFileDialog opf = new System.Windows.Forms.OpenFileDialog();
            opf.CheckFileExists = true;
            opf.CheckPathExists = true;
            opf.Filter = "FBX & OBJ (*.fbx;*.obj)|*.fbx;*.obj";
            opf.Multiselect = false;
            if (opf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = opf.FileName;

                RSMI.Importer importer = new RSMI.Importer();
                List<Mesh> meshes = importer.Parse(path);

                if(meshes != null && meshes.Count > 0)
                {
                    customMesh = meshes[0];

                    if(custom != null)
                    {
                        custom.Release();
                        custom = null;
                    }

                    custom = new MeshRenderer(customMesh);
                   
                }
            }
        }

        private void PreviewType_Click(object sender, RoutedEventArgs e)
        {
            MenuItem s = sender as MenuItem;

            PreviewGeometryType gtype;

            Enum.TryParse<PreviewGeometryType>(s.Header.ToString().Replace("_", "").Replace(" ", ""), out gtype);

            var pane = UIPreviewPane.Instance;

            if (pane != null)
            {
                switch (gtype)
                {
                    case PreviewGeometryType.Cube:
                        pane.SetMesh(cube);
                        break;
                    case PreviewGeometryType.Cylinder:
                        pane.SetMesh(cylinder);
                        break;
                    case PreviewGeometryType.Plane:
                        pane.SetMesh(plane);
                        break;
                    case PreviewGeometryType.RoundedCube:
                        pane.SetMesh(cubeRounded);
                        break;
                    case PreviewGeometryType.Sphere:
                        pane.SetMesh(sphere);
                        break;
                    case PreviewGeometryType.Custom:
                        ShowCustomMeshDialog();
                        pane.SetMesh(custom);
                        break;
                }
            }

            previewType = gtype;

            Invalidate();
        }

        private void CameraPosition_Click(object sender, RoutedEventArgs e)
        {
            MenuItem s = sender as MenuItem;

            PreviewCameraPosition ctype;

            Enum.TryParse<PreviewCameraPosition>(s.Header.ToString().Replace("_", ""), out ctype);

            previewPosition = ctype;

            switch (previewPosition)
            {
                case PreviewCameraPosition.Back:
                    camera.LocalEulerAngles = new Vector3(0, 180, 0);
                    break;
                case PreviewCameraPosition.Front:
                    camera.LocalEulerAngles = new Vector3(0, 0, 0);
                    break;
                case PreviewCameraPosition.Top:
                    camera.LocalEulerAngles = new Vector3(90, 0, 0);
                    break;
                case PreviewCameraPosition.Bottom:
                    camera.LocalEulerAngles = new Vector3(270, 0, 0);
                    break;
                case PreviewCameraPosition.Right:
                    camera.LocalEulerAngles = new Vector3(0, 90, 0);
                    break;
                case PreviewCameraPosition.Left:
                    camera.LocalEulerAngles = new Vector3(0, 270, 0);
                    break;
                case PreviewCameraPosition.Perspective:
                    camera.LocalEulerAngles = new Vector3(25, 45, 0);
                    break;
                default:
                    camera.LocalEulerAngles = new Vector3(0, 0, 0);
                    break;
            }

            Invalidate();
        }

        private void CameraMode_Click(object sender, RoutedEventArgs e)
        {
            MenuItem s = sender as MenuItem;
            PreviewCameraMode ctype;
            Enum.TryParse<PreviewCameraMode>(s.Header.ToString().Replace("_", ""), out ctype);

            previewCameraMode = ctype;

            Invalidate();
        }

        private void ResetScene_Click(object sender, RoutedEventArgs e)
        {
            previewObject.LocalPosition = new Vector3(0,0,0);

            switch (previewPosition)
            {
                case PreviewCameraPosition.Back:
                    camera.LocalEulerAngles = new Vector3(0, 180, 0);
                    break;
                case PreviewCameraPosition.Front:
                    camera.LocalEulerAngles = new Vector3(0, 0, 0);
                    break;
                case PreviewCameraPosition.Top:
                    camera.LocalEulerAngles = new Vector3(90, 0, 0);
                    break;
                case PreviewCameraPosition.Bottom:
                    camera.LocalEulerAngles = new Vector3(270, 0, 0);
                    break;
                case PreviewCameraPosition.Right:
                    camera.LocalEulerAngles = new Vector3(0, 90, 0);
                    break;
                case PreviewCameraPosition.Left:
                    camera.LocalEulerAngles = new Vector3(0, 270, 0);
                    break;
                case PreviewCameraPosition.Perspective:
                    camera.LocalEulerAngles = new Vector3(25, 45, 0);
                    break;
                default:
                    camera.LocalEulerAngles = new Vector3(0, 0, 0);
                    break;
            }

            Invalidate();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void CameraSettings_Click(object sender, RoutedEventArgs e)
        {
            if(UINodeParameters.Instance != null)
            {
                UINodeParameters.Instance.SetActive(camera);
            }
        }

        //this is used for material menu item click
        //and light settings click
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;

            if(mnu.Header.ToString().ToLower().Contains("material"))
            {
                if(UINodeParameters.Instance != null)
                {
                    UINodeParameters.Instance.SetActive(materialSettings);
                }
            }
            else if(mnu.Header.ToString().ToLower().Contains("light setting"))
            {
                if(UINodeParameters.Instance != null)
                {
                    UINodeParameters.Instance.SetActive(lightSettings);
                }
            }
            else if(mnu.Header.ToString().ToLower().Contains("to origin"))
            {
                lightSettings.SetToOrigin();
            }
            else if(mnu.Header.ToString().ToLower().Contains("default position"))
            {
                lightSettings.DefaultPosition();
            }
            else if(mnu.Header.ToString().ToLower().Contains("reset light"))
            {
                lightSettings.Reset();
            }
        }
    }
}
