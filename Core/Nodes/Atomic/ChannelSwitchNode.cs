using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Nodes.Attributes;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;
using Materia.Nodes.Helpers;

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
            previewProcessor = new BasicImageRenderer();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Input 0");
            input2 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Input 1");
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Outputs.Add(output);
        }

        private void GetParams()
        {
            if (!input.HasInput || !input2.HasInput) return;

            predChannel = redChannel;
            pgreenChannel = greenChannel;
            pblueChannel = blueChannel;
            palphaChannel = alphaChannel;

            if (ParentGraph != null)
            {
                if (ParentGraph.HasParameterValue(Id, "RedChannel"))
                {
                    predChannel = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "RedChannel"));
                }

                if (ParentGraph.HasParameterValue(Id, "GreenChannel"))
                {
                    pgreenChannel = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "GreenChannel"));
                }

                if (ParentGraph.HasParameterValue(Id, "BlueChannel"))
                {
                    pblueChannel = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "BlueChannel"));
                }

                if (ParentGraph.HasParameterValue(Id, "AlphaChannel"))
                {
                    palphaChannel = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "AlphaChannel"));
                }
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float predChannel;
        float pgreenChannel;
        float pblueChannel;
        float palphaChannel;
        void Process()
        {
            if (!input.HasInput || !input2.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;
            GLTextuer2D i2 = (GLTextuer2D)input2.Reference.Data;

            if (i1 == null || i1.Id == 0) return;
            if (i2 == null || i2.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = TileY;

            processor.RedChannel = (int)predChannel;
            processor.GreenChannel = (int)pgreenChannel;
            processor.BlueChannel = (int)pblueChannel;
            processor.AlphaChannel = (int)palphaChannel;
            processor.Process(width, height, i1, i2, buffer);
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

            if(processor != null)
            {
                processor.Release();
                processor = null;
            }
        }
    }
}
