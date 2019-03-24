using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;
using Materia.Imaging.GLProcessing;
using Materia.Textures;

namespace Materia.Nodes.Atomic
{
    public class BlurNode : ImageNode
    {
        NodeInput input;

        int intensity;

        NodeOutput Output;

        BlurProcessor processor;

        [Promote(NodeType.Float)]
        [Slider(IsInt = true, Max = 128, Min = 1, Snap = false, Ticks = new float[0])]
        public int Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;

                if (intensity < 0) intensity = 0;

                TryAndProcess();
            }
        }

        public BlurNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Blur";

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            width = w;
            height = h;

            intensity = 10;

            internalPixelType = p;

            previewProcessor = new BasicImageRenderer();
            processor = new BlurProcessor();

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputRemoved += Input_OnInputRemoved;
            input.OnInputChanged += Input_OnInputChanged;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        protected override void OnWidthHeightSet()
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

            processor.TileX = 1;
            processor.TileY = 1;

            int pintensity = intensity;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "Intensity"))
            {
                pintensity = ParentGraph.GetParameterValue<int>(Id, "Intensity");
            }

            processor.Intensity = pintensity;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            previewProcessor.TileX = tileX;
            previewProcessor.TileY = tileY;

            previewProcessor.Process(width, height, buffer, buffer);
            previewProcessor.Complete();

            previewProcessor.TileY = 1;
            previewProcessor.TileX = 1;

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

        public class BlurData : NodeData
        {
            public int intensity;
        }

        public override string GetJson()
        {
            BlurData d = new BlurData();
            FillBaseNodeData(d);
            d.intensity = intensity;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            BlurData d = JsonConvert.DeserializeObject<BlurData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;

            SetConnections(nodes, d.outputs);

            OnWidthHeightSet();
        }
    }
}
