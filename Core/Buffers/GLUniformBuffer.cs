using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;
using Materia.Math3D;

namespace Materia.Buffers
{
    public class GLUniformBuffer
    {
        int id;

        public GLUniformBuffer()
        {
            id = IGL.Primary.GenBuffer();
        }

        public void Bind()
        {
            IGL.Primary.BindBuffer((int)BufferTarget.UniformBuffer, id);
        }

        public static void Unbind()
        {
            IGL.Primary.BindBuffer((int)BufferTarget.UniformBuffer, 0);
        }

        public void Release()
        {
            if(id != 0)
            {
                IGL.Primary.DeleteBuffer(id);
                id = 0;
            }
        }

        public void BindBase(int pos)
        {
            IGL.Primary.BindBufferBase((int)BufferRangeTarget.UniformBuffer, pos, id);
        }

        public void SetDataLength(int length)
        {
            IGL.Primary.BufferData((int)BufferTarget.UniformBuffer, length, new IntPtr(0), (int)BufferUsageHint.StaticDraw);
        }

        public void SetSubData(uint offset, bool b)
        {
            IntPtr p = new IntPtr(offset);
            IGL.Primary.BufferSubData((int)BufferTarget.UniformBuffer, p, 4, ref b);
        }

        public void SetSubData(uint offset, float f)
        {
            IntPtr p = new IntPtr(offset);
            IGL.Primary.BufferSubData((int)BufferTarget.UniformBuffer, p, 4, ref f);
        }

        public void SetSubData(uint offset, int i)
        {
            IntPtr p = new IntPtr(offset);
            IGL.Primary.BufferSubData((int)BufferTarget.UniformBuffer, p, 4, ref i);
        }

        public void SetSubData(uint offset, Vector4 v)
        {
            IntPtr p = new IntPtr(offset);
            IGL.Primary.BufferSubData((int)BufferTarget.UniformBuffer, p, 16, ref v);
        }

        public void SetSubData(uint offset, Matrix4 m)
        {
            IntPtr p = new IntPtr(offset);
            IGL.Primary.BufferSubData((int)BufferTarget.UniformBuffer, p, 64, ref m);
        }
    }
}
