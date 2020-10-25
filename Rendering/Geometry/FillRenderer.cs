using Materia.Rendering.Buffers;
using Materia.Rendering.Geometry;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Textures;
using Materia.Rendering.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Materia.Rendering.Geometry
{
    public class FillRenderer : IGeometry, IDisposeShared
    {
        public Fill FillData { get; protected set; }

        protected static bool isSharedDisposed = false;
        protected static GLVertexArray sharedVao;
        /// <summary>
        /// Gets the shared vao. Make sure the Stroke is set before calling this.
        /// </summary>
        /// <value>
        /// The shared vao.
        /// </value>
        public static GLVertexArray SharedVao
        {
            get
            {
                if (sharedVao == null && !isSharedDisposed)
                {
                    sharedVao = new GLVertexArray();
                }

                return sharedVao;
            }
        }

        protected GLArrayBuffer vbo;
        protected GLElementBuffer ebo;
        protected int indicesCount;

        public GLTexture2D GradientTexture { get; protected set; }

        public FillRenderer(Fill d)
        {
            GeometryCache.RegisterForDispose(this);

            FillData = d;

            vbo = new GLArrayBuffer(Materia.Rendering.Interfaces.BufferUsageHint.StaticDraw);
            ebo = new GLElementBuffer(Materia.Rendering.Interfaces.BufferUsageHint.StaticDraw);
            GradientTexture = new GLTexture2D(PixelInternalFormat.Rgba8);
            UpdateGradientTexture();
        }

        public void Update()
        {
            if (vbo == null || ebo == null || FillData == null) return;

            vbo?.Bind();
            ebo?.Bind();

            try
            {

                if (FillData != null && FillData.Triangles != null 
                    && FillData.Triangles.Count > 0 && vbo.Id != 0 && ebo.Id != 0
                    && FillData.Points != null && FillData.Points.Count >= 3)
                {
                    int[] tris = FillData.Triangles.ToArray();
                    float[] data = FillData.Compact();
                    vbo.SetData(data);
                    ebo.SetData(tris);
                    indicesCount = tris.Length;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            vbo?.Unbind();
            ebo?.Unbind();
        }

        public void UpdateGradientTexture()
        {
            GradientTexture?.Bind();
            GradientTexture?.SetData(FillData.GradientMap.Image, PixelFormat.Bgra, FillData.GradientMap.Width, FillData.GradientMap.Height, 0);
            GradientTexture?.Linear();
            GradientTexture?.Repeat();
            GLTexture2D.Unbind();
        }


        public void DisposeShared()
        {
            isSharedDisposed = true;
            sharedVao?.Dispose();
            sharedVao = null;
        }

        public void Dispose()
        {
            vbo?.Dispose();
            vbo = null;

            ebo?.Dispose();
            ebo = null;

            GradientTexture?.Dispose();
            GradientTexture = null;
        }

        public void Draw()
        {
            if (vbo == null || ebo == null || FillData == null 
                || indicesCount == 0 || FillData.Points == null || FillData.Points.Count < 3) return;

            vbo?.Bind();
            ebo?.Bind();

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
            GradientTexture?.Bind();

            IGL.Primary.VertexAttribPointer(0, 2, (int)VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            IGL.Primary.VertexAttribPointer(1, 2, (int)VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * 4);
            IGL.Primary.EnableVertexAttribArray(0);
            IGL.Primary.EnableVertexAttribArray(1);

            IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);

            vbo?.Unbind();
            ebo?.Unbind();
            GLTexture2D.Unbind();
        }
    }
}
