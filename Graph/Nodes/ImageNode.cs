using Newtonsoft.Json;
using Materia.Graph;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using System;
using Materia.Graph.IO;

namespace Materia.Nodes
{
    public abstract class ImageNode : Node
    {
        public override void FromBinary(Reader r, Archive archive = null)
        {
            FromBinary(r);
        }

        public override void FromJson(string data, Archive archive = null)
        {
            FromJson(data);
        }

        public virtual void FromBinary(Reader r)
        {
            NodeData d = new NodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
        }

        public virtual void FromJson(string data)
        {
            NodeData d = JsonConvert.DeserializeObject<NodeData>(data);
            SetBaseNodeDate(d);
        }

        public override void GetBinary(Writer w)
        {
            NodeData d = new NodeData();
            FillBaseNodeData(d);
            d.Write(w);
        }

        public override string GetJson()
        {
            NodeData d = new NodeData();
            FillBaseNodeData(d);
            return JsonConvert.SerializeObject(d);
        }

        public override byte[] Export(int w = 0, int h = 0)
        {
            if (buffer == null) return null;

            ImageProcessor previewProcessor = new ImageProcessor();

            int nwidth = w <= 0 ? width : w;
            int nheight = h <= 0 ? height : h;

            GLTexture2D temp = new GLTexture2D(buffer.InternalFormat);
            temp.Bind();
            temp.SetData(IntPtr.Zero, Rendering.Interfaces.PixelFormat.Bgra, nwidth, nheight);
            temp.ClampToEdge();
            temp.Linear();
            GLTexture2D.Unbind();

            previewProcessor.PrepareView(temp);
            previewProcessor.Process(buffer);
            byte[] data = previewProcessor.ReadByte(nwidth, nheight);
            previewProcessor.Complete();
            temp.Dispose();
            previewProcessor.Dispose();

            return data;
        }
    }
}
