using Materia.Imaging.GLProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Nodes.Attributes;
using Materia.Textures;
using Newtonsoft.Json;
using Materia.Nodes.Helpers;
using NLog;

namespace Materia.Nodes.Atomic
{
    public enum FXPivot
    {
        Center = 0,
        Min = 1,
        Max = 2,
        MinX = 3,
        MaxX = 4,
        MinY = 5,
        MaxY = 6
    }

    public enum FXBlend
    {
        Blend = 0,
        Add = 1,
        Max = 2
    }

    public class FXNode : ImageNode
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        NodeInput q1;
        NodeInput q2;
        NodeInput q3;
        NodeInput q4;

        NodeOutput Output;

        FXProcessor processor;

        protected int iterations;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntInput, "Iterations")]
        public int Iterations
        {
            get
            {
                return iterations;
            }
            set
            {
                iterations = value;
                TryAndProcess();
            }
        }

        protected MVector translation;
        [Promote(NodeType.Float2)]
        [Editable(ParameterInputType.Float2Input, "Translation", "Transform")]
        public MVector Translation
        {
            get
            {
                return translation;
            }
            set
            {
                translation = value;
                TryAndProcess();
            }
        }

        protected MVector scale;
        [Promote(NodeType.Float2)]
        [Editable(ParameterInputType.Float2Input, "Scale", "Transform")]
        public MVector Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                TryAndProcess();
            }
        }

        protected int rotation;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Rotation", "Transform", 0, 360)]
        public int Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                TryAndProcess();
            }
        }

        protected FXPivot patternPivot;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.Dropdown, "Pattern Pivot", "Transform")]
        public FXPivot PatternPivot
        {
            get
            {
                return patternPivot;
            }
            set
            {
                patternPivot = value;
                TryAndProcess();
            }
        }

        protected float luminosity;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Luminosity", "Effects")]
        public float Luminosity
        {
            get
            {
                return luminosity;
            }
            set
            {
                luminosity = value;
                TryAndProcess();
            }
        }

        protected float luminosityRandomness;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Luminosity Randomness", "Effects")]
        public float LuminosityRandomness
        {
            get
            {
                return luminosityRandomness;
            }
            set
            {
                luminosityRandomness = value;
                TryAndProcess();
            }
        }

        protected FXBlend blending;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.Dropdown, "Blending", "Effects")]
        public FXBlend Blending
        {
            get
            {
                return blending;
            }
            set
            {
                blending = value;
                TryAndProcess();
            }
        }

        /// <summary>
        /// Hiding the tiling
        /// as it does not apply
        /// to the FX node
        /// </summary>
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

        public FXNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "FX";

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            width = w;
            height = h;

            internalPixelType = p;
            luminosity = 1.0f;
            luminosityRandomness = 0.0f;

            iterations = 1;
            translation = new MVector();
            scale = new MVector(1, 1);
            rotation = 0;

            blending = FXBlend.Blend;

            previewProcessor = new BasicImageRenderer();
            processor = new FXProcessor();

            q1 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Quadrant");
            q2 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Quadrant");
            q3 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Quadrant");
            q4 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Quadrant");

            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            q1.OnInputAdded += Input_OnInputAdded;
            q1.OnInputRemoved += Input_OnInputRemoved;
            q1.OnInputChanged += Input_OnInputChanged;

            q2.OnInputAdded += Input_OnInputAdded;
            q2.OnInputRemoved += Input_OnInputRemoved;
            q2.OnInputChanged += Input_OnInputChanged;

            q3.OnInputAdded += Input_OnInputAdded;
            q3.OnInputRemoved += Input_OnInputRemoved;
            q3.OnInputChanged += Input_OnInputChanged;

            q4.OnInputAdded += Input_OnInputAdded;
            q4.OnInputRemoved += Input_OnInputRemoved;
            q4.OnInputChanged += Input_OnInputChanged;

            Inputs = new List<NodeInput>();
            Inputs.Add(q1);
            Inputs.Add(q2);
            Inputs.Add(q3);
            Inputs.Add(q4);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if(!Async)
            {
                if (q1.HasInput || q2.HasInput || q3.HasInput || q4.HasInput)
                {
                    GetParams();
                    CollectQuadData();
                    Process();
                }

                return;
            }

            if (ParentGraph != null)
            {
                if (q1.HasInput || q2.HasInput || q3.HasInput || q4.HasInput)
                {
                    ParentGraph.Schedule(this);
                }
            }
        }

        float CalculateRandomLuminosity(float iter, float randLum)
        {
            MVector m2 = new MVector(randLum + iter + ParentGraph.RandomSeed, randLum + iter + ParentGraph.RandomSeed);
            float f = Utils.Rand(ref m2);
            f = f * randLum;
            return f;
        }

        //special helper struct
        private class FXQuadData
        {
            public FXBlend blending;
            public FXPivot pivot;
            public float luminosity;
            public float angle;
            public MVector translation;
            public MVector scale;

            public int quadrant;

            public FXQuadData(int q, FXBlend blend, FXPivot piv, float lum, float ang, MVector trans, MVector scal)
            {
                quadrant = q;
                blending = blend;
                pivot = piv;
                luminosity = lum;
                angle = ang;
                translation = trans;
                scale = scal;
            }
        }

        void ProcessQuad(FXQuadData data, int quads)
        {
            GLTextuer2D i1 = null;
            if (data.quadrant == 0)
            {
                if (!q1.HasInput) return;
                i1 = (GLTextuer2D)q1.Input.Data;
            }
            else if(data.quadrant == 1)
            {
                if (!q2.HasInput) return;
                i1 = (GLTextuer2D)q2.Input.Data;
            }
            else if(data.quadrant == 2)
            {
                if (!q3.HasInput) return;
                i1 = (GLTextuer2D)q3.Input.Data;
            }
            else if(data.quadrant == 3)
            {
                if (!q4.HasInput) return;
                i1 = (GLTextuer2D)q4.Input.Data;
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;

            processor.Blending = data.blending;
            processor.Scale = data.scale;
            processor.Translation = data.translation;
            processor.Angle = data.angle;
            processor.Pivot = data.pivot;
            processor.Luminosity = data.luminosity;

            processor.Process(data.quadrant, width, height, i1, buffer, quads);
        }

        FXQuadData GetQuad(float i, float x, float y, float imax, int quad)
        {
            MVector pTrans = translation;
            MVector pScale = scale;
            float pRot = rotation;

            FXPivot pivot = PatternPivot;
            FXBlend blend = blending;

            float luminosity = Luminosity;
            float luminosityRandomness = LuminosityRandomness;

            GetQuadParams(i, imax, x, y, ref pTrans, ref pScale, ref pRot, ref pivot, ref blend, ref luminosity, ref luminosityRandomness);

            float rlum = CalculateRandomLuminosity(i, luminosityRandomness);
            luminosity += rlum;
            luminosity = Math.Min(1.0f, Math.Max(0, luminosity));

            float angle = (float)(pRot * (Math.PI / 180.0f));

            return new FXQuadData(quad, blend, pivot, luminosity, angle, pTrans, pScale);
        }

        void GetQuadParams(float i, float imax, float x, float y, ref MVector trans, 
            ref MVector scale, ref float rot, 
            ref FXPivot pivot, ref FXBlend blend, 
            ref float luminosity, ref float luminosityRandomness)
        {
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Blending"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Blending"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Blending").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(x, y), NodeType.Float2);
                    g.SetVar("iteration", i, NodeType.Float);
                    g.SetVar("maxIterations", imax, NodeType.Float);
                    g.TryAndProcess();
                    blend = (FXBlend)Convert.ToInt32(g.Result);
                }
                else
                {
                    blend = (FXBlend)Convert.ToInt32(ParentGraph.GetParameterValue(Id, "Blending"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Luminosity"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(x, y), NodeType.Float2);
                    g.SetVar("iteration", i, NodeType.Float);
                    g.SetVar("maxIterations", imax, NodeType.Float);
                    g.TryAndProcess();
                    luminosity = Convert.ToSingle(g.Result);
                }
                else
                {
                    luminosity = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Luminosity"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "LuminosityRandomness"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(x, y), NodeType.Float2);
                    g.SetVar("iteration", i, NodeType.Float);
                    g.SetVar("maxIterations", imax, NodeType.Float);
                    g.TryAndProcess();
                    luminosityRandomness = Convert.ToSingle(g.Result);
                }
                else
                {
                    luminosityRandomness = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "LuminosityRandomness"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "PatternPivot"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "PatternPivot"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "PatternPivot").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(x, y), NodeType.Float2);
                    g.SetVar("iteration", i, NodeType.Float);
                    g.SetVar("maxIterations", imax, NodeType.Float);
                    g.TryAndProcess();
                    pivot = (FXPivot)Convert.ToInt32(g.Result);
                }
                else
                {
                    pivot = (FXPivot)Convert.ToInt32(ParentGraph.GetParameterValue(Id, "PatternPivot"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Translation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Translation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Translation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(x, y), NodeType.Float2);
                    g.SetVar("iteration", i, NodeType.Float);
                    g.SetVar("maxIterations", imax, NodeType.Float);
                    g.TryAndProcess();

                    object o = g.Result;
                    if(o != null && o is MVector)
                    {
                        trans = (MVector)o;
                    }
                }
                else
                {
                    object o = ParentGraph.GetParameterValue(Id, "Translation");
                    if (o != null && o is MVector)
                    {
                        trans = (MVector)o;
                    }
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Scale").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(x, y), NodeType.Float2);
                    g.SetVar("iteration", i, NodeType.Float);
                    g.SetVar("maxIterations", imax, NodeType.Float);
                    g.TryAndProcess();

                    object o = g.Result;
                    if(o != null && o is MVector)
                    {
                        scale = (MVector)o;
                    }
                }
                else
                {
                    object o = ParentGraph.GetParameterValue(Id, "Scale");
                    if (o != null && o is MVector)
                    {
                        scale = (MVector)o;
                    }
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Rotation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(x, y), NodeType.Float2);
                    g.SetVar("iteration", i, NodeType.Float);
                    g.SetVar("maxIterations", imax, NodeType.Float);
                    g.TryAndProcess();
                    rot = Convert.ToSingle(g.Result);
                }
                else
                {
                    rot = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Rotation"));
                }
            }
        }

        public override Task GetTask()
        {
            return Task.Factory.StartNew(() =>
            {
                GetParams();
                CollectQuadData();
            })
            .ContinueWith(t =>
            {
                if (q1.HasInput || q2.HasInput || q3.HasInput || q4.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        private void CollectQuadData()
        {
            quads.Clear();

            if (quadsConnected == 0 || pmaxIter == 0) return;

            bool q1IsValid = quadsConnected >= 1;
            bool q2IsValid = quadsConnected >= 2;
            bool q3IsValid = quadsConnected >= 3;
            bool q4IsValid = quadsConnected >= 4;

            ///this part here is the most computational
            ///expensive and is thus better to run in a separate thread
            for (int i = 0; i < pmaxIter; i++)
            {
                if(q1IsValid)
                {
                    quads.Add(GetQuad(i, 0, 0, pmaxIter, 0));
                }
                if(q2IsValid)
                {
                    quads.Add(GetQuad(i, 1, 0, pmaxIter, 1));
                }
                if(q3IsValid)
                {
                    quads.Add(GetQuad(i, 0, 1, pmaxIter, 2));
                }
                if(q4IsValid)
                {
                    quads.Add(GetQuad(i, 1, 1, pmaxIter, 3));
                }
            }
        }

        private void GetParams()
        {
            quadsConnected = 0;

            if (q1.HasInput && q1.Input.Data != null) quadsConnected++;
            if (q2.HasInput && q2.Input.Data != null) quadsConnected++;
            if (q3.HasInput && q3.Input.Data != null) quadsConnected++;
            if (q4.HasInput && q4.Input.Data != null) quadsConnected++;

            pmaxIter = iterations;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Iterations"))
            {
                pmaxIter = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Iterations"));
            }

            ///ho boy this was not cool!
            if (float.IsNaN(pmaxIter) || float.IsInfinity(pmaxIter))
            {
                pmaxIter = 0;
            }

            //also we are capping to a maximum of 512
            //for performance reasons
            pmaxIter = Math.Min(pmaxIter, 512);
        }

        List<FXQuadData> quads = new List<FXQuadData>();
        int quadsConnected;
        float pmaxIter;
        void Process()
        {
            //we release the previous buffer if there is one
            //as we have to make sure we have a clean buffer
            //for the iteration cycles
            //and quadrant transforms
            if(buffer != null)
            {
                buffer.Release();
                buffer = null;
            }

            if (processor == null) return;

            if (quadsConnected == 0 || pmaxIter == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Prepare(width, height, null, buffer);

            foreach(FXQuadData d in quads)
            {
                ProcessQuad(d, quadsConnected);
            }

            processor.Complete();

            quads.Clear();

            Updated();
            Output.Data = buffer;
            Output.Changed();
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

        public class FXNodeData : NodeData
        {
            public int iterations;
            public int rotation;
            public float tx;
            public float ty;
            public float sx;
            public float sy;
            public int pivot;
            public int blending;
        }

        public override string GetJson()
        {
            FXNodeData d = new FXNodeData();
            FillBaseNodeData(d);
            d.iterations = iterations;
            d.rotation = rotation;
            d.tx = translation.X;
            d.ty = translation.Y;
            d.sx = scale.X;
            d.sy = scale.Y;
            d.pivot = (int)patternPivot;
            d.blending = (int)blending;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            FXNodeData d = JsonConvert.DeserializeObject<FXNodeData>(data);
            SetBaseNodeDate(d);
            iterations = d.iterations;
            rotation = d.rotation;
            translation = new MVector(d.tx, d.ty);
            scale = new MVector(d.sx, d.sy);
            patternPivot = (FXPivot)d.pivot;
            blending = (FXBlend)d.blending;
        }
    }
}
