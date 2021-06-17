using Materia.Nodes;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL
{
    public interface IGraphNode
    {
        Node Node { get; }
        UIGraph Graph { get; }
        string Id { get; }

        Box2 GetViewSpaceRect();

        void Snap();

        void LoadConnection(UINodePoint n, NodeInput p);
        void LoadConnections();
        void Restore();
    }
}
