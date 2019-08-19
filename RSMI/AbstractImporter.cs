using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSMI.Containers;
using System.IO;

namespace RSMI
{
    public abstract class AbstractImporter
    {
        public abstract List<Containers.Mesh> Parse(Stream stream);
        public abstract List<Mesh> Parse(string path);
    }
}
