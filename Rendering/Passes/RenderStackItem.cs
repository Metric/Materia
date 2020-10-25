using Materia.Rendering.Textures;
using System;

namespace Materia.Rendering.Passes
{
    public abstract class RenderStackItem : IDisposable
    {
        public abstract void Dispose();
        public abstract void Render(GLTexture2D[] inputs, out GLTexture2D[] outputs);
    }
}
