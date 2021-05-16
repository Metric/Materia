using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Rendering.Hdr
{
    public interface IHdrFile : IDisposable
    {
        int Width { get; }
        int Height { get; }

        float[] Pixels { get; }

        GLTexture2D Texture { get; }

        GLTexture2D GetTexture();

        bool Load();
    }
}
