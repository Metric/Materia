using Materia.Rendering.Buffers;
using Materia.Rendering.Geometry;
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

        public GLTexture2D Image
        {
            get
            {
                if (stack == null || stack.Output == null || stack.Output.Length < 4) return null;
                return stack.Output[3];
            }
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

        public void Render()
        {
            if (scene == null) return;
            if (!scene.IsModified) return;

            InitializeFrameBuffer();
            
            basePass.Update((int)scene.ViewSize.X, (int)scene.ViewSize.Y);
            bloomPass.Intensity = scene.SceneLightingSettings.BloomIntensity;

            scene.Invalidate();

            var meshes = scene.ActiveMeshes;
            var skybox = scene.ActiveSkybox;
            stack.Process((state) =>
            {
                IGL.Primary.Enable((int)EnableCap.CullFace);
                IGL.Primary.CullFace((int)CullFaceMode.Back);
                IGL.Primary.Enable((int)EnableCap.DepthTest);

                IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);

                MeshRenderer.SharedVao?.Bind();

                if (meshes == null) return;
                for(int i = 0; i < meshes.Count; ++i)
                {
                    meshes[i]?.Draw();
                }

                IGL.Primary.Disable((int)EnableCap.CullFace);

                skybox?.Draw();

                MeshRenderer.SharedVao?.Unbind();
            });
            scene.IsModified = false;
        }
    }
}
