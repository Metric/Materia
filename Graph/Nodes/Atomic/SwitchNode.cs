using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public enum SwitchInput
    {
        Input0 = 0,
        Input1 = 1
    }

    public class SwitchNode : ImageNode
    {
        protected NodeInput input;
        protected NodeInput input2;

        NodeOutput Output;

        protected SwitchInput selected;
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
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            selected = SwitchInput.Input0;

            tileX = tileY = 1;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Input 0");
            input2 = new NodeInput(NodeType.Gray | NodeType.Color, this, "Input 1");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Outputs.Add(Output);
        }

        private void GetParams()
        {
            if (!input.HasInput) return;

            pinput = selected;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Selected"))
            {
                pinput = (SwitchInput)ParentGraph.GetParameterValue(Id, "Selected").ToInt();
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        SwitchInput pinput;
        void Process()
        {
            GLTexture2D buff = GetActiveBuffer();

            if (buff == null || buff.Id == 0) return;

            Output.Data = buff;
            TriggerTextureChange();
        }

        public override byte[] GetPreview(int width, int height)
        {
            GLTexture2D active = GetActiveBuffer();

            if (active == null) return null;
            if (active.Id == 0) return null;

            //todo: correct this GetPreview 
            //we may or may not need it anymore

            return null;
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

        public class NormalData : NodeData
        {
            public int selected;
        }

        public override string GetJson()
        {
            NormalData d = new NormalData();
            FillBaseNodeData(d);
            d.selected = (int)selected;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            NormalData d = JsonConvert.DeserializeObject<NormalData>(data);
            SetBaseNodeDate(d);
            selected = (SwitchInput)d.selected;
        }
    }
}
