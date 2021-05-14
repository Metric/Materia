using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Renderer
{
    public interface ISceneRenderer : IDisposable
    {
        GLTexture2D Image { get; }

        void Render();
    }
}
