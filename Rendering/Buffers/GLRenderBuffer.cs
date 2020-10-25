using System;
using Materia.Rendering.Interfaces;

namespace Materia.Rendering.Buffers
{
    public class GLRenderBuffer : IDisposable
    {
        public int Id { get; protected set; }

        public GLRenderBuffer()
        {
            Id = IGL.Primary.GenRenderbuffer();
        }

        public void Bind()
        {
            IGL.Primary.BindRenderbuffer((int)RenderbufferTarget.Renderbuffer, Id);
        }

        public void Unbind()
        {
            IGL.Primary.BindRenderbuffer((int)RenderbufferTarget.Renderbuffer, 0);
        }

        public void SetBufferStorageAsColor(int width, int height)
        {
            IGL.Primary.RenderbufferStorage((int)RenderbufferTarget.Renderbuffer, (int)RenderbufferStorage.Rgba32f, width, height);
        }

        public void SetBufferStorageAsDepth(int width, int height)
        {
            IGL.Primary.RenderbufferStorage((int)RenderbufferTarget.Renderbuffer, (int)RenderbufferStorage.Depth24Stencil8, width, height);
        }

        public void Dispose()
        {
            if(Id != 0)
            {
                IGL.Primary.DeleteRenderbuffer(Id);
                Id = 0;
            }
        }
    }
}
