using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging.GLProcessing;
using Materia.Textures;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;
using Materia.Nodes.Helpers;
using Materia.GLInterfaces;

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

        protected float distance;
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

        protected bool sourceOnly;
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

        
        //we override as the distance node
        //does not allow changing internal pixel format
        //since it requires RGBA32F no matter what to function
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

        public DistanceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Distance";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;
            distance = 0.2f;

            previewProcessor = new BasicImageRenderer();
            processor = new DistanceProcessor();

            //distance node requires RGBA32F to compute properly
            internalPixelType = GraphPixelType.RGBA32F;

            input = new NodeInput(NodeType.Gray, this, "Mask");
            input2 = new NodeInput(NodeType.Gray | NodeType.Color, this, "Source");
            Output = new NodeOutput(NodeType.Gray, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Outputs.Add(Output);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (processor != null)
            {
                processor.Release();
                processor = null;
            }

            if(shader != null)
            {
                shader.Release();
                shader = null;
            }

            if(preshader != null)
            {
                preshader.Release();
                preshader = null;
            }
        }
        
        void GetParams()
        {
            if (!input.HasInput) return;

            pmaxDistance = distance;
            psourceonly = sourceOnly;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "MaxDistance"))
            {
                pmaxDistance = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "MaxDistance"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "SourceOnly"))
            {
                psourceonly = Utils.ConvertToBool(ParentGraph.GetParameterValue(Id, "SourceOnly"));
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            BuildShader();
            Process();
        }

        bool rebuild = true;
        void BuildShader()
        {
            if (shader == null || preshader == null || rebuild)
            {
                if(shader != null)
                {
                    shader.Release();
                    shader = null;
                }

                if(preshader != null)
                {
                    preshader.Release();
                    preshader = null;
                }

                string rawFrag = Material.Material.GetRawFrag("distance.glsl");

                if (string.IsNullOrEmpty(rawFrag)) return;

                string outputType = "rgba32f";
                rawFrag = rawFrag.Replace("{0}", outputType);

                shader = Material.Material.CompileCompute(rawFrag);

                if (shader == null) return;

                rawFrag = Material.Material.GetRawFrag("distanceprecalc.glsl");

                if (string.IsNullOrEmpty(rawFrag)) return;
                rawFrag = rawFrag.Replace("{0}", outputType);
                preshader = Material.Material.CompileCompute(rawFrag);

                if (preshader == null) return;

                rebuild = false;
            }
        }

        protected override void OnPixelFormatChange()
        {
            
        }

        public override void AssignPixelType(GraphPixelType pix)
        {
            
        }

        float pmaxDistance;
        bool psourceonly;
        void Process()
        {
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;
            GLTextuer2D i2 = null;

            if(input2.HasInput)
            {
                i2 = (GLTextuer2D)input2.Reference.Data;
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (processor == null) return;
            if (shader == null) return;
            if (preshader == null) return;

            CreateBufferIfNeeded();

            buffer.Bind();
            IGL.Primary.ClearTexImage(buffer.Id, (int)PixelFormat.Rgba, (int)PixelType.Float);
            GLTextuer2D.Unbind();

            processor.TileX = tileX;
            processor.TileY = tileY;

            processor.Shader = shader;
            processor.PreShader = preshader;
            processor.SourceOnly = psourceonly;
            processor.Distance = pmaxDistance;
            processor.Process(width, height, i1, i2, buffer);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class DistanceNodeData : NodeData
        {
            public float maxDistance;
            public bool sourceOnly;
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
