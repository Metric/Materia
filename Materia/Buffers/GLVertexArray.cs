using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Materia.Buffers
{
    public class GLVertexArray
    {
        int vao;

        public GLVertexArray()
        {
            vao = GL.GenVertexArray();
        }

        public void Bind()
        {
            GL.BindVertexArray(vao);
        }

        public static void Unbind()
        {
            GL.BindVertexArray(0);
        }

        public void Release()
        {
            if(vao != 0)
            {
                GL.DeleteVertexArray(vao);
                vao = 0;
            }
        }
    }
}
