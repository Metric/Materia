using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Materia.Buffers
{
    public class GLArrayBuffer
    {
        int vbo;

        BufferUsageHint type;

        public GLArrayBuffer(BufferUsageHint t)
        {
            type = t;
            vbo = GL.GenBuffer();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        }

        public static void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void SetData(float[] data)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, (int)(4 * data.Length), data, type);
        }

        public void SetSubData(float[] data, int offset)
        {
            IntPtr pointer = new IntPtr(offset);

            GL.BufferSubData(BufferTarget.ArrayBuffer, pointer, (int)(4 * data.Length), data);
        }

        public void Release()
        {
            if (vbo != 0)
            {
                GL.DeleteBuffer(vbo);
                vbo = 0;
            }
        }
    }
}
