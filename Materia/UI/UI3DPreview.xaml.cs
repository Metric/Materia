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
using System.Windows.Media;
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

namespace Materia.UI
{
    public enum PreviewGeometryType
    {
        Cube,
        Sphere,
        Cylinder,
        RoundedCube,
        Plane
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
        TK.GLControl glview;
        public GLTextuer2D irradiance { get; protected set; }
        public GLTextuer2D prefiltered { get; protected set; }

        public GLTextuer2D defaultBlack { get; protected set; }
        public GLTextuer2D defaultGray { get; protected set; }
        public GLTextuer2D defaultWhite { get; protected set; }
        public GLTextuer2D defaultDarkGray { get; protected set; }

        Matrix4 proj;
        Quaternion rotation;
        Vector3 cameraTranslation;
        Vector3 objectTranslation;

        TK.Vector3 lightPosition;
        TK.Vector3 lightColor;

        MeshRenderer cube;
        MeshRenderer sphere;
        MeshRenderer cylinder;
        MeshRenderer plane;
        MeshRenderer cubeRounded;

        Mesh cubeMesh;
        Mesh sphereMesh;
        Mesh cylinderMesh;
        Mesh planeMesh;
        Mesh cubeRoundedMesh;

        Point start;

        float rotX = 0;
        float rotY = 0;

        Material.PBRMaterial mat;

        UINode occlusionNode;
        UINode albedoNode;
        UINode metallicNode;
        UINode roughnessNode;
        UINode normalNode;
        UINode heightNode;

        PreviewGeometryType previewType;
        PreviewCameraPosition previewPosition;
        PreviewCameraMode previewCameraMode;

        public static UI3DPreview Instance { get; protected set; }

        public UI3DPreview()
        {
            InitializeComponent();
            Console.WriteLine("3d view inited");
            Instance = this;
            InitGL();
        }

        private void InitGL()
        {
            if (glview == null)
            {
                previewType = PreviewGeometryType.Cube;
                previewCameraMode = PreviewCameraMode.Perspective;
                previewPosition = PreviewCameraPosition.Front;

                glview = new TK.GLControl(GraphicsMode.Default);
                glview.Load += Glview_Load;
                glview.Paint += Glview_Paint;
                glview.MouseWheel += Glview_MouseWheel;
                glview.MouseMove += Glview_MouseMove;
                glview.MouseDown += Glview_MouseDown;
                FHost.Child = glview;
                rotX = 25;
                rotY = 45;
                rotation = Quaternion.FromEulerAngles(rotX * ((float)Math.PI / 180.0f), rotY * ((float)Math.PI / 180.0f), 0);
                cameraTranslation = new Vector3(0, 0, 3);
                objectTranslation = new Vector3(0, 0, 0);
            }
        }

        private void Glview_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            start = new Point(e.Location.X, e.Location.Y);
        }

        private void Glview_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Point p = new Point(e.Location.X, e.Location.Y);

                Quaternion n = Quaternion.FromEulerAngles(rotX * ((float)Math.PI / 180.0f), rotY * ((float)Math.PI / 180.0f), 0);

                Vector3 up = n * new Vector3(0, 1, 0);

                rotX += ((float)p.Y - (float)start.Y) * 0.25f;
                rotY += ((float)p.X - (float)start.X) * 0.25f * Math.Sign(up.Y);

                rotX = rotX % 360;
                rotY = rotY % 360;

                rotation = n;

                start = p;

