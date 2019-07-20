using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;

namespace Materia.Buffers
{
    public class GLArrayBuffer
    {
        int vbo;

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

        public static void Unbind()
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

        public void Release()
        {
            if (vbo != 0)
            {
                IGL.Primary.DeleteBuffer(vbo);
                vbo = 0;
            }
        }
    }
}
