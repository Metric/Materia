using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class GradientDynamicNode : ImageNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput Output;

        GradientMapProcessor processor;

        protected bool horizontal = true;
        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Horizontal")]
        public bool Horizontal
        {
            get
            {
                return horizontal;
            }
            set
            {
                horizontal = value;
                TriggerValueChange();
            }
        }

        public GradientDynamicNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            defaultName = Name = "Gradient Dynamic";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray, this, "Image Input");
            input2 = new NodeInput(NodeType.Color, this, "Gradient Input");
            input3 = new NodeInput(NodeType.Gray, this, "Mask");
            Output = new NodeOutput(NodeType.Color, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);
            Outputs.Add(Output);
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (isDisposing) return;
            if (!input.HasInput || !input2.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;
            GLTexture2D i2 = (GLTexture2D)input2.Reference.Data;
            GLTexture2D i3 = null;

            if (input3.HasInput)
            {
                if (input3.Reference.Data != null)
                {
                    i3 = (GLTexture2D)input3.Reference.Data;
                }
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;
            if (i2 == null) return;
            if (i2.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new GradientMapProcessor();

            //setting params must go before PrepareView()
            processor.Tiling = GetTiling();
            processor.Horizontal = GetParameter("Horizontal", horizontal);
            processor.ColorLUT = i2;
            processor.Mask = i3;

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class GradientMapData : NodeData
        {
            public bool horizontal;
            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(horizontal);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                horizontal = r.NextBool();
            }
        }

        public override void GetBinary(Writer w)
        {
            GradientMapData d = new GradientMapData();
            FillBaseNodeData(d);
            d.horizontal = horizontal;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            GradientMapData d = new GradientMapData();
            d.Parse(r);
            SetBaseNodeDate(d);
            horizontal = d.horizontal;
        }

        public override void FromJson(string data)
        {
            GradientMapData d = JsonConvert.DeserializeObject<GradientMapData>(data);
            SetBaseNodeDate(d);
            horizontal = d.horizontal;
        }

        public override string GetJson()
        {
            GradientMapData d = new GradientMapData();
            FillBaseNodeData(d);
            d.horizontal = horizontal;

            return JsonConvert.SerializeObject(d);
        }
    }
}
