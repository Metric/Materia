using Materia.Rendering.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Rendering.Imaging.Processing
{
    public class UVProcessor : ImageProcessor
    {
        public void Process(UVRenderer renderer)
        {
            renderer?.Draw();
        }
    }
}
