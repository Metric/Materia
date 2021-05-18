using Materia.Rendering.Buffers;
using Materia.Rendering.Geometry;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Passes;
using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Renderer
{
    public class SceneStackRenderer : ISceneRenderer
    {
        protected RenderStack stack;
        protected BasePass basePass;
        protected BloomPass bloomPass;
        protected GLFrameBuffer frameBuffer;
        protected GLRenderBuffer renderBuffer;
        protected Scene scene;

        protected PreviewRenderMode polyMode = PreviewRenderMode.Solid;
        public PreviewRenderMode PolyMode
        {
            get => polyMode;
            set
            {
                if (polyMode != value)
                {
                    polyMode = value;
                    if (scene != null) scene.IsModified = true;
                }
            }
        }

        protected GLTexture2D uvTexture;
        protected UVRenderer uvRenderer;
        protected UVProcessor uvProcessor;

        public GLTexture2D Image
        {
            get;
            protected set;
        }

        public SceneStackRenderer(Scene s)
        {
            scene = s;
            InitializeFrameBuffer();
            stack = new RenderStack();
            basePass = new BasePass(frameBuffer, (int)scene.ViewSize.X, (int)scene.ViewSize.Y);
            bloomPass = new BloomPass(frameBuffer);
            stack.Add(basePass);
            stack.Add(bloomPass);
        }

        protected void InitializeFrameBuffer()
        {
            renderBuffer?.Dispose();
            renderBuffer = new GLRenderBuffer();
            renderBuffer.Bind();
            renderBuffer.SetBufferStorageAsDepth((int)scene.ViewSize.X, (int)scene.ViewSize.Y);
            renderBuffer.Unbind();

            if (frameBuffer == null)
            {
                frameBuffer = new GLFrameBuffer();
            }

            frameBuffer.Bind();
            frameBuffer.AttachDepth(renderBuffer);
            frameBuffer.Unbind();
        }

        public void Dispose()
        {
            uvRenderer?.Dispose();
            uvRenderer = null;

            uvProcessor?.Dispose();
            uvProcessor = null;

            uvTexture?.Dispose();
            uvTexture = null;

            stack?.Dispose();
            stack = null;

            basePass?.Dispose();
            basePass = null;

            bloomPass?.Dispose();
            bloomPass = null;

            renderBuffer?.Dispose();
            renderBuffer = null;

            frameBuffer?.Dispose();
            frameBuffer = null;
        }

        public void UV()
        {
            //esnure image is set to uvTexture if available
            if (scene == null)
            {
                Image = uvTexture;
                return;
            }
            if (!scene.IsModified || scene.ViewSize.X <= 0 || scene.ViewSize.Y <= 0)
            {
                Image = uvTexture;
                return;
            }

            if (uvTexture == null)
            {
                uvTexture = new GLTexture2D(PixelInternalFormat.Rgba8);
                uvTexture.Bind();
                uvTexture.SetData(IntPtr.Zero, PixelFormat.Bgra, 512, 512);
                uvTexture.ClampToEdge();
                uvTexture.Linear();
                GLTexture2D.Unbind();
            }

            if (uvProcessor == null)
            {
                uvProcessor = new UVProcessor();
            }
            if (uvRenderer == null)
            {
                uvRenderer = new UVRenderer();
            }

            uvProcessor.PrepareView(uvTexture);

            MeshRenderer.SharedVao?.Bind();

            var meshes = scene.ActiveMeshes;
            for (int i = 0; i < meshes.Count; ++i)
            {
                var m = meshes[i];
                uvRenderer.Set(m.Renderer);
                uvProcessor.Process(uvRenderer);
            }
            MeshRenderer.SharedVao?.Unbind();

            uvProcessor.Complete();
            Image = uvTexture;
        }

        public void Render()
        {
            //always ensure image is set to stack output
            if (scene == null)
            {
                Image = stack != null && stack.Output != null && stack.Output.Length >= 4 ? stack.Output[3] : null;
                return;
            }
            if (!scene.IsModified || scene.ViewSize.X <= 0 || scene.ViewSize.Y <= 0)
            {
                Image = stack != null && stack.Output != null && stack.Output.Length >= 4 ? stack.Output[3] : null;
                return;
            }

            InitializeFrameBuffer();
            
            basePass.Update((int)scene.ViewSize.X, (int)scene.ViewSize.Y);
            bloomPass.Intensity = scene.SceneLightingSettings.BloomIntensity;

            scene.Invalidate();

            var meshes = scene.ActiveMeshes;
            var skybox = scene.ActiveSkybox;
            var light = scene.ActiveLight;
            stack.Process((state) =>
            {
                IGL.Primary.Enable((int)EnableCap.CullFace);
                IGL.Primary.CullFace((int)CullFaceMode.Back);
                IGL.Primary.Enable((int)EnableCap.DepthTest);
                IGL.Primary.DepthFunc((int)DepthFunction.Lequal);

                switch (polyMode)
                {
                    case PreviewRenderMode.Solid:
                        IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);
                        break;
                    case PreviewRenderMode.Wireframe:
                        IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Line);
                        break;
                }

                MeshRenderer.SharedVao?.Bind();

                if (meshes != null)
                {
                    for (int i = 0; i < meshes.Count; ++i)
                    {
                        meshes[i]?.Draw();
                    }
                }

                light?.Draw();

                IGL.Primary.Disable((int)EnableCap.CullFace);

                //ensure we switch back to default
                if (polyMode != PreviewRenderMode.Solid)
                {
                    IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);
                }

                skybox?.Draw();

                MeshRenderer.SharedVao?.Unbind();

                IGL.Primary.Disable((int)EnableCap.DepthTest);
            });
            scene.IsModified = false;
            Image = stack != null && stack.Output != null && stack.Output.Length >= 4 ? stack.Output[3] : null;
        }
    }
}
