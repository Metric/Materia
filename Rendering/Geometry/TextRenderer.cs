using Materia.Rendering.Buffers;
using Materia.Rendering.Geometry;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rendering.Geometry
{
    public class TextData
    {
        public Vector2 pos;
        public Vector2 size;
        public Vector4 uv;
    }

    public class TextRenderer : IGeometry, IDisposeShared
    {
        public List<TextData> Text = new List<TextData>();

        protected GLArrayBuffer vbo;
        protected int pointCount = 0;

        protected static bool isSharedDisposed = false;
        protected static GLVertexArray sharedVao;

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

        public TextRenderer(BufferUsageHint hint = BufferUsageHint.StaticDraw)
        {
            GeometryCache.RegisterForDispose(this);
            vbo = new GLArrayBuffer(hint);
        }

        public void Dispose()
        {
            vbo?.Dispose();
            vbo = null;
        }

        public void Draw()
        {
            if (Text == null || Text.Count == 0 || vbo == null)
            {
                return;
            }

            if (pointCount > 0)
            {
                vbo?.Bind();
                IGL.Primary.VertexAttribPointer(0, 2, (int)VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                IGL.Primary.VertexAttribPointer(1, 2, (int)VertexAttribPointerType.Float, false, 8 * sizeof(float), 2 * 4);
                IGL.Primary.VertexAttribPointer(2, 4, (int)VertexAttribPointerType.Float, false, 8 * sizeof(float), 4 * 4);
                IGL.Primary.EnableVertexAttribArray(0);
                IGL.Primary.EnableVertexAttribArray(1);
                IGL.Primary.EnableVertexAttribArray(2);

                IGL.Primary.DrawArrays((int)PrimitiveType.Points, 0, pointCount);
                vbo?.Unbind();
            }
        }

        /// <summary>
        /// Disposes the shared resources. Call before exiting the application fully.
        /// To properly release GPU resources for Shared VertexArray
        /// </summary>
        public void DisposeShared()
        {
            isSharedDisposed = true;
            sharedVao?.Dispose();
            sharedVao = null;
        }

        public void Update()
        {
            pointCount = Text.Count;
            if (Text.Count <= 0) return;
            List<float> buffer = new List<float>();
            for (int i = 0; i < Text.Count; ++i)
            {
                var c = Text[i];
                buffer.Add(c.pos.X);
                buffer.Add(c.pos.Y);
                buffer.Add(c.size.X);
                buffer.Add(c.size.Y);
                buffer.Add(c.uv.X);
                buffer.Add(c.uv.Y);
                buffer.Add(c.uv.Z);
                buffer.Add(c.uv.W);
            }

            vbo?.Bind();
            vbo?.SetData(buffer.ToArray());
            vbo?.Unbind();
        }
    }
}
