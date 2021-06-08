using Newtonsoft.Json;
using Materia.Graph;
using Materia.Rendering.Imaging.Processing;

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

        public override byte[] Export()
        {
            if (previewProcessor == null) previewProcessor = new ImageProcessor();
            
            if (buffer == null) return null;

            var temp = buffer.Copy();
            if (temp == null) return null;
            
            previewProcessor.PrepareView(temp);
            previewProcessor.Process(buffer);
            byte[] data = previewProcessor.ReadByte(width, height);
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
