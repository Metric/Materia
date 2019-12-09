using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Attributes;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;
using Materia.Nodes.Helpers;

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

            previewProcessor = new BasicImageRenderer();

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
                pinput = (SwitchInput)Utils.ConvertToInt(ParentGraph.GetParameterValue(Id, "Selected"));
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
            GLTextuer2D buff = GetActiveBuffer();

            if (buff == null || buff.Id == 0) return;

            Output.Data = buff;
            TriggerTextureChange();
        }

        public override byte[] GetPreview(int width, int height)
        {
            GLTextuer2D active = GetActiveBuffer();

            if (active == null) return null;
            if (active.Id == 0) return null;

            previewProcessor.Process(width, height, active);
            byte[] bits = previewProcessor.ReadByte(width, height);
            previewProcessor.Complete();
            return bits;
        }

        public override GLTextuer2D GetActiveBuffer()
        {
            switch(pinput)
            {
                case SwitchInput.Input1:
                    return input2.HasInput ? (GLTextuer2D)input2.Reference.Data : null;
                case SwitchInput.Input0:
                default:
                    return input.HasInput ? (GLTextuer2D)input.Reference.Data : null;
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
