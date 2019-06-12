using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Materia.Buffers
{
    public class GLRenderBuffer
    {
        public int Id { get; protected set; }

        public GLRenderBuffer()
        {
            Id = GL.GenRenderbuffer();
        }

        public void Bind()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Id);
        }

        public static void Unbind()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }

        public void SetBufferStorageAsColor(int width, int height)
        {
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba32f, width, height);
        }

        public void SetBufferStorageAsDepth(int width, int height)
        {
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth32fStencil8, width, height);
        }

        public void Release()
        {
            if(Id != 0)
            {
                GL.DeleteRenderbuffer(Id);
                Id = 0;
            }
        }
    }
}
