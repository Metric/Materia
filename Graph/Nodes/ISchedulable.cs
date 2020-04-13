using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes
{
    public interface ISchedulable
    {
        bool IsScheduled { get; set; }
    }
}
