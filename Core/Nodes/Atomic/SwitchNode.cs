using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Attributes;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;

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
                TryAndProcess();
            }
        }

        public SwitchNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
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

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputRemoved += Input_OnInputRemoved;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputRemoved += Input_OnInputRemoved;
            input2.OnInputChanged += Input_OnInputChanged;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);
            Inputs.Add(input2);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if (!Async)
            {
                if (input.HasInput && input2.HasInput)
                {
                    GetParams();
                    Process();
                }

                return;
            }

            if (input.HasInput && input2.HasInput)
            {
                if (ParentGraph != null)
                {
                    ParentGraph.Schedule(this);
                }
            }
        }

        public override Task GetTask()
        {
            return Task.Factory.StartNew(() =>
            {
                GetParams();
            })
            .ContinueWith(t =>
            {
                if (input.HasInput && input2.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        private void GetParams()
        {
            pinput = selected;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Selected"))
            {
                pinput = (SwitchInput)Convert.ToInt32(ParentGraph.GetParameterValue(Id, "Selected"));
            }
        }

        SwitchInput pinput;
        void Process()
        {
            GLTextuer2D buff = GetActiveBuffer();

            if (buff == null || buff.Id == 0) return;

            Updated();
            Output.Data = buff;
            Output.Changed();
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
                    return input2.HasInput ? (GLTextuer2D)input2.Input.Data : null;
                case SwitchInput.Input0:
                default:
                    return input.HasInput ? (GLTextuer2D)input.Input.Data : null;
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
