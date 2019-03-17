using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Materia.Buffers
{
    public class GLUniformBuffer
    {
        int id;

        public GLUniformBuffer()
        {
            id = GL.GenBuffer();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, id);
        }

        public static void Unbind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void Release()
        {
            if(id != 0)
            {
                GL.DeleteBuffer(id);
                id = 0;
            }
        }

        public void BindBase(int pos)
        {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, pos, id);
        }

        public void SetDataLength(int length)
        {
            GL.BufferData(BufferTarget.UniformBuffer, length, new IntPtr(0), BufferUsageHint.StaticDraw);
        }

        public void SetSubData(uint offset, bool b)
        {
            IntPtr p = new IntPtr(offset);
            GL.BufferSubData(BufferTarget.UniformBuffer, p, 4, ref b);
        }

        public void SetSubData(uint offset, float f)
        {
            IntPtr p = new IntPtr(offset);
            GL.BufferSubData(BufferTarget.UniformBuffer, p, 4, ref f);
        }

        public void SetSubData(uint offset, int i)
        {
            IntPtr p = new IntPtr(offset);
            GL.BufferSubData(BufferTarget.UniformBuffer, p, 4, ref i);
        }

        public void SetSubData(uint offset, Vector4 v)
        {
            IntPtr p = new IntPtr(offset);
            GL.BufferSubData(BufferTarget.UniformBuffer, p, 16, ref v);
        }

        public void SetSubData(uint offset, Matrix4 m)
        {
            IntPtr p = new IntPtr(offset);
            GL.BufferSubData(BufferTarget.UniformBuffer, p, 64, ref m);
        }
    }
}
