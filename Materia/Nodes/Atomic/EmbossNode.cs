using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Nodes.Attributes;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class EmbossNode : ImageNode
    {
        NodeInput input;

        int angle;

        NodeOutput Output;

        EmbossProcessor processor;

        [Slider(IsInt = true, Max = 360, Min = 0, Snap = false, Ticks = new float[0])]
        public int Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
                TryAndProcess();
            }
        }

        int elevation;

        [Slider(IsInt = true, Max = 180, Min = 0, Snap = false, Ticks = new float[0])]
        public int Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;
                TryAndProcess();
            }
        }

        public EmbossNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Emboss";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            elevation = 2;
            angle = 0;

            tileX = tileY = 1;

            processor = new EmbossProcessor();

            previewProcessor = new BasicImageRenderer();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Gray, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        public class EmbossNodeData : NodeData
        {
            public int angle;
            public int elevation;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            EmbossNodeData d = JsonConvert.DeserializeObject<EmbossNodeData>(data);
            SetBaseNodeDate(d);
            angle = d.angle;
            elevation = d.elevation;

            SetConnections(nodes, d.outputs);

            OnWidthHeightSet();
        }

        public override string GetJson()
        {
            EmbossNodeData d = new EmbossNodeData();

            FillBaseNodeData(d);
            d.angle = angle;
            d.elevation = elevation;

            return JsonConvert.SerializeObject(d);
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
            processor.Azimuth = angle * (float)(Math.PI / 180.0f);
            processor.Elevation = elevation * (float)(Math.PI / 180.0f);
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
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

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }
    }
}
