using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Textures
{
    public interface GLTexture
    {
        void SetFilter(int min, int mag);
        void SetWrap(int wrap);
        void GenerateMipMaps();
        void Bind();
        void Release();
    }
}
