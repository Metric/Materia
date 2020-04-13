using System;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;

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

        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Red")]
        public bool Red
        {
            get
            {
                return red;
            }
            set
            {
                red = value;
                TriggerValueChange();
            }
        }

        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Green")]
        public bool Green
        {
            get
            {
                return green;
            }
            set
            {
                green = value;
                TriggerValueChange();
            }
        }

        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Blue")]
        public bool Blue
        {
            get
            {
                return blue;
            }
            set
            {
                blue = value;
                TriggerValueChange();
            }
        }

        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Alpha")]
        public bool Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
                TriggerValueChange();
            }
        }

        public InvertNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
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

            Inputs.Add(input);
            Outputs.Add(output);
        }


        private void GetParams()
        {
            if (!input.HasInput) return;

            pred = red;
            pgreen = green;
            pblue = blue;
            palpha = alpha;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "Red"))
            {
                pred = ParentGraph.GetParameterValue(Id, "Red").ToBool();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Green"))
            {
                pgreen = ParentGraph.GetParameterValue(Id, "Green").ToBool();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Blue"))
            {
                pblue = ParentGraph.GetParameterValue(Id, "Blue").ToBool();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Alpha"))
            {
                palpha = ParentGraph.GetParameterValue(Id, "Alpha").ToBool();
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        bool pred;
        bool pgreen;
        bool pblue;
        bool palpha;
        void Process()
        {
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Red = pred;
            processor.Green = pgreen;
            processor.Blue = pblue;
            processor.Alpha = palpha;

            processor.Process(width, height, i1, buffer);
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if(processor != null)
            {
                processor.Dispose();
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

        public override void FromJson(string data)
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
    }
}
