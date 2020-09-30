using Materia.Rendering.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Rendering.Geometry
{
    public enum RenderType
    {
        Stroke,
        Polygon,
        Mesh
    }

    public interface IRenderable : IQuadComparable
    {
        int Texture { get; }
        RenderType RenderType { get; }
        void UpdateBounds();

    }

    public interface IGeometry : IDisposable
    {
        void Draw();

        void Update();
    }
}
