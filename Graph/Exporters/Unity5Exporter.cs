using System.Collections.Generic;
using System.Threading.Tasks;
using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Rendering.Imaging;
using System.IO;
using Materia.Rendering.Extensions;

namespace Materia.Graph.Exporters
{
    public class Unity5Exporter : Exporter
    {
        string path;
        Graph graph;
        int index;
        int totalOutputs;
        RawBitmap merge;
        public Unity5Exporter(Graph g, string exportPath)
        {
            if (g == null) return;

            path = exportPath;
            graph = g;
            index = 0;
            totalOutputs = g.OutputNodes.Count;

            int maxWidth = 0;
            int maxHeight = 0;

            for (int i = 0; i < g.OutputNodes.Count; ++i)
            {
                var id = g.OutputNodes[i];
                if (g.NodeLookup.TryGetValue(id, out Node n))
                {
                    OutputNode on = n as OutputNode;
                    if (on == null) continue;
                    if (on.OutType == OutputType.metallic || on.OutType == OutputType.roughness)
                    {
                        maxWidth = n.Width.Max(maxWidth);
                        maxHeight = n.Height.Max(maxHeight);
                    }
                }
            }

            merge = new RawBitmap(maxWidth, maxHeight);
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

                switch (on.OutType)
                {
                    case OutputType.metallic:
                        if (merge.Width >= bmp.Width && merge.Height >= bmp.Height)
                        {
                            merge.CopyRedToRed(bmp);
                        }
                        break;
                    case OutputType.roughness:
                        if (merge.Width >= bmp.Width && merge.Height >= bmp.Height)
                        {
                            merge.CopyRedToAlpha(bmp);
                        }
                        break;
                    default:
                        var src = bmp.ToBitmap();

                        using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + extension), FileMode.OpenOrCreate))
                        {
                            src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                        }

                        src.Dispose();
                        break;
                }
            }

            return true;
        }

        public override void Complete()
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path)) return;
            if (graph == null) return;

            string name = graph.Name;

            if (merge.Width > 0 && merge.Height > 0)
            {
                var src = merge.ToBitmap();

                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_metalrough.png"), FileMode.OpenOrCreate))
                {
                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                }

                src.Dispose();
            }
        }

        public override bool IsValid(Graph g)
        {
            return g == graph;
        }
    }
}
