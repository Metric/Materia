using Materia.Rendering.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Nodes
{
    public interface INodePoint : IDisposable
    {
        NodeType Type { get; }
        string Name { get; }
    }
}
