using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;

namespace Materia.Rendering
{
    public abstract class RenderStackItem
    {
        public abstract void Release();
        public abstract void Render(GLTextuer2D[] inputs, out GLTextuer2D[] outputs);
    }
}
