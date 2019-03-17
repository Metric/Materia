using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Buffers;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Materia.Geometry
{
    public class FullScreenQuad : Geometry
    {
        static float[] buffer =
        {
            1,1,0, 1, 1,
            1,-1,0, 1, 0,
            -1,-1,0, 0, 0,
            -1,1,0, 0, 1
        };

        static uint[] indices =
        {
            0,1,3,1,2,3
        };

        GLVertexArray vao;
        GLElementBuffer ebo;
        GLArrayBuffer vbo;

        public FullScreenQuad()
        {
            vao = new GLVertexArray();
            ebo = new GLElementBuffer(BufferUsageHint.StaticDraw);
            vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);
            Setup();
        }

        void Setup()
        {
            vao.Bind();

            vbo.Bind();
            vbo.SetData(buffer);

            ebo.Bind();
            ebo.SetData(indices);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * 4);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GLVertexArray.Unbind();

            GLElementBuffer.Unbind();
            GLArrayBuffer.Unbind();
        }

        public override void Draw()
        {
            vao.Bind();
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GLVertexArray.Unbind();
        }

        public override void Release()
        {
            if(vao != null)
            {
                vao.Release();
                vao = null;
            }
            if(ebo != null)
            {
                ebo.Release();
                ebo = null;
            }
            if(vbo != null)
            {
                vbo.Release();
                vbo = null;
            }
        }
    }
}
