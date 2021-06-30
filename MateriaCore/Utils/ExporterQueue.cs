using Materia.Graph;
using Materia.Graph.Exporters;
using Materia.Rendering.Geometry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Utils
{
    public static class ExporterQueue
    {
        private static Queue<Exporter> queue = new Queue<Exporter>();
        public static void Enqueue(Exporter exp)
        {
            queue.Enqueue(exp);
        }

        public static void Poll(Graph activeGraph)
        {
            if (activeGraph == null) return;

            if (queue.Count > 0 && queue.TryPeek(out var exporter))
            {
                if (exporter == null)
                {
                    queue.Dequeue();
                    return;
                }

                if (!exporter.IsValid(activeGraph))
                {
                    queue.Dequeue();
                    return;
                }

                FullScreenQuad.SharedVao?.Bind();
                if (!exporter.Next())
                {
                    exporter.Complete();
                    queue.Dequeue();
                }
                FullScreenQuad.SharedVao?.Unbind();
            }
        }

        public static void Clear()
        {
            queue.Clear();
        }
    }
}
