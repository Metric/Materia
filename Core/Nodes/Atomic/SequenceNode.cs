using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;
using Materia.Textures;
using System.Threading;

namespace Materia.Nodes.Atomic
{ 
    public class SequenceNode : ImageNode
    {
        public new bool AbsoluteSize { get; set; }

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

        public new GraphPixelType InternalPixelFormat
        {
            get
            {
                return internalPixelType;
            }
            set
            {
                internalPixelType = value;
            }
        }

        NodeInput input;

        public SequenceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Sequence";
            Id = Guid.NewGuid().ToString();

            input = new NodeInput(NodeType.Bool | NodeType.Color | NodeType.Gray | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Input");

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            Outputs = new List<NodeOutput>();

            for(int i = 0; i < 2; i++)
            {
                AddPlaceholderOutput();
            }
        }

        protected override void AddPlaceholderOutput()
        {
            var output = new NodeOutput(NodeType.Bool | NodeType.Color | NodeType.Gray | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, String.Format("{0:0}", Outputs.Count));
            output.OnInputAdded += Output_OnInputAdded;
            output.OnInputRemoved += Output_OnInputRemoved;

            //go ahead and set the data for the new output
            if(input.HasInput)
            {
                output.Data = input.Input.Data;
            }

            Outputs.Add(output);
            AddedOutput(output);
        }

        private void Output_OnInputRemoved(NodeOutput inp)
        {
            var empties = Outputs.FindAll(m => m.To.Count == 0);

            if (Outputs.Count > 2 && empties != null && empties.Count >= 2)
            {
                var inp2 = empties[empties.Count - 1];
                inp2.OnInputAdded -= Output_OnInputAdded;
                inp2.OnInputRemoved -= Output_OnInputRemoved;

                Outputs.Remove(inp2);
                RemovedOutput(inp2);
            }
        }

        private void Output_OnInputAdded(NodeOutput inp)
        {
            if(!HasEmpytOutput)
            {
                AddPlaceholderOutput();
            } 
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
                if (input.HasInput)
                {
                    Process();
                }

                return;
            }

            if (input.HasInput)
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
                if(input.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        void Process()
        {
            if (input.Input.Data == null) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            width = i1.Width;
            height = i1.Height;

            int c = Outputs.Count;

            for(int i = 0; i < c; i++)
            {
                Outputs[i].Data = i1;
                Outputs[i].Changed();
            }

            Updated();
        }

        public override GLTextuer2D GetActiveBuffer()
        {
            if(input.HasInput)
            {
                return input.Input.Node.GetActiveBuffer();
            }

            return null;
        }

        public override byte[] GetPreview(int width, int height)
        {
            if(input.HasInput)
            {
                return input.Input.Node.GetPreview(width, height);
            }

            return null;
        }

        protected override void OnWidthHeightSet()
        {
            //do nothing
        }

        public override string GetJson()
        {
            NodeData d = new NodeData();
            FillBaseNodeData(d);

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            NodeData d = JsonConvert.DeserializeObject<NodeData>(data);
            SetBaseNodeDate(d);
        }
    }
}
