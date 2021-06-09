using Newtonsoft.Json;
using Materia.Graph;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using System;

namespace Materia.Nodes
{
    public abstract class ImageNode : Node
    {
        private ImageProcessor previewProcessor;

        public override void FromJson(string data, Archive archive = null)
        {
            FromJson(data);
        }

        public virtual void FromJson(string data)
        {
            NodeData d = JsonConvert.DeserializeObject<NodeData>(data);
            SetBaseNodeDate(d);
        }

        public override string GetJson()
        {
            NodeData d = new NodeData();
            FillBaseNodeData(d);
            return JsonConvert.SerializeObject(d);
        }

        public override byte[] Export(int w = 0, int h = 0)
        {
            if (previewProcessor == null) previewProcessor = new ImageProcessor();
            
            if (buffer == null) return null;

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

            return data;
        }

        public override void Dispose()
        {
            base.Dispose();
            previewProcessor?.Dispose();
            previewProcessor = null;
        }
    }
}
