using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;
using Materia.Imaging.GLProcessing;
using Materia.Imaging;
using Materia.Textures;
using Materia.MathHelpers;
using System.Threading;

namespace Materia.Nodes.Atomic
{
    public class PixelProcessorNode : ImageNode
    {
        CancellationTokenSource ctk;

        protected NodeOutput output;

        protected FloatBitmap bmp;

        protected PixelShaderProcessor processor;

        protected FunctionGraph function;
        public FunctionGraph Function
        {
            get
            {
                return function;
            }
        }

        public PixelProcessorNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Pixel Processor";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            tileX = tileY = 1;

            function = new FunctionGraph("Pixel Processor Function");
            function.ParentNode = this;

            function.ExpectedOutput = NodeType.Float4 | NodeType.Float;
            function.OnGraphUpdated += Function_OnGraphUpdated;

            previewProcessor = new BasicImageRenderer();
            processor = new PixelShaderProcessor();

            internalPixelType = p;

            Inputs = new List<NodeInput>();

            AddPlaceholderInput();
            AddPlaceholderInput();
            AddPlaceholderInput();
            AddPlaceholderInput();

            Outputs = new List<NodeOutput>();
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs.Add(output);
        }

        private void Function_OnGraphUpdated(Graph g)
        {
            TryAndProcess();
        }

        protected override void AddPlaceholderInput()
        {
            var input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Input " + Inputs.Count);
            Inputs.Add(input);

            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputAdded += Input_OnInputAdded;

            AddedInput(input);
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if(!Async)
            {
                if (function.HasExpectedOutput)
                {
                    Process();
                }

                return;
            }

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(100, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                RunInContext(() =>
                {
                    if (function.HasExpectedOutput)
                    {
                        Process();
                    }
                });
            });
        }

        void Process()
        {
            GLTextuer2D i1 = null;
            GLTextuer2D i2 = null;
            GLTextuer2D i3 = null;
            GLTextuer2D i4 = null;

            if(Inputs[0].HasInput)
            {
                i1 = (GLTextuer2D)Inputs[0].Input.Data;
            }

            if (Inputs[1].HasInput)
            {
                i2 = (GLTextuer2D)Inputs[1].Input.Data;
            }

            if(Inputs[2].HasInput)
            {
                i3 = (GLTextuer2D)Inputs[2].Input.Data;
            }

            if(Inputs[3].HasInput)
            {
                i4 = (GLTextuer2D)Inputs[3].Input.Data;
            }

            if (!function.BuildShader())
            {
                return;
            }

            CreateBufferIfNeeded();

            processor.Shader = function.Shader;
            processor.Process(width, height, i1, i2, i3, i4, buffer);
            processor.Complete();

            output.Data = buffer;
            output.Changed();
            Updated();
        }

        public class PixelProcessorData : NodeData
        {
            public string functionGraph;
        }

        public override void FromJson(string data)
        {
            PixelProcessorData d = JsonConvert.DeserializeObject<PixelProcessorData>(data);
            SetBaseNodeDate(d);

            if(function != null)
            {
                function.OnGraphUpdated -= Function_OnGraphUpdated;
            }

            function = new FunctionGraph("Pixel Processor Function");
            function.ExpectedOutput = NodeType.Float4 | NodeType.Float;
            function.OnGraphUpdated += Function_OnGraphUpdated;
            function.FromJson(d.functionGraph);
            function.ParentNode = this;
            function.SetConnections();
        }

        public override string GetJson()
        {
            PixelProcessorData d = new PixelProcessorData();
            FillBaseNodeData(d);
            d.functionGraph = function.GetJson();

            return JsonConvert.SerializeObject(d);
        }

        public override void Dispose()
        {
            base.Dispose();

            if(function != null)
            {
                function.Dispose();
                function = null;
            }
        }
    }
}
