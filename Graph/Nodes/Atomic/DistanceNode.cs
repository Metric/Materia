using System;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Extensions;
using Materia.Rendering.Shaders;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class DistanceNode : ImageNode
    {
        IGLProgram shader;
        IGLProgram preshader;
        NodeInput input;
        NodeInput input2;
        NodeOutput Output;

        DistanceProcessor processor;

        protected float distance = 0.2f;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Max Distance", "Default")]
        public float MaxDistance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = value;
                TriggerValueChange();
            }
        }

        protected bool sourceOnly = false;
        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Source Only")]
        public bool SourceOnly
        {
            get
            {
                return sourceOnly;
            }
            set
            {
                sourceOnly = value;
                TriggerValueChange();
            }
        }

        public DistanceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Distance";

            width = w;
            height = h;

            internalPixelType = GraphPixelType.RGBA32F;

            input = new NodeInput(NodeType.Gray, this, "Mask");
            input2 = new NodeInput(NodeType.Gray | NodeType.Color, this, "Source");
            Output = new NodeOutput(NodeType.Gray, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Outputs.Add(Output);

            input.OnInputChanged += Input_OnInputChanged;
            input2.OnInputChanged += Input_OnInputChanged;
        }


        private void Input_OnInputChanged(NodeInput n)
        {
            rebuild = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;


            shader?.Dispose();
            shader = null;


            preshader?.Dispose();
            preshader = null;
        }

        public override void TryAndProcess()
        {
            BuildShader();
            Process();
        }

        private string GetFormat(GLTexture2D t)
        {
            string outputType = "rgba32f";

            if (t == null) return outputType;

            var format = t.InternalFormat;
            if (format == PixelInternalFormat.Rgba16f || format == PixelInternalFormat.Rgb16f)
            {
                outputType = "rgba16f";
            }
            else if (format == PixelInternalFormat.Rgb || format == PixelInternalFormat.Rgba
                || format == PixelInternalFormat.Rgb8 || format == PixelInternalFormat.Rgba8)
            {
                outputType = "rgba8";
            }
            else if (format == PixelInternalFormat.R32f)
            {
                outputType = "r32f";
            }
            else if (format == PixelInternalFormat.R16f)
            {
                outputType = "r16f";
            }
            return outputType;
        }

        bool rebuild = true;
        void BuildShader()
        {
            if (isDisposing) return;
            if (!input.HasInput) return;

            CreateBufferIfNeeded();

            if (shader == null || preshader == null || rebuild)
            {
                shader?.Dispose();
                shader = null;

                preshader?.Dispose();
                preshader = null;

                string rawFrag = GLShaderCache.GetRawFrag("distance.glsl");

                if (string.IsNullOrEmpty(rawFrag)) return;

                string sourceType = GetFormat(input2.HasInput ? input2.Reference.Data as GLTexture2D : buffer);

                rawFrag = rawFrag.Replace("{0}", sourceType);

                shader = GLShaderCache.CompileCompute(rawFrag);

                if (shader == null) return;

                string outputType = GetFormat(buffer);
                string inputType = GetFormat(input.Reference.Data as GLTexture2D);

                rawFrag = GLShaderCache.GetRawFrag("distanceprecalc.glsl");

                if (string.IsNullOrEmpty(rawFrag)) return;
                rawFrag = rawFrag.Replace("{0}", inputType).Replace("{1}", sourceType).Replace("{2}", outputType);
                preshader = GLShaderCache.CompileCompute(rawFrag);

                if (preshader == null) return;

                rebuild = false;
            }
        }

        protected override void OnPixelFormatChange()
        {
            base.OnPixelFormatChange();
            rebuild = true;
        }

        public override void AssignPixelType(GraphPixelType pix)
        {
            base.AssignPixelType(pix);
            rebuild = true;
        }

        void Process()
        {
            if (isDisposing) return;
            if (!input.HasInput) return;
            if (shader == null) return;
            if (preshader == null) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;
            GLTexture2D i2 = null;

            if(input2.HasInput)
            {
                i2 = (GLTexture2D)input2.Reference.Data;
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;

            processor ??= new DistanceProcessor();

            processor.Tiling = GetTiling();
            processor.Shader = shader;
            processor.PreShader = preshader;
            processor.SourceOnly = GetParameter("SourceOnly", sourceOnly);
            processor.Distance = GetParameter("MaxDistance", distance);

            processor.PrepareView(buffer);
            processor.Process(i1, i2);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class DistanceNodeData : NodeData
        {
            public float maxDistance;
            public bool sourceOnly;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(maxDistance);
                w.Write(sourceOnly);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                maxDistance = r.NextFloat();
                sourceOnly = r.NextBool();
            }
        }

        public override void GetBinary(Writer w)
        {
            DistanceNodeData d = new DistanceNodeData();
            FillBaseNodeData(d);
            d.maxDistance = distance;
            d.sourceOnly = sourceOnly;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            DistanceNodeData d = new DistanceNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            distance = d.maxDistance;
            sourceOnly = d.sourceOnly;
        }

        public override string GetJson()
        {
            DistanceNodeData d = new DistanceNodeData();
            FillBaseNodeData(d);
            d.maxDistance = distance;
            d.sourceOnly = sourceOnly;
            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            DistanceNodeData d = JsonConvert.DeserializeObject<DistanceNodeData>(data);
            SetBaseNodeDate(d);
            internalPixelType = GraphPixelType.RGBA32F;
            distance = d.maxDistance;
            sourceOnly = d.sourceOnly;
        }
    }
}
