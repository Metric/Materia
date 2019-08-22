using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes;
using Materia.GLInterfaces;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using System.Threading;
using Materia.Imaging;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class GradientDynamicNode : ImageNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput Output;

        GradientMapProcessor processor;

        protected bool horizontal;
        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Horizontal")]
        public bool Horizontal
        {
            get
            {
                return horizontal;
            }
            set
            {
                horizontal = value;
                TryAndProcess();
            }
        }

        public GradientDynamicNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Gradient Dynamic";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new GradientMapProcessor();

            internalPixelType = p;

            horizontal = true;

            input = new NodeInput(NodeType.Gray, this, "Image Input");
            input2 = new NodeInput(NodeType.Color, this, "Gradient Input");
            input3 = new NodeInput(NodeType.Gray, this, "Mask");
            Output = new NodeOutput(NodeType.Color, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            input3.OnInputAdded += Input_OnInputAdded;
            input3.OnInputChanged += Input_OnInputChanged;
            input3.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            TryAndProcess();
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

        public override void Dispose()
        {
            base.Dispose();

            if (processor != null)
            {
                processor.Release();
                processor = null;
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
                if(input.HasInput && input2.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        private void GetParams()
        {
            horiz = horizontal;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Horizontal"))
            {
                horiz = Convert.ToBoolean(ParentGraph.GetParameterValue(Id, "Horizontal"));
            }
        }

        bool horiz;
        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;
            GLTextuer2D i2 = (GLTextuer2D)input2.Input.Data;
            GLTextuer2D i3 = null;

            if (input3.HasInput)
            {
                if (input3.Input.Data != null)
                {
                    i3 = (GLTextuer2D)input3.Input.Data;
                }
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;
            if (i2 == null) return;
            if (i2.Id == 0) return;

            if (processor == null) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;

            processor.Horizontal = horiz;

            processor.ColorLUT = i2;
            processor.Mask = i3;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Output.Data = buffer;
            Output.Changed();
            Updated();
        }

        public class GradientMapData : NodeData
        {
            public bool horizontal;
        }

        public override void FromJson(string data)
        {
            GradientMapData d = JsonConvert.DeserializeObject<GradientMapData>(data);
            SetBaseNodeDate(d);
            horizontal = d.horizontal;
        }

        public override string GetJson()
        {
            GradientMapData d = new GradientMapData();
            FillBaseNodeData(d);
            d.horizontal = horizontal;

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }
    }
}
