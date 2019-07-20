using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;

namespace Materia.Buffers
{
    public class GLRenderBuffer
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

        public static void Unbind()
        {
            IGL.Primary.BindRenderbuffer((int)RenderbufferTarget.Renderbuffer, 0);
        }

        public void SetBufferStorageAsColor(int width, int height)
        {
            IGL.Primary.RenderbufferStorage((int)RenderbufferTarget.Renderbuffer, (int)RenderbufferStorage.Rgba32f, width, height);
        }

        public void SetBufferStorageAsDepth(int width, int height)
        {
            IGL.Primary.RenderbufferStorage((int)RenderbufferTarget.Renderbuffer, (int)RenderbufferStorage.Depth32fStencil8, width, height);
        }

        public void Release()
        {
            if(Id != 0)
            {
                IGL.Primary.DeleteRenderbuffer(Id);
                Id = 0;
            }
        }
    }
}
