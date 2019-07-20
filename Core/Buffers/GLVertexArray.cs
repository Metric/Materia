using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;

namespace Materia.Buffers
{
    public class GLVertexArray
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

        public void Release()
        {
            if(vao != 0)
            {
                IGL.Primary.DeleteVertexArray(vao);
                vao = 0;
            }
        }
    }
}
