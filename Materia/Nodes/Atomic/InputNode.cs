using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;
using Materia.Imaging.GLProcessing;
using Materia.Textures;

namespace Materia.Nodes.Atomic
{
    public class InputNode : ImageNode
    {
        [HideProperty]
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

        [HideProperty]
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

        [HideProperty]
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

        [HideProperty]
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

        NodeOutput Output;
        NodeInput Input;

        public InputNode(GraphPixelType p = GraphPixelType.RGBA)
        {
            Id = Guid.NewGuid().ToString();

            Name = "Input";

            internalPixelType = p;

            //this actually does nothing for this node
            width = 16;
            height = 16;
            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();

            //only an output is present
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            //no actual inputs are present
            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        public void SetInput(NodeInput input)
        {
            foreach(NodeInput o in Inputs)
            {
                o.OnInputRemoved -= Input_OnInputRemoved;
                o.OnInputChanged -= Input_OnInputChanged;
                o.OnInputAdded -= Input_OnInputAdded;
            }

            Inputs.Clear();

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Input = input;

            Inputs.Add(input);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
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
            if(Input != null && Input.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)Input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            width = i1.Width;
            height = i1.Height;

            previewProcessor.Process(i1.Width, i1.Height, i1, buffer);
            previewProcessor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public class InputNodeData : NodeData
        {
            
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            InputNodeData d = JsonConvert.DeserializeObject<InputNodeData>(data);
            SetBaseNodeDate(d);

            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs.Clear();
            Outputs.Add(Output);

            SetConnections(nodes, d.outputs);

            TryAndProcess();
        }

        public override string GetJson()
        {
            InputNodeData d = new InputNodeData();
            FillBaseNodeData(d);

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            
        }
    }
}
