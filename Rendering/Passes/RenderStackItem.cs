using Materia.Rendering.Textures;

namespace Materia.Rendering.Passes
{
    public abstract class RenderStackItem
    {
        public abstract void Release();
        public abstract void Render(GLTexture2D[] inputs, out GLTexture2D[] outputs);
    }
}
