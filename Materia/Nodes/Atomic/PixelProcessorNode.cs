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

            function.ExpectedOutput = NodeType.Float4;
            function.OnGraphUpdated += Function_OnGraphUpdated;

            previewProcessor = new BasicImageRenderer();
            processor = new PixelShaderProcessor();

            internalPixelType = p;

            Inputs = new List<NodeInput>();

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
            input.OnInputRemoved += Input_OnInputRemoved;
            input.OnInputAdded += Input_OnInputAdded;

            AddedInput(input);
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();

            //if (!HasEmptyInput)
            //{
            //    AddPlaceholderInput();
            //}
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            var noinputs = Inputs.FindAll(m => !m.HasInput);

            if (noinputs != null && noinputs.Count >= 2 && Inputs.Count > 2)
            {
                var inp = noinputs[noinputs.Count - 1];

                inp.OnInputChanged -= Input_OnInputChanged;
                inp.OnInputRemoved -= Input_OnInputRemoved;
                inp.OnInputAdded -= Input_OnInputAdded;

                Inputs.Remove(inp);
                RemovedInput(inp);
            }
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if(function.HasExpectedOutput)
            {
                Process();
            }
        }

        void Process()
        {
            GLTextuer2D i1 = null;
            GLTextuer2D i2 = null;

            if(Inputs[0].HasInput)
            {
                i1 = (GLTextuer2D)Inputs[0].Input.Data;
            }

            if (Inputs[1].HasInput)
            {
                i2 = (GLTextuer2D)Inputs[1].Input.Data;
            }

            if (!function.BuildShader()) return;

            CreateBufferIfNeeded();

            processor.Shader = function.Shader;
            processor.Process(width, height, i1, i2, buffer);
            processor.Complete();

            output.Data = buffer;
            output.Changed();
            Updated();
        }

        public class PixelProcessorData : NodeData
        {
            public string functionGraph;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            PixelProcessorData d = JsonConvert.DeserializeObject<PixelProcessorData>(data);
            SetBaseNodeDate(d);

            if(function != null)
            {
                function.OnGraphUpdated -= Function_OnGraphUpdated;
            }

            function = new FunctionGraph("Pixel Processor Function");
            function.ExpectedOutput = NodeType.Float4;
            function.OnGraphUpdated += Function_OnGraphUpdated;
            function.FromJson(d.functionGraph);
            function.ParentNode = this;

            SetConnections(nodes, d.outputs);

            TryAndProcess();
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
