using System;
using Materia.Rendering.Interfaces;

namespace Materia.Rendering.Buffers
{
    public class GLArrayBuffer : IDisposable
    {
        int vbo;

        public int Id
        {
            get
            {
                return vbo;
            }
        }

        BufferUsageHint type;

        public GLArrayBuffer(BufferUsageHint t)
        {
            type = t;
            vbo = IGL.Primary.GenBuffer();
        }

        public void Bind()
        {
            IGL.Primary.BindBuffer((int)BufferTarget.ArrayBuffer, vbo);
        }

        public void Unbind()
        {
            IGL.Primary.BindBuffer((int)BufferTarget.ArrayBuffer, 0);
        }

        public void SetData(float[] data)
        {
            IGL.Primary.BufferData((int)BufferTarget.ArrayBuffer, (int)(4 * data.Length), data, (int)type);
        }

        public void SetSubData(float[] data, int offset)
        {
            IntPtr pointer = new IntPtr(offset);

            IGL.Primary.BufferSubData((int)BufferTarget.ArrayBuffer, pointer, (int)(4 * data.Length), data);
        }

        public void Dispose()
        {
            if (vbo != 0)
            {
                IGL.Primary.DeleteBuffer(vbo);
                vbo = 0;
            }
        }
    }
}
