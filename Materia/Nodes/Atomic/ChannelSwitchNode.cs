using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Nodes.Attributes;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class ChannelSwitchNode : ImageNode
    {
        protected ChannelSwitchProcessor processor;

        protected NodeInput input;
        protected NodeInput input2;
        protected NodeOutput output;

        protected int redChannel;

        [Title(Title = "Red Channel")]
        [Dropdown(null, "Input0 Red", "Input0 Green", "Input0 Blue", "Input0 Alpha", "Input1 Red", "Input1 Green", "Input1 Blue", "Input1 Alpha")]
        public int RedChannel
        {
            get
            {
                return redChannel;
            }
            set
            {
                redChannel = value;
                TryAndProcess();
            }
        }

        protected int greenChannel;

        [Title(Title = "Green Channel")]
        [Dropdown(null, "Input0 Red", "Input0 Green", "Input0 Blue", "Input0 Alpha", "Input1 Red", "Input1 Green", "Input1 Blue", "Input1 Alpha")]
        public int GreenChannel
        {
            get
            {
                return greenChannel;
            }
            set
            {
                greenChannel = value;
                TryAndProcess();
            }
        }

        protected int blueChannel;

        [Title(Title = "Blue Channel")]
        [Dropdown(null, "Input0 Red", "Input0 Green", "Input0 Blue", "Input0 Alpha", "Input1 Red", "Input1 Green", "Input1 Blue", "Input1 Alpha")]
        public int BlueChannel
        {
            get
            {
                return blueChannel;
            }
            set
            {
                blueChannel = value;
                TryAndProcess();
            }
        }

        protected int alphaChannel;

        [Title(Title = "Alpha Channel")]
        [Dropdown(null, "Input0 Red", "Input0 Green", "Input0 Blue", "Input0 Alpha", "Input1 Red", "Input1 Green", "Input1 Blue", "Input1 Alpha")]
        public int AlphaChannel
        {
            get
            {
                return alphaChannel;
            }
            set
            {
                alphaChannel = value;
                TryAndProcess();
            }
        }

        public ChannelSwitchNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
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

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;
            input2.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Outputs = new List<NodeOutput>();

            Inputs.Add(input);
            Inputs.Add(input2);

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
            if(input.HasInput && input2.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;
            GLTextuer2D i2 = (GLTextuer2D)input2.Input.Data;

            if (i1 == null || i1.Id == 0) return;
            if (i2 == null || i2.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = TileY;

            processor.RedChannel = redChannel;
            processor.GreenChannel = greenChannel;
            processor.BlueChannel = blueChannel;
            processor.AlphaChannel = alphaChannel;
            processor.Process(width, height, i1, i2, buffer);
            processor.Complete();

            output.Data = buffer;
            output.Changed();
            Updated();
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

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            ChannelSwitchData d = JsonConvert.DeserializeObject<ChannelSwitchData>(data);
            SetBaseNodeDate(d);
            redChannel = d.red;
            greenChannel = d.green;
            blueChannel = d.blue;
            alphaChannel = d.alpha;

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
