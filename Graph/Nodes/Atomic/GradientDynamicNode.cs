using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Atomic
{
    public class GradientDynamicNode : ImageNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput Output;

        GradientMapProcessor processor;

        protected bool horizontal;
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
            Name = "Gradient Dynamic";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;

            processor = new GradientMapProcessor();

            internalPixelType = p;

            horizontal = true;

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

        private void GetParams()
        {
            if (!input.HasInput) return;

            horiz = horizontal;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Horizontal"))
            {
                horiz = ParentGraph.GetParameterValue(Id, "Horizontal").ToBool();
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        bool horiz;
        void Process()
        {
            if (processor == null) return;
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

            processor.PrepareView(buffer);

            processor.Tiling = new Vector2(TileX, TileY);

            processor.Horizontal = horiz;

            processor.ColorLUT = i2;
            processor.Mask = i3;
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class GradientMapData : NodeData
        {
            public bool horizontal;
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
