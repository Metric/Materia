using System;
using Materia.Rendering.Interfaces;

namespace Materia.Rendering.Buffers
{
    public class GLVertexArray : IDisposable
    {
        int vao;

        public GLVertexArray()
        {
            vao = IGL.Primary.GenVertexArray();
        }

        public void Bind()
        {
            IGL.Primary.BindVertexArray(vao);
        }

        public static void Unbind()
        {
            IGL.Primary.BindVertexArray(0);
        }

        public void Dispose()
        {
            if(vao != 0)
            {
                IGL.Primary.DeleteVertexArray(vao);
                vao = 0;
            }
        }
    }
}
