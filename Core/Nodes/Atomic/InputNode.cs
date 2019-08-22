using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;
using Materia.Imaging.GLProcessing;
using Materia.Textures;

namespace Materia.Nodes.Atomic
{
    public class InputNode : ImageNode
    {
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
            for(int i = 0; i < Inputs.Count; i++)
            {
                NodeInput o = Inputs[i];
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
            if(!Async)
            {
                if (Input != null && Input.HasInput)
                {
                    Process();
                }

                return;
            }

            if (Input != null && Input.HasInput)
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

            })
            .ContinueWith(t =>
            {
                if (Input != null && Input.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)Input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            width = i1.Width;
            height = i1.Height;

            if (previewProcessor == null) return;

            previewProcessor.Process(i1.Width, i1.Height, i1, buffer);
            previewProcessor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public override void FromJson(string data)
        {
            NodeData d = JsonConvert.DeserializeObject<NodeData>(data);
            SetBaseNodeDate(d);

            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs.Clear();
            Outputs.Add(Output);
        }

        public override string GetJson()
        {
            NodeData d = new NodeData();
            FillBaseNodeData(d);

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            
        }
    }
}
