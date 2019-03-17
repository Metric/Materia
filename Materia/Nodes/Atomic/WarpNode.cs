using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;

namespace Materia.Nodes.Atomic
{
    public class WarpNode : ImageNode
    {
        protected WarpProcessor processor;

        protected NodeInput input;
        protected NodeInput input1;

        protected NodeOutput output;

        protected float intensity;

        [Slider(IsInt = false, Max = 1.0f, Min = 0f, Snap = false, Ticks = new float[0])]
        public float Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;
                TryAndProcess();
            }
        }

        public WarpNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Warp";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            intensity = 1;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new WarpProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Image Input");
            input1 = new NodeInput(NodeType.Gray, this, "Grayscale Gradients");

            output = new NodeOutput(NodeType.Gray | NodeType.Color, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            input1.OnInputAdded += Input_OnInputAdded;
            input1.OnInputChanged += Input_OnInputChanged;
            input1.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Outputs = new List<NodeOutput>();

            Inputs.Add(input);
            Inputs.Add(input1);
            Outputs.Add(output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            output.Data = null;
            output.Changed();
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if(input.HasInput && input1.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;
            GLTextuer2D i2 = (GLTextuer2D)input1.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (i2 == null) return;
            if (i2.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = TileY;
            processor.Intensity = intensity;
            processor.Process(width, height, i1, i2, buffer);
            processor.Complete();

            Updated();
            output.Data = buffer;
            output.Changed();
        }

        public class WarpData : NodeData
        {
            public float intensity;
        }

        public override string GetJson()
        {
            WarpData d = new WarpData();
            FillBaseNodeData(d);
            d.intensity = intensity;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            WarpData d = JsonConvert.DeserializeObject<WarpData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;

            SetConnections(nodes, d.outputs);

            OnWidthHeightSet();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(processor != null)
            {
                processor.Release();
                processor = null;
            }
        }
    }
}
