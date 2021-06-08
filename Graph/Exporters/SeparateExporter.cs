using System.Collections.Generic;
using System.Threading.Tasks;
using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Rendering.Imaging;
using System.IO;

namespace Materia.Graph.Exporters
{
    public class SeparateExporter : Exporter
    {
        Graph graph;
        int index = 0;
        int totalOutputs = 0;
        string path;

        public SeparateExporter(Graph g, string exportPath)
        {
            if (g == null) return;

            path = exportPath;
            graph = g;
            index = 0;
            totalOutputs = graph.OutputNodes.Count;
        }

        public override bool Next()
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (!Directory.Exists(path)) return false;
            if (graph == null) return false;
            if (totalOutputs <= 0) return false;
            if (index >= totalOutputs) return false;

            string name = graph.Name;
            string nid = graph.OutputNodes[index++];

            ProgressChanged(index, totalOutputs, (float)index / (float)totalOutputs);

            Node n = null;
            if (graph.NodeLookup.TryGetValue(nid, out n))
            {
                OutputNode on = n as OutputNode;

                if (on == null) return true;

                string extension = $"_{on.OutType}.png";

                RawBitmap bmp = null;
                byte[] bits = on.Export();

                if (bits == null) return true;

                bmp = new RawBitmap(on.Width, on.Height, bits);
                var src = bmp.ToBitmap();

                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + extension), FileMode.OpenOrCreate))
                {
                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                }

                src.Dispose();
            }

            return true;
        }

        public override void Complete()
        {
            //do nothing here
        }

        public override bool IsValid(Graph g)
        {
            return g == graph;
        }
    }
}
