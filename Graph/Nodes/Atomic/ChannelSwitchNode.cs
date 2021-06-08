using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Attributes;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Atomic
{
    public class ChannelSwitchNode : ImageNode
    {
        protected ChannelSwitchProcessor processor;

        protected NodeInput input;
        protected NodeInput input2;
        protected NodeOutput output;

        protected int redChannel;

        [Promote(NodeType.Float)]
        [Dropdown(null, false, "Input0 Red", "Input0 Green", "Input0 Blue", "Input0 Alpha", "Input1 Red", "Input1 Green", "Input1 Blue", "Input1 Alpha")]
        [Editable(ParameterInputType.Dropdown, "Red Channel")]
        public int RedChannel
        {
            get
            {
                return redChannel;
            }
            set
            {
                redChannel = value;
                TriggerValueChange();
            }
        }

        protected int greenChannel;

        [Promote(NodeType.Float)]
        [Dropdown(null, false, "Input0 Red", "Input0 Green", "Input0 Blue", "Input0 Alpha", "Input1 Red", "Input1 Green", "Input1 Blue", "Input1 Alpha")]
        [Editable(ParameterInputType.Dropdown, "Green Channel")]
        public int GreenChannel
        {
            get
            {
                return greenChannel;
            }
            set
            {
                greenChannel = value;
                TriggerValueChange();
            }
        }

        protected int blueChannel;

        [Promote(NodeType.Float)]
        [Dropdown(null, false, "Input0 Red", "Input0 Green", "Input0 Blue", "Input0 Alpha", "Input1 Red", "Input1 Green", "Input1 Blue", "Input1 Alpha")]
        [Editable(ParameterInputType.Dropdown, "Blue Channel")]
        public int BlueChannel
        {
            get
            {
                return blueChannel;
            }
            set
            {
                blueChannel = value;
                TriggerValueChange();
            }
        }

        protected int alphaChannel;

        [Promote(NodeType.Float)]
        [Dropdown(null, false, "Input0 Red", "Input0 Green", "Input0 Blue", "Input0 Alpha", "Input1 Red", "Input1 Green", "Input1 Blue", "Input1 Alpha")]
        [Editable(ParameterInputType.Dropdown, "Alpha Channel")]
        public int AlphaChannel
        {
            get
            {
                return alphaChannel;
            }
            set
            {
                alphaChannel = value;
                TriggerValueChange();
            }
        }

        public ChannelSwitchNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Channel Switch";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            redChannel = 0;
            greenChannel = 1;
            blueChannel = 2;
            alphaChannel = 3;

            tileX = tileY = 1;

            processor = new ChannelSwitchProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Input 0");
            input2 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Input 1");
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Outputs.Add(output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (processor == null) return;
            if (!input.HasInput || !input2.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;
            GLTexture2D i2 = (GLTexture2D)input2.Reference.Data;

            if (i1 == null || i1.Id == 0) return;
            if (i2 == null || i2.Id == 0) return;

            CreateBufferIfNeeded();

            processor.Tiling = GetTiling();
            processor.RedChannel = GetParameter("RedChannel", redChannel);
            processor.GreenChannel = GetParameter("GreenChannel", greenChannel);
            processor.BlueChannel = GetParameter("BlueChannel", blueChannel);
            processor.AlphaChannel = GetParameter("AlphaChannel", alphaChannel);

            processor.PrepareView(buffer);
            processor.Process(i1, i2);
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class ChannelSwitchData : NodeData
        {
            public int red;
            public int green;
            public int blue;
            public int alpha;
        }

        public override string GetJson()
        {
            ChannelSwitchData d = new ChannelSwitchData();
            FillBaseNodeData(d);
            d.red = redChannel;
            d.green = greenChannel;
            d.blue = blueChannel;
            d.alpha = alphaChannel;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            ChannelSwitchData d = JsonConvert.DeserializeObject<ChannelSwitchData>(data);
            SetBaseNodeDate(d);
            redChannel = d.red;
            greenChannel = d.green;
            blueChannel = d.blue;
            alphaChannel = d.alpha;
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }
    }
}
