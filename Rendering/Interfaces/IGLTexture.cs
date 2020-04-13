using System;

namespace Materia.Rendering.Textures
{
    public interface IGLTexture : IDisposable
    {
        void SetFilter(int min, int mag);
        void SetWrap(int wrap);
        void GenerateMipMaps();
        void Bind();
    }
}
