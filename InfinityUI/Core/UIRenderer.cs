using Materia.Rendering.Buffers;
using Materia.Rendering.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Core
{
    public static class UIRenderer
    {
        public static GLVertexArray Vao { get; set; }
        public static GLArrayBuffer Vbo { get; set; }
        public static int StencilStage { get; set; } = 0;

        public static void Init()
        {
            if (Vao == null)
            {
                Vao = new GLVertexArray();
            }

            if (Vbo == null)
            {
                Vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);
                Vbo.Bind();
                Vbo.SetData(new float[] { 0, 0 });
                Vbo.Unbind();
            }
        }

        public static void Bind()
        {
            Vao?.Bind();
            Vbo?.Bind();
        }

        public static void Draw()
        {
            IGL.Primary.VertexAttribPointer(0, 2, (int)VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            IGL.Primary.EnableVertexAttribArray(0);

            IGL.Primary.DrawArrays((int)PrimitiveType.Points, 0, 1);
        }

        public static void Unbind()
        {
            Vbo?.Unbind();
            Vao?.Unbind();
        }

        public static void Dispose()
        {
            Vbo?.Dispose();
            Vbo = null;

            Vao?.Dispose();
            Vao = null;
        }
    }
}
