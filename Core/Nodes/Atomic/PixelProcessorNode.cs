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
using Materia.GLInterfaces;

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

        bool isRebuildRequired = false;

        public PixelProcessorNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Pixel Processor";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            tileX = tileY = 1;

            function = new FunctionGraph("Pixel Processor Function", w, h);
            function.AssignParentNode(this);

            function.ExpectedOutput = NodeType.Float4 | NodeType.Float;

            previewProcessor = new BasicImageRenderer();
            processor = new PixelShaderProcessor();

            internalPixelType = p;

            for(int i = 0; i < 4; ++i)
            {
                var input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Input " + Inputs.Count);
                Inputs.Add(input);
            }

            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs.Add(output);
        }

        public override void TryAndProcess()
        {
            if (function != null && (function.Shader == null || function.Modified || isRebuildRequired))
            {
                Prepare();
                BuildShader();
            }
            else
            {
                shaderBuilt = true;
            }

            Process();
        }

        private void Prepare()
        {
            if(function != null)
            {
                function.PrepareShader(internalPixelType, false);
            }
        }

        protected override void OnPixelFormatChange()
        {
            base.OnPixelFormatChange();
            isRebuildRequired = true;
        }

        public override void AssignPixelType(GraphPixelType pix)
        {
            base.AssignPixelType(pix);
            isRebuildRequired = true;
        }

        private void BuildShader()
        {
            if (function != null)
            {
                shaderBuilt = function.BuildShader();
                if(shaderBuilt)
                {
                    isRebuildRequired = false;
                }
            }
            else
            {
                shaderBuilt = false;
            }
        }

        bool shaderBuilt;
        void Process()
        {
            GLTextuer2D i1 = null;
            GLTextuer2D i2 = null;
            GLTextuer2D i3 = null;
            GLTextuer2D i4 = null;

            if(Inputs[0].HasInput)
            {
                i1 = (GLTextuer2D)Inputs[0].Reference.Data;
            }

            if (Inputs[1].HasInput)
            {
                i2 = (GLTextuer2D)Inputs[1].Reference.Data;
            }

            if(Inputs[2].HasInput)
            {
                i3 = (GLTextuer2D)Inputs[2].Reference.Data;
            }

            if(Inputs[3].HasInput)
            {
                i4 = (GLTextuer2D)Inputs[3].Reference.Data;
            }

            if (!shaderBuilt || function == null)
            {
                return;
            }

            CreateBufferIfNeeded();

            buffer.Bind();

            IGL.Primary.ClearTexImage(buffer.Id, (int)PixelFormat.Rgba, (int)PixelType.Float);

            GLTextuer2D.Unbind();      
            processor.Shader = function.Shader;
            processor.Process(function, width, height, i1, i2, i3, i4, buffer);
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class PixelProcessorData : NodeData
        {
            public string functionGraph;
        }

        public override void FromJson(string data)
        {
            PixelProcessorData d = JsonConvert.DeserializeObject<PixelProcessorData>(data);
            SetBaseNodeDate(d);

            function = new FunctionGraph("Pixel Processor Function");
            function.ExpectedOutput = NodeType.Float4 | NodeType.Float;
            function.AssignParentNode(this);
            function.FromJson(d.functionGraph);
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
