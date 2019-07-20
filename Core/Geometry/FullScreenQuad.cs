using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Buffers;
using Materia.GLInterfaces;
using Materia.Math3D;

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

            IGL.Primary.VertexAttribPointer(0, 3, (int)VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            IGL.Primary.VertexAttribPointer(1, 2, (int)VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * 4);
            IGL.Primary.EnableVertexAttribArray(0);
            IGL.Primary.EnableVertexAttribArray(1);
            GLVertexArray.Unbind();
            GLElementBuffer.Unbind();
            GLArrayBuffer.Unbind();
        }

        public override void Draw()
        {
            vao.Bind();
            IGL.Primary.DrawElements((int)BeginMode.Triangles, 6, (int)DrawElementsType.UnsignedInt, 0);
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
