using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Interfaces
{
    public interface IDrawable : ILayout
    {
        void Draw(Matrix4 projection);
    }
}
