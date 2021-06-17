using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class SwitchNode : ImageNode
    {
        protected NodeInput input;
        protected NodeInput input2;

        NodeOutput Output;

        protected SwitchInput selected = SwitchInput.Input0;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.Dropdown, "Selected")]
        public SwitchInput Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                TriggerValueChange();
            }
        }

        /// <summary>
        /// Replacing with new
        /// to remove attributes
        /// and stay hidden in editor
        /// as these are not really
        /// used for this node
        /// </summary>

        public new bool AbsoluteSize { get; set; }

        public new GraphPixelType InternalPixelFormat
        {
            get
            {
                return internalPixelType;
            }
            set
            {
                internalPixelType = value;
                OnPixelFormatChange();
            }
        }

        public new float TileY
        {
            get
            {
                return tileY;
            }
            set
            {
                tileY = value;
            }
        }

        public new float TileX
        {
            get
            {
                return tileX;
            }
            set
            {
                tileX = value;
            }
        }

        public new int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        public new int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public SwitchNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Switch";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Input 0");
            input2 = new NodeInput(NodeType.Gray | NodeType.Color, this, "Input 1");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Outputs.Add(Output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        SwitchInput pinput;
        void Process()
        {
            if (isDisposing) return;
            pinput = (SwitchInput)GetParameter("Selected", (int)selected);

            GLTexture2D buff = GetActiveBuffer();

            if (buff == null || buff.Id == 0) return;

            Output.Data = buff;
            TriggerTextureChange();
        }

        public override byte[] Export(int w = 0, int h = 0)
        {
            switch (pinput)
            {
                case SwitchInput.Input1:
                    return input2.HasInput ? input2.Reference.Export(w, h) : null;
                case SwitchInput.Input0:
                default:
                    return input.HasInput ? input.Reference.Export(w, h) : null;
            }
        }

        public override GLTexture2D GetActiveBuffer()
        {
            switch(pinput)
            {
                case SwitchInput.Input1:
                    return input2.HasInput ? (GLTexture2D)input2.Reference.Data : null;
                case SwitchInput.Input0:
                default:
                    return input.HasInput ? (GLTexture2D)input.Reference.Data : null;
            }
        }

        public class SwitchData : NodeData
        {
            public int selected;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(selected);
            }
            public override void Parse(Reader r)
            {
                base.Parse(r);
                selected = r.NextInt();
            }
        }

        public override void GetBinary(Writer w)
        {
            SwitchData d = new SwitchData();
            FillBaseNodeData(d);
            d.selected = (int)selected;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            SwitchData d = new SwitchData();
            d.Parse(r);
            SetBaseNodeDate(d);
            selected = (SwitchInput)d.selected;
        }

        public override string GetJson()
        {
            SwitchData d = new SwitchData();
            FillBaseNodeData(d);
            d.selected = (int)selected;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            SwitchData d = JsonConvert.DeserializeObject<SwitchData>(data);
            SetBaseNodeDate(d);
            selected = (SwitchInput)d.selected;
        }
    }
}
