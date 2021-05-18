using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Rendering;
using Materia.Rendering.Geometry;
using Materia.Rendering.Imaging;
using Materia.Rendering.Importer;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Material;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Passes;
using Materia.Rendering.Textures;
using MateriaCore.Utils;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MateriaCore.Components.GL.Renderer
{ 
    public class Scene : IDisposable
    {
        #region Default Accessors
        public GLTexture2D DefaultBlack { get; protected set; }
        public GLTexture2D DefaultGray { get; protected set; }
        public GLTexture2D DefaultWhite { get; protected set; }
        public GLTexture2D DefaultDarkGray { get; protected set; }

        public PBRMaterial DefaultMaterial { get; protected set; }
        public PBRTess DefaultTessMaterial { get; protected set; }
        public PBRLight DefaultLightMaterial { get; protected set; }

        public PBRSkybox DefaultSkyboxMaterial { get; protected set; }

        public Settings.Material SceneMaterialSettings { get; protected set; }
        public Settings.Lighting SceneLightingSettings { get; protected set; }

        public PBRMaterial ActiveMaterial { get; protected set; }
        public GLTextureCube ActiveIrradiance { get; set; }
        public GLTextureCube ActivePrefilter { get; set; }

        protected Dictionary<string, List<Mesh>> meshCache = new Dictionary<string, List<Mesh>>();
        protected Dictionary<string, MeshSceneObject> meshSceneObjectCache = new Dictionary<string, MeshSceneObject>();

        protected Vector2 viewSize = new Vector2(512, 512);
        public Vector2 ViewSize 
        { 
            get => viewSize;
            set
            {
                if (value != viewSize)
                {
                    viewSize = value;
                    IsModified = true;
                    Debug.WriteLine(viewSize.ToString());
                }
            }
        }
        public PreviewCameraMode CameraMode { get; set; } = PreviewCameraMode.Perspective;

        public Matrix4 ActiveProjection { get; protected set; }

        public List<MeshSceneObject> ActiveMeshes { get => meshSceneObjects.FindAll(m => m.Visible && m.RenderMode != MeshRenderType.Skybox); }
        public MeshSceneObject ActiveSkybox { get => meshSceneObjects.Find(m => m.Visible && m.RenderMode == MeshRenderType.Skybox); }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is modified.
        /// Set this to true when the scene has changed in some fashion
        /// So that the renderer can render a new frame
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is modified; otherwise, <c>false</c>.
        /// </value>
        public bool IsModified { get; set; } = false;
        #endregion

        #region Active Node Previews
        protected UINode activeColor;
        protected UINode activeNormal;
        protected UINode activeMetallic;
        protected UINode activeRoughness;
        protected UINode activeOcclusion;
        protected UINode activeHeight;
        protected UINode activeThickness;
        protected UINode activeEmission;
        #endregion

        #region Storage Helpers
        protected Dictionary<string, SceneObject> registeredSceneObjects = new Dictionary<string, SceneObject>();
        protected List<MeshSceneObject> meshSceneObjects = new List<MeshSceneObject>();
        #endregion

        #region Primary Objects
        public Light Light { get; protected set; }
        public Camera Cam { get; protected set; }

        public SceneObject Root { get; set; }
        #endregion

        public Scene()
        {
            CreateDefaults();
            AddEvents();
        }

        protected virtual void CreateProjection()
        {
            if (viewSize.LengthSquared <= float.Epsilon) return;

            float aspect = viewSize.X / viewSize.Y;
            if (aspect <= float.Epsilon) aspect = 1;
            Cam.Aspect = aspect;

            switch (CameraMode)
            {
                case PreviewCameraMode.Perspective:
                    ActiveProjection = Cam.Perspective;
                    break;
                case PreviewCameraMode.Orthographic:
                    ActiveProjection = Cam.Orthographic;
                    break;
            }
        }

        protected virtual void CreateDefaults()
        {
            Cam = new Camera()
            {
                LocalEulerAngles = new Vector3(0, 0, 0),
                LocalPosition = new Vector3(0, 0, 3)
            };

            Light = new Light()
            {
                LocalPosition = new Vector3(0, 2, 0),
                Color = new Vector3(1, 0, 0),
                LocalScale = new Vector3(1f, 1f, 1f)
            };

            SceneLightingSettings = new Settings.Lighting();
            SceneLightingSettings.Update += SceneLightingSettings_Update;
            SceneLightingSettings.Load();

            Light.Power = SceneLightingSettings.Power;
            Light.LocalPosition = SceneLightingSettings.Position.ToVector3();
            Light.Color = SceneLightingSettings.Color.ToVector3();

            SceneMaterialSettings = new Settings.Material();
            SceneMaterialSettings.Update += SceneMaterialSettings_Update;
            SceneMaterialSettings.Load();

            CreateDefaultTextures();

            DefaultMaterial = new PBRMaterial();
            DefaultLightMaterial = new PBRLight();
            DefaultTessMaterial = new PBRTess();

            DefaultSkyboxMaterial = new PBRSkybox
            {
                Albedo = DefaultWhite
            };

            ActiveMaterial = DefaultMaterial;

            //Setup material defaults via
            //Already established handlers
            SetColor(null);
            SetHeight(null);
            SetMetallic(null);
            SetNormal(null);
            SetOcclusion(null);
            SetRoughness(null);
            SetThickness(null);
            SetEmission(null);

            Invalidate();
        }

        protected virtual void CreateDefaultTextures()
        {
            RawBitmap bmp = new RawBitmap(16, 16);

            //black
            bmp.Clear(GLPixel.FromRGBA(0f, 0f, 0f, 1f));
            DefaultBlack = new GLTexture2D(PixelInternalFormat.Rgba8);
            DefaultBlack.Bind();
            DefaultBlack.SetData(bmp.Image, PixelFormat.Bgra, 16, 16);
            DefaultBlack.Linear();
            DefaultBlack.Repeat();
            DefaultBlack.GenerateMipMaps();
            GLTexture2D.Unbind();
            MeshNode.DefaultBlack = DefaultBlack;

            //white 
            bmp.Clear(GLPixel.FromRGBA(1f, 1f, 1f, 1f));
            DefaultWhite = new GLTexture2D(PixelInternalFormat.Rgba8);
            DefaultWhite.Bind();
            DefaultWhite.SetData(bmp.Image, PixelFormat.Bgra, 16, 16);
            DefaultWhite.Linear();
            DefaultWhite.Repeat();
            DefaultWhite.GenerateMipMaps();
            GLTexture2D.Unbind();
            MeshNode.DefaultWhite = DefaultWhite;

            //gray
            bmp.Clear(GLPixel.FromRGBA(0.5f, 0.5f, 0.5f, 1f));
            DefaultGray = new GLTexture2D(PixelInternalFormat.Rgba8);
            DefaultGray.Bind();
            DefaultGray.SetData(bmp.Image, PixelFormat.Bgra, 16, 16);
            DefaultGray.Linear();
            DefaultGray.Repeat();
            DefaultGray.GenerateMipMaps();
            GLTexture2D.Unbind();

            //dark gray
            bmp.Clear(GLPixel.FromRGBA(0.25f, 0.25f, 0.25f, 1f));
            DefaultDarkGray = new GLTexture2D(PixelInternalFormat.Rgba8);
            DefaultDarkGray.Bind();
            DefaultDarkGray.SetData(bmp.Image, PixelFormat.Bgra, 16, 16);
            DefaultDarkGray.Linear();
            DefaultDarkGray.Repeat();
            DefaultDarkGray.GenerateMipMaps();
            MeshNode.DefaultDarkGray = DefaultDarkGray;
            GLTexture2D.Unbind();
        }

        #region General Events
        private void SceneMaterialSettings_Update(Settings.Material obj)
        {
            IsModified = true;
            Invalidate(true);
        }

        private void SceneLightingSettings_Update(Settings.Lighting obj)
        {
            Light.Power = SceneLightingSettings.Power;
            Light.LocalPosition = SceneLightingSettings.Position.ToVector3();
            Light.Color = SceneLightingSettings.Color.ToVector3();

            IsModified = true;
            Invalidate();
        }
        #endregion

        public virtual void Invalidate(bool matSwitchCheck = false)
        {
            DefaultMaterial.SSSAmbient = DefaultTessMaterial.SSSAmbient = SceneMaterialSettings.SSSAmbient;
            DefaultMaterial.SSSDistortion = DefaultTessMaterial.SSSDistortion = SceneMaterialSettings.SSSDistortion;
            DefaultMaterial.SSSPower = DefaultTessMaterial.SSSPower = SceneMaterialSettings.SSSPower;

            DefaultMaterial.IOR = DefaultTessMaterial.IOR = SceneMaterialSettings.IndexOfRefraction;
            DefaultMaterial.HeightScale = DefaultTessMaterial.HeightScale = SceneMaterialSettings.HeightScale;
            DefaultMaterial.ClipHeight = DefaultTessMaterial.ClipHeight = SceneMaterialSettings.Clip;
            DefaultMaterial.ClipHeightBias = DefaultTessMaterial.ClipHeightBias = SceneMaterialSettings.HeightClipBias;

            DefaultTessMaterial.UseDisplacement = true;
            DefaultMaterial.UseDisplacement = false;

            CreateProjection();

            if (matSwitchCheck && SceneMaterialSettings.Displacement)
            {
                ActiveMaterial = DefaultTessMaterial;
                IsModified = true;
            }
            else if (matSwitchCheck && !SceneMaterialSettings.Displacement)
            {
                ActiveMaterial = DefaultMaterial;
                IsModified = true;
            }
        }

        #region Node Event Handling
        protected virtual void AddNodeEvents(UINode n)
        {
            if (n == null) return;
            n.Restored += N_Restored;
            n.PreviewUpdated += N_PreviewUpdated;
        }

        protected virtual void RemoveNodeEvents(UINode n)
        {
            if (n == null) return;
            n.Restored -= N_Restored;
            n.PreviewUpdated -= N_PreviewUpdated;
        }

        private void TryAndUpdatePreview(UINode n)
        {
            if (n == activeColor)
            {
                SetColor(n);
            }
            else if (n == activeNormal)
            {
                SetNormal(n);
            }
            else if (n == activeMetallic)
            {
                SetMetallic(n);
            }
            else if (n == activeRoughness)
            {
                SetRoughness(n);
            }
            else if (n == activeOcclusion)
            {
                SetOcclusion(n);
            }
            else if (n == activeHeight)
            {
                SetHeight(n);
            }
            else if (n == activeThickness)
            {
                SetThickness(n);
            }
            else if (n == activeEmission)
            {
                SetEmission(n);
            }
        }

        private void N_PreviewUpdated(UINode obj)
        {
            TryAndUpdatePreview(obj);
        }

        private void N_Restored(UINode obj)
        {
            TryAndUpdatePreview(obj);
        }

        #endregion

        #region Global Event Handling
        protected virtual void AddEvents()
        {
            GlobalEvents.On(GlobalEvent.Preview3DColor, OnPreviewColor);
            GlobalEvents.On(GlobalEvent.Preview3DNormal, OnPreviewNormal);
            GlobalEvents.On(GlobalEvent.Preview3DMetallic, OnPreviewMetallic);
            GlobalEvents.On(GlobalEvent.Preview3DOcclusion, OnPreviewOcclusion);
            GlobalEvents.On(GlobalEvent.Preview3DRoughness, OnPreviewRoughness);
            GlobalEvents.On(GlobalEvent.Preview3DHeight, OnPreviewHeight);
            GlobalEvents.On(GlobalEvent.Preview3DThickness, OnPreviewThickness);
            GlobalEvents.On(GlobalEvent.Preview3DEmission, OnPreviewEmission);
            GlobalEvents.On(GlobalEvent.HdriUpdate, OnHdriUpdate);
            GlobalEvents.On(GlobalEvent.SkyboxUpdate, OnSkyboxUpdate);
        }

        protected virtual void RemoveEvents()
        {
            GlobalEvents.Off(GlobalEvent.Preview3DColor, OnPreviewColor);
            GlobalEvents.Off(GlobalEvent.Preview3DNormal, OnPreviewNormal);
            GlobalEvents.Off(GlobalEvent.Preview3DMetallic, OnPreviewMetallic);
            GlobalEvents.Off(GlobalEvent.Preview3DOcclusion, OnPreviewOcclusion);
            GlobalEvents.Off(GlobalEvent.Preview3DRoughness, OnPreviewRoughness);
            GlobalEvents.Off(GlobalEvent.Preview3DHeight, OnPreviewHeight);
            GlobalEvents.Off(GlobalEvent.Preview3DThickness, OnPreviewThickness);
            GlobalEvents.Off(GlobalEvent.Preview3DEmission, OnPreviewEmission);
            GlobalEvents.Off(GlobalEvent.HdriUpdate, OnHdriUpdate);
            GlobalEvents.Off(GlobalEvent.SkyboxUpdate, OnSkyboxUpdate);
        }

        protected virtual void OnSkyboxUpdate(object sender, object texture)
        {
            if (texture is IGLTexture)
            {
                if (DefaultSkyboxMaterial != null)
                {
                    DefaultSkyboxMaterial.Albedo = texture as IGLTexture;
                    MeshNode.Environment = texture as GLTextureCube;
                }

                IsModified = true;
            }
        }

        protected virtual void OnHdriUpdate(object irradiance, object prefilter)
        {
            if (irradiance is GLTextureCube && prefilter is GLTextureCube)
            {
                ActiveIrradiance = irradiance as GLTextureCube;
                ActivePrefilter = prefilter as GLTextureCube;

                MeshNode.Irradiance = ActiveIrradiance;
                MeshNode.Prefilter = ActivePrefilter;

                IsModified = true;
            }
        }

        protected virtual void OnPreviewColor(object sender, object n)
        {
            var node = n as UINode;
            RemoveNodeEvents(activeColor);
            SetColor(node);
            activeColor = node;
            AddNodeEvents(activeColor);
        }

        protected virtual void SetColor(UINode node)
        {
            GLTexture2D buffer = node?.GetActiveBuffer();
            DefaultMaterial.Albedo = DefaultTessMaterial.Albedo = buffer ?? DefaultWhite;
            IsModified = true;
        }

        protected virtual void OnPreviewNormal(object sender, object n)
        {
            var node = n as UINode;
            RemoveNodeEvents(activeNormal);
            SetNormal(node);
            activeNormal = node;
            AddNodeEvents(activeNormal);
        }

        protected virtual void SetNormal(UINode node)
        {
            GLTexture2D buffer = node?.GetActiveBuffer();
            DefaultMaterial.Normal = DefaultTessMaterial.Normal = buffer ?? DefaultBlack;
            IsModified = true;
        }

        protected virtual void OnPreviewMetallic(object sender, object n)
        {
            var node = n as UINode;
            RemoveNodeEvents(activeMetallic);
            SetMetallic(node);
            activeMetallic = node;
            AddNodeEvents(activeMetallic);
        }

        protected virtual void SetMetallic(UINode node)
        {
            GLTexture2D buffer = node?.GetActiveBuffer();
            DefaultMaterial.Metallic = DefaultTessMaterial.Metallic = buffer ?? DefaultGray;
            IsModified = true;
        }

        protected virtual void OnPreviewOcclusion(object sender, object n)
        {
            var node = n as UINode;
            RemoveNodeEvents(activeOcclusion);
            SetOcclusion(node);
            activeOcclusion = node;
            AddNodeEvents(activeOcclusion);
        }

        protected virtual void SetOcclusion(UINode node)
        {
            GLTexture2D buffer = node?.GetActiveBuffer();
            DefaultMaterial.Occlusion = DefaultTessMaterial.Occlusion = buffer ?? DefaultWhite;
            IsModified = true;
        }

        protected virtual void OnPreviewRoughness(object sender, object n)
        {
            var node = n as UINode;
            RemoveNodeEvents(activeRoughness);
            SetRoughness(node);
            activeRoughness = node;
            AddNodeEvents(activeRoughness);
        }

        protected virtual void SetRoughness(UINode node)
        {
            GLTexture2D buffer = node?.GetActiveBuffer();
            DefaultMaterial.Roughness = DefaultTessMaterial.Roughness = buffer ?? DefaultDarkGray;
            IsModified = true;
        }

        protected virtual void OnPreviewHeight(object sender, object n)
        {
            var node = n as UINode;
            RemoveNodeEvents(activeHeight);
            SetHeight(node);
            activeHeight = node;
            AddNodeEvents(activeHeight);
        }

        protected virtual void SetHeight(UINode node)
        {
            GLTexture2D buffer = node?.GetActiveBuffer();
            DefaultMaterial.Height = DefaultTessMaterial.Height = buffer ?? DefaultWhite;
            IsModified = true;
        }

        protected virtual void OnPreviewThickness(object sender, object n)
        {
            var node = n as UINode;
            RemoveNodeEvents(activeThickness);
            SetThickness(node);
            activeThickness = node;
            AddNodeEvents(activeThickness);
        }

        protected virtual void SetThickness(UINode node)
        {
            GLTexture2D buffer = node?.GetActiveBuffer();
            DefaultMaterial.Thickness = DefaultTessMaterial.Thickness = buffer ?? DefaultBlack;
            IsModified = true;
        }

        protected virtual void OnPreviewEmission(object sender, object n)
        {
            var node = n as UINode;
            RemoveNodeEvents(activeThickness);
            SetEmission(node);
            activeEmission = node;
            AddNodeEvents(activeThickness);
        }

        protected virtual void SetEmission(UINode node)
        {
            GLTexture2D buffer = node?.GetActiveBuffer();
            DefaultMaterial.Emission = DefaultTessMaterial.Emission = buffer ?? DefaultBlack;
            IsModified = true;
        }
        #endregion


        public SceneObject CreateMeshFromEmbeddedFile(string path, Type t, MeshRenderType mode = MeshRenderType.PBR)
        {
            try
            {
                SceneObject obj = CreateFromCache(path, mode);
                if (obj != null) return obj;
                EmbeddedFileProvider provider = new EmbeddedFileProvider(t.Assembly);
                return CreateMeshFromStream(path, provider.GetFileInfo(path).CreateReadStream(), mode);
            }
            catch (Exception e)
            {
                MLog.Log.Error(e);
            }

            return null;
        }

        public SceneObject CreateMeshFromFile(string path, MeshRenderType mode = MeshRenderType.PBR)
        {
            try
            {
                if (!File.Exists(path)) return null;
                SceneObject obj = CreateFromCache(path, mode);
                if (obj != null) return obj;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return CreateMeshFromStream(path, fs, mode);
                }
            }
            catch (Exception e)
            {
                MLog.Log.Error(e);
            }

            return null;
        }

        protected SceneObject CreateFromCache(string id, MeshRenderType mode)
        {
            List<Mesh> meshes = null;
            meshCache.TryGetValue(id, out meshes);
            if (meshes == null) return null;

            SceneObject group = new SceneObject();
            for (int i = 0; i < meshes.Count; ++i)
            {
                var mid = id + i;
                MeshSceneObject origin = null;
                MeshSceneObject ms = null;
                if (meshSceneObjectCache.TryGetValue(mid, out origin))
                {
                    ms = new MeshSceneObject
                    {
                        RenderMode = mode,
                        Renderer = origin.Renderer,
                        GetActiveFar = () => Cam.Far,
                        GetActiveNear = () => Cam.Near,
                        GetActiveEyePosition = () => Cam.OrbitEyePosition,
                        GetActiveLight = () => Light,
                        GetActiveMaterial = (m) =>
                        {
                            switch (m)
                            {
                                case MeshRenderType.Light:
                                    return DefaultLightMaterial;
                                case MeshRenderType.Skybox:
                                    return DefaultSkyboxMaterial;
                                default:
                                    return ActiveMaterial;
                            }
                        },
                        GetActiveIrradianceMap = () => ActiveIrradiance,
                        GetActivePrefilterMap = () => ActivePrefilter,
                        GetActiveEnvironmentMap = () => DefaultSkyboxMaterial.Albedo as GLTextureCube,
                        GetActiveView = () => Cam.OrbitView,
                        GetActiveProjection = () => ActiveProjection
                    };
                    meshSceneObjects.Add(ms);
                    registeredSceneObjects[ms.Id] = ms;
                    group.Add(ms);
                }
            }

            registeredSceneObjects[group.Id] = group;
            return group;
        }

        protected SceneObject CreateMeshFromStream(string id, Stream stream, MeshRenderType mode)
        {
            Importer importer = new Importer();
            var meshes = importer.Parse(stream);

            meshCache[id] = meshes;

            SceneObject group = new SceneObject();

            for(int i = 0; i < meshes.Count; ++i)
            {
                MeshSceneObject ms = new MeshSceneObject
                {
                    RenderMode = mode,
                    Renderer = new MeshRenderer(meshes[i]),
                    GetActiveFar = () => Cam.Far,
                    GetActiveNear = () => Cam.Near,
                    GetActiveEyePosition = () => Cam.OrbitEyePosition,
                    GetActiveLight = () => Light,
                    GetActiveMaterial = (m) =>
                    {
                        switch(m)
                        {
                            case MeshRenderType.Light:
                                return DefaultLightMaterial;
                            case MeshRenderType.Skybox:
                                return DefaultSkyboxMaterial;
                            default:
                                return ActiveMaterial;
                        }
                    },
                    GetActiveIrradianceMap = () => ActiveIrradiance,
                    GetActivePrefilterMap = () => ActivePrefilter,
                    GetActiveEnvironmentMap = () => DefaultSkyboxMaterial.Albedo as GLTextureCube,
                    GetActiveView = () => Cam.OrbitView,
                    GetActiveProjection = () => ActiveProjection
                };
                meshSceneObjects.Add(ms);
                meshSceneObjectCache[id + i] = ms;
                registeredSceneObjects[ms.Id] = ms;
                group.Add(ms);
            }

            registeredSceneObjects[group.Id] = group;

            return group;
        }

        public void Dispose()
        {
            DefaultBlack?.Dispose();
            DefaultWhite?.Dispose();
            DefaultGray?.Dispose();
            DefaultDarkGray?.Dispose();

            DefaultMaterial?.Dispose();
            DefaultLightMaterial?.Dispose();
            DefaultTessMaterial?.Dispose();
            DefaultSkyboxMaterial?.Dispose();

            RemoveEvents();

            var objects = registeredSceneObjects.Values.ToList();
            for (int i = 0; i < objects.Count; ++i)
            {
                objects[i]?.Dispose();
            }
            registeredSceneObjects.Clear();
            meshSceneObjectCache.Clear();
            meshCache.Clear();
        }
    }
}
