﻿using System;
using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging;
using Materia.Rendering.Interfaces;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class PixelProcessorNode : ImageNode
    {
        protected NodeOutput output;

        protected FloatBitmap bmp;

        protected PixelShaderProcessor processor;

        protected Function function;
        public Function Function
        {
            get
            {
                return function;
            }
        }

        //hide TileX / TileY from UI
        public new float TileX { get => tileX; set => tileX = value; }
        public new float TileY { get => tileY; set => tileY = value; }

        bool isRebuildRequired = false;

        public PixelProcessorNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Pixel Processor";

            width = w;
            height = h;

            function = new Function("Pixel Processor Function", w, h);
            function.AssignParentNode(this);

            function.ExpectedOutput = NodeType.Float4 | NodeType.Float;

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
            if (isDisposing) return;
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
            if (isDisposing) return;
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
            if (isDisposing) return;
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
            if (isDisposing) return;

            GLTexture2D i1 = null;
            GLTexture2D i2 = null;
            GLTexture2D i3 = null;
            GLTexture2D i4 = null;

            if(Inputs[0].HasInput)
            {
                i1 = (GLTexture2D)Inputs[0].Reference.Data;
            }

            if (Inputs[1].HasInput)
            {
                i2 = (GLTexture2D)Inputs[1].Reference.Data;
            }

            if(Inputs[2].HasInput)
            {
                i3 = (GLTexture2D)Inputs[2].Reference.Data;
            }

            if(Inputs[3].HasInput)
            {
                i4 = (GLTexture2D)Inputs[3].Reference.Data;
            }

            if (!shaderBuilt || function == null)
            {
                return;
            }

            CreateBufferIfNeeded();


            processor ??= new PixelShaderProcessor();

            processor.Shader = function.Shader;

            //prepare uniforms first here
            function.PrepareUniforms();

            //prepare outgoing texture view
            processor.PrepareView(buffer);

            //prepare textures and bind shader
            processor.Prepare(i1, i2, i3, i4);

            //assign uniforms to shader
            function.AssignUniforms();
            
            //process & complete
            processor.Process();
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class PixelProcessorData : NodeData
        {
            public string functionGraph;
        }

        public override void GetBinary(Writer w)
        {
            base.GetBinary(w);
            function.GetBinary(w);
        }

        public override void FromBinary(Reader r)
        {
            base.FromBinary(r);

            function = new Function("Pixel Processor Function");
            function.ExpectedOutput = NodeType.Float4 | NodeType.Float;
            function.AssignParentNode(this);
            function.FromBinary(r);
            function.SetConnections();
        }

        public override void FromJson(string data)
        {
            PixelProcessorData d = JsonConvert.DeserializeObject<PixelProcessorData>(data);
            SetBaseNodeDate(d);

            function = new Function("Pixel Processor Function");
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

            processor?.Dispose();
            processor = null;

            function?.Dispose();
            function = null;
        }
    }
}
