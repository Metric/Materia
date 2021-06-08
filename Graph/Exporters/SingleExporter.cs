using Materia.Nodes;
using Materia.Rendering.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Materia.Graph.Exporters
{
    public class SingleExporter : Exporter
    {
        Node node;
        string path;

        public SingleExporter(Node n, string exportPath)
        {
            if (n == null) return;

            path = exportPath;
        }

        public override bool Next()
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (node == null) return false;

            byte[] bits = node.Export();
            if (bits == null) return false;
            RawBitmap bmp = new RawBitmap(node.Width, node.Height, bits);

            var src = bmp.ToBitmap();

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
            }

            src.Dispose();

            return false;
        }

        public override void Complete()
        {
           //do nothing here
        }

        public override bool IsValid(Graph g)
        {
            if (node == null || node.ParentGraph != g) return false;
            return true;
        }
    }
}
