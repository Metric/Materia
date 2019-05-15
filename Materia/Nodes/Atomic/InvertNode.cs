using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Imaging.GLProcessing;
using Materia.Textures;

namespace Materia.Nodes.Atomic
{
    public class InvertNode : ImageNode
    {
        NodeInput input;
        NodeOutput output;

        bool red;
        bool green;
        bool blue;
        bool alpha;

        InvertProcessor processor;

        public bool Red
        {
            get
            {
                return red;
            }
            set
            {
                red = value;
                TryAndProcess();
            }
        }

        public bool Green
        {
            get
            {
                return green;
            }
            set
            {
                green = value;
                TryAndProcess();
            }
        }

        public bool Blue
        {
            get
            {
                return blue;
            }
            set
            {
                blue = value;
                TryAndProcess();
            }
        }

        public bool Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
                TryAndProcess();
            }
        }

        public InvertNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Invert";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;
            red = true;
            blue = true;
            green = true;
            alpha = false;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new InvertProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
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
            if(input.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Red = red;
            processor.Green = green;
            processor.Blue = blue;
            processor.Alpha = alpha;

            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Updated();
            output.Data = buffer;
            output.Changed();
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

        public class InvertNodeData : NodeData
        {
            public bool red;
            public bool blue;
            public bool green;
            public bool alpha;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            InvertNodeData d = JsonConvert.DeserializeObject<InvertNodeData>(data);
            SetBaseNodeDate(d);

            red = d.red;
            green = d.green;
            blue = d.blue;
            alpha = d.alpha;
        }

        public override string GetJson()
        {
            InvertNodeData d = new InvertNodeData();
            FillBaseNodeData(d);
            d.red = red;
            d.green = green;
            d.blue = blue;
            d.alpha = alpha;

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }
    }
}
