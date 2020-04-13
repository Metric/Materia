using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Materia.Rendering.Geometry;

namespace Materia.Rendering.Importer
{
    public interface IMeshImporter
    {
        List<Mesh> Parse(Stream stream);
        List<Mesh> Parse(string path);
    }
}
