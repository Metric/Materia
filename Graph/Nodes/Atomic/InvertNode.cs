using System;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class InvertNode : ImageNode
    {
        NodeInput input;
        NodeOutput output;

        bool red = true;
        bool green = true;
        bool blue = true;
        bool alpha = false;

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
            defaultName = Name = "Invert";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(output);
        }


        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (isDisposing) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new InvertProcessor();

            processor.Tiling = GetTiling();
            processor.Red = GetParameter("Red", red); ;
            processor.Green = GetParameter("Green", green);
            processor.Blue = GetParameter("Blue", blue);
            processor.Alpha = GetParameter("Alpha", alpha);

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            processor?.Dispose();
            processor = null;
        }

        public class InvertNodeData : NodeData
        {
            public bool red;
            public bool green;
            public bool blue;
            public bool alpha;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(red);
                w.Write(green);
                w.Write(blue);
                w.Write(alpha);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                red = r.NextBool();
                green = r.NextBool();
                blue = r.NextBool();
                alpha = r.NextBool();
            }
        }

        public override void GetBinary(Writer w)
        {
            InvertNodeData d = new InvertNodeData();
            FillBaseNodeData(d);
            d.red = red;
            d.green = green;
            d.blue = blue;
            d.alpha = alpha;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            InvertNodeData d = new InvertNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);

            red = d.red;
            green = d.green;
            blue = d.blue;
            alpha = d.alpha;
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
