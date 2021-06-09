using Materia.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Graph.Exporters
{
    public class MemoryExporter : Exporter
    {
        protected byte[] rawData;
        protected Node node;
        protected Action<byte[]> onComplete;
        protected int width = 0;
        protected int height = 0;

        public MemoryExporter(Node n, int w = 0, int h = 0, Action<byte[]> completed = null)
        {
            width = w;
            height = h;
            onComplete = completed;
            node = n;
        }

        public override bool Next()
        {
            if (node == null || node.ParentGraph == null) return false;
            rawData = node.Export(width, height);
            return false;
        }

        public override bool IsValid(Graph g)
        {
            return node != null && node.ParentGraph == g;
        }

        public override void Complete()
        {
            //call callback
            onComplete?.Invoke(rawData);
        }
    }
}
