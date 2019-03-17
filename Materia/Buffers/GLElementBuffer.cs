using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Materia.Buffers
{
    public class GLElementBuffer
    {
        int ebo;
        BufferUsageHint type;

        public GLElementBuffer(BufferUsageHint t)
        {
            type = t;
            ebo = GL.GenBuffer();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        }

        public static void Unbind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void SetData(int[] indices)
        {
            GL.BufferData(BufferTarget.ElementArrayBuffer, (int)(4 * indices.Length), indices, type);
        }

        public void SetData(uint[] indices)
        {
            GL.BufferData(BufferTarget.ElementArrayBuffer, (int)(4 * indices.Length), indices, type);
        }

        public void SetSubData(uint[] indices, int offset)
        {
            IntPtr pointer = new IntPtr(offset);

            GL.BufferSubData(BufferTarget.ArrayBuffer, pointer, (int)(4 * indices.Length), indices);
        }

        public void Release()
        {
            if(ebo != 0)
            {
                GL.DeleteBuffer(ebo);
                ebo = 0;
            }
        }
    }
}