                Invalidate();
            }
            else if(e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                Point p = new Point(e.Location.X, e.Location.Y);

                Vector3 right = Vector3.Normalize(rotation * Vector3.UnitX);
                Vector3 up = Vector3.Normalize(rotation * Vector3.UnitY);

                float dx = ((float)p.X - (float)start.X) * 0.0005f;
                float dy = ((float)p.Y - (float)start.Y) * -0.0005f;

                Vector3 t = right * dx + up * dy;

                objectTranslation += t;

                Invalidate();
            }
        }

        private void Glview_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                cameraTranslation += new Vector3(0, 0, -0.25f);
            }
            else if (e.Delta < 0)
            {
                cameraTranslation += new Vector3(0, 0, 0.25f);
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

        private void Node_OnUpdate(Nodes.Node n)
        {
            if(albedoNode != null && albedoNode.Node == n)
            {
                SetAlbedo(n.Buffer);
            }
            else if(metallicNode != null && metallicNode.Node == n)
            {
                SetMetallic(n.Buffer);
            }
            else if(roughnessNode != null && roughnessNode.Node == n)
            {
                SetRoughness(n.Buffer);
            }
            else if(normalNode != null && normalNode.Node == n)
            {
                SetNormal(n.Buffer);
            }
            else if(heightNode != null && heightNode.Node == n)
            {
                SetHeight(n.Buffer);
            }
            else if(occlusionNode != null && occlusionNode.Node == n)
            {
                SetOcclusion(n.Buffer);
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

        void LoadDefaultTextures()
        {
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
                Console.WriteLine(e.StackTrace);
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

            GL.Viewport(0, 0, glview.Width, glview.Height);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);            
            GL.Clear(ClearBufferMask.DepthBufferBit); 

            float wratio = (float)glview.Width / (float)glview.Height;

            if (previewCameraMode == PreviewCameraMode.Orthographic)
            {
                proj = Matrix4.CreateOrthographic(Math.Max(1, cameraTranslation.Z), Math.Max(1, cameraTranslation.Z / wratio), 0.03f, 1000f);
            }
            else
            {
                proj = Matrix4.CreatePerspectiveFieldOfView(40 * (float)(Math.PI / 180.0f), (float)glview.Width / (float)glview.Height, 0.03f, 1000f);
            }

            Matrix4 view = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(-cameraTranslation);
            Vector3 pos = Vector3.Normalize((view * new Vector4(0, 0, 1, 1)).Xyz) * cameraTranslation.Z;

            CheckMaterials();

            if (previewType == PreviewGeometryType.Cube)
            {
                if (cube != null)
                {
                    cube.CameraPosition = pos;
                    cube.IrradianceMap = irradiance;
                    cube.PrefilterMap = prefiltered;
                    cube.Projection = proj;
                    cube.Model = TK.Matrix4.CreateTranslation(objectTranslation);
                    cube.View = view;
                    cube.LightColor = lightColor;
                    cube.LightPosition = lightPosition;
                    cube.Draw();
                }
            }
            else if(previewType == PreviewGeometryType.Sphere)
            {
                if(sphere != null)
                {
                    sphere.CameraPosition = pos; 
                    sphere.IrradianceMap = irradiance;
                    sphere.PrefilterMap = prefiltered;
                    sphere.Projection = proj;
                    sphere.Model = TK.Matrix4.CreateTranslation(objectTranslation);
                    sphere.View = view;
                    sphere.LightColor = lightColor;
                    sphere.LightPosition = lightPosition;
                    sphere.Draw();
                }
            }
            else if(previewType == PreviewGeometryType.Cylinder)
            {
                if(cylinder != null)
                {
                    cylinder.CameraPosition = pos;
                    cylinder.IrradianceMap = irradiance;
                    cylinder.PrefilterMap = prefiltered;
                    cylinder.Projection = proj;
                    cylinder.Model = TK.Matrix4.CreateTranslation(objectTranslation);
                    cylinder.View = view;
                    cylinder.LightColor = lightColor;
                    cylinder.LightPosition = lightPosition;
                    cylinder.Draw();
                }
            }
            else if(previewType == PreviewGeometryType.Plane)
            {
                if (plane != null)
                {
                    plane.CameraPosition = pos;
                    plane.IrradianceMap = irradiance;
                    plane.PrefilterMap = prefiltered;
                    plane.Projection = proj;
                    plane.Model = TK.Matrix4.CreateTranslation(objectTranslation);
                    plane.View = view;
                    plane.LightColor = lightColor;
                    plane.LightPosition = lightPosition;
                    plane.Draw();
                }
            }
            else if(previewType == PreviewGeometryType.RoundedCube)
            {
                if (cubeRounded != null)
                {
                    cubeRounded.CameraPosition = pos;
                    cubeRounded.IrradianceMap = irradiance;
                    cubeRounded.PrefilterMap = prefiltered;
                    cubeRounded.Projection = proj;
                    cubeRounded.Model = TK.Matrix4.CreateTranslation(objectTranslation);
                    cubeRounded.View = view;
                    cubeRounded.LightColor = lightColor;
                    cubeRounded.LightPosition = lightPosition;
                    cubeRounded.Draw();
                }
            }

            glview.SwapBuffers();
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

            lightColor = new TK.Vector3(1, 1, 1);
            lightPosition = new TK.Vector3(0, 4, 4);

            Task.Run(async () =>
            {
                await LoadHdri("Circus");
            });
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (glview != null)
            {
                glview.Invalidate();
            }
        }

        protected async Task LoadHdri(string folder)
        {
            string iradpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hdri", folder, "irradiance.dds");
            string prefpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hdri", folder, "prefiltered.dds");

            try
            {
                DDSImage rad = await DDSReader.DDSReader.ReadImageAsync(iradpath);
                DDSImage pre = await DDSReader.DDSReader.ReadImageAsync(prefpath);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Collection<DDSMipMap> mips = (Collection<DDSMipMap>)rad.Frames;

                    if (mips.Count > 0)
                    {

                        var mip = mips[0];
                        byte[] data = mip.MipmapData[0];
                        if (irradiance != null)
                        {
                            irradiance.Release();
                        }

                        irradiance = new GLTextuer2D(PixelInternalFormat.Rgb8);
                        irradiance.Bind();
                        irradiance.SetData(data, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, (int)mip.Width, (int)mip.Height);
                        irradiance.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                        irradiance.SetWrap((int)TextureWrapMode.ClampToEdge);
                        GLTextuer2D.Unbind();
                    }

                    mips = (Collection<DDSMipMap>)pre.Frames;

                    if (mips.Count > 0)
                    {
                        if (prefiltered != null)
                        {
                            prefiltered.Release();
                        }

                        prefiltered = new GLTextuer2D(PixelInternalFormat.Rgb8);
                        prefiltered.Bind();
                        prefiltered.SetMaxMipLevel(4);

                        for (int i = 0; i < mips.Count; i++)
                        {
                            var mip = mips[i];
                            byte[] data = mip.MipmapData[0];

                            prefiltered.SetData(data, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, (int)mip.Width, (int)mip.Height, i);
                        }

                        prefiltered.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
                        prefiltered.SetWrap((int)TextureWrapMode.ClampToEdge);

                        GLTextuer2D.Unbind();
                    }

                    Invalidate();
                });
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("3d view unloaded");
        }

        public void Release()
        {
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

            if (irradiance != null)
            {
                irradiance.Release();
                irradiance = null;
            }

            if (prefiltered != null)
            {
                prefiltered.Release();
                prefiltered = null;
            }

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

            if(glview != null)
            {
                FHost.Child = null;
                glview.Dispose();
                glview = null;
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
                    rotX = 0; rotY = 180;
                    break;
                case PreviewCameraPosition.Front:
                    rotX = 0; rotY = 0;
                    break;
                case PreviewCameraPosition.Top:
                    rotX = 90; rotY = 0;
                    break;
                case PreviewCameraPosition.Bottom:
                    rotX = 270; rotY = 0;
                    break;
                case PreviewCameraPosition.Right:
                    rotX = 0; rotY = 90;
                    break;
                case PreviewCameraPosition.Left:
                    rotX = 0; rotY = 270;
                    break;
                case PreviewCameraPosition.Perspective:
                    rotX = 25; rotY = 45;
                    break;
                default:
                    rotX = 0; rotY = 0;
                    break;
            }


            rotation = Quaternion.FromEulerAngles(rotX * ((float)Math.PI / 180.0f), rotY * ((float)Math.PI / 180.0f), 0);

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
            objectTranslation = new Vector3(0,0,0);

            switch (previewPosition)
            {
                case PreviewCameraPosition.Back:
                    rotX = 0; rotY = 180;
                    break;
                case PreviewCameraPosition.Front:
                    rotX = 0; rotY = 0;
                    break;
                case PreviewCameraPosition.Top:
                    rotX = 90; rotY = 0;
                    break;
                case PreviewCameraPosition.Bottom:
                    rotX = 270; rotY = 0;
                    break;
                case PreviewCameraPosition.Right:
                    rotX = 0; rotY = 90;
                    break;
                case PreviewCameraPosition.Left:
                    rotX = 0; rotY = 270;
                    break;
                case PreviewCameraPosition.Perspective:
                    rotX = 25; rotY = 45;
                    break;
                default:
                    rotX = 0; rotY = 0;
                    break;
            }

            rotation = Quaternion.FromEulerAngles(rotX * ((float)Math.PI / 180.0f), rotY * ((float)Math.PI / 180.0f), 0);

            Invalidate();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("3d view loaded");
        }
    }
}
