using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.Attributes
{
    public class FileSelectorAttribute : Attribute
    {
        public string Filter { get; set; }
    }
}
