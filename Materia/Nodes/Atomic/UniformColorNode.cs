using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Newtonsoft.Json;
using Materia.Imaging.GLProcessing;
using Materia.Nodes.Attributes;

namespace Materia.Nodes.Atomic
{
    public class UniformColorNode : ImageNode
    {
        Vector4 color;

        [ColorPicker]
        public Vector4 Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                TryAndProcess();
            }
        }

        [HideProperty]
        public new float TileX
        {
            get
            {
                return tileX;
            }
            set
            {
                tileX = value;
            }
        }

        [HideProperty]
        public new float TileY
        {
            get
            {
                return tileY;
            }
            set
            {
                tileY = value;
            }
        }

        NodeOutput output;
        UniformColorProcessor processor;

        public UniformColorNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Uniform Color";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            tileX = tileY = 1;

            color = new Vector4(0, 0, 0, 1);

            processor = new UniformColorProcessor();
            previewProcessor = new BasicImageRenderer();

            internalPixelType = p;

            Inputs = new List<NodeInput>();
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs = new List<NodeOutput>();
            Outputs.Add(output);

        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            CreateBufferIfNeeded();

            processor.Color = color;
            processor.Process(width, height, null, buffer);
            processor.Complete();

            Updated();
            output.Data = buffer;
            output.Changed();
        }

        public class UniformColorNodeData : NodeData
        {
            public float[] color;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            UniformColorNodeData d = JsonConvert.DeserializeObject<UniformColorNodeData>(data);
            SetBaseNodeDate(d);
            float[] c = d.color;
            color = new Vector4(c[0], c[1], c[2], c[3]);

            SetConnections(nodes, d.outputs);

            OnWidthHeightSet();
        }

        public override string GetJson()
        {
            UniformColorNodeData d = new UniformColorNodeData();
            FillBaseNodeData(d);

            d.color = new float[] { color.X, color.Y, color.Z, color.W };

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(processor != null)
            {
                processor.Release();
            }
        }
    }
}
