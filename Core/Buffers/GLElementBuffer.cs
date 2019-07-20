using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;

namespace Materia.Buffers
{
    public class GLElementBuffer
    {
        int ebo;
        BufferUsageHint type;

        public GLElementBuffer(BufferUsageHint t)
        {
            type = t;
            ebo = IGL.Primary.GenBuffer();
        }

        public void Bind()
        {
            IGL.Primary.BindBuffer((int)BufferTarget.ElementArrayBuffer, ebo);
        }

        public static void Unbind()
        {
            IGL.Primary.BindBuffer((int)BufferTarget.ElementArrayBuffer, 0);
        }

        public void SetData(int[] indices)
        {
            IGL.Primary.BufferData((int)BufferTarget.ElementArrayBuffer, (int)(4 * indices.Length), indices, (int)type);
        }

        public void SetData(uint[] indices)
        {
            IGL.Primary.BufferData((int)BufferTarget.ElementArrayBuffer, (int)(4 * indices.Length), indices, (int)type);
        }

        public void SetSubData(uint[] indices, int offset)
        {
            IntPtr pointer = new IntPtr(offset);

            IGL.Primary.BufferSubData((int)BufferTarget.ArrayBuffer, pointer, (int)(4 * indices.Length), indices);
        }

        public void Release()
        {
            if(ebo != 0)
            {
                IGL.Primary.DeleteBuffer(ebo);
                ebo = 0;
            }
        }
    }
}
