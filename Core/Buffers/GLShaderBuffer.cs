using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;
using Materia.Math3D;

namespace Materia.Buffers
{
    public class GLShaderBuffer
    {
        public int Id { get; protected set; }

        int lastKnownSize = 0;

        public GLShaderBuffer()
        {
            Id = IGL.Primary.GenBuffer();
        }

        public void Bind(int pos = 0)
        {
            IGL.Primary.BindBufferBase((int)BufferRangeTarget.ShaderStorageBuffer, pos, Id);
        }

        public void Storage(int size)
        {
            Storage(size, IntPtr.Zero);
        }

        public void Storage(int size, IntPtr data)
        {
            lastKnownSize = size;
            IGL.Primary.BufferStorage((int)BufferTarget.ShaderStorageBuffer, size, data, (int)(BufferStorageFlags.MapReadBit | BufferStorageFlags.MapCoherentBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapWriteBit));
        }

        public void Data(float[] data)
        {
            IGL.Primary.BufferData((int)BufferTarget.ShaderStorageBuffer, data.Length * sizeof(float), data, (int)BufferUsageHint.DynamicDraw);
        }

        public void Unbind(int pos = 0)
        {
            IGL.Primary.BindBufferBase((int)BufferRangeTarget.ShaderStorageBuffer, pos, 0);
        }

        public IntPtr MapRange(BufferAccessMask mask)
        {
            return IGL.Primary.MapBufferRange((int)BufferTarget.ShaderStorageBuffer, IntPtr.Zero, lastKnownSize, (int)mask);
        }

        public IntPtr Map(BufferAccess access)
        {
            return IGL.Primary.MapBuffer((int)BufferTarget.ShaderStorageBuffer, (int)access);
        }

        public void Unmap()
        {
            IGL.Primary.UnmapBuffer((int)BufferTarget.ShaderStorageBuffer);
        }

        public void Release()
        {
            IGL.Primary.DeleteBuffer(Id);
        }
    }
}
