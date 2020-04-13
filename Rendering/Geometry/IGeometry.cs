using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Rendering.Geometry
{
    public interface IGeometry : IDisposable
    {
        void Draw();
    }
}
