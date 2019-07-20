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

    public class FXNode : ImageNode
    {
        CancellationTokenSource ctk;
        NodeInput q1;
        NodeInput q2;
        NodeInput q3;
        NodeInput q4;

        NodeOutput Output;

        FXProcessor processor;

        protected int iterations;
        [Promote(NodeType.Float)]
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
        [Section(Section = "Transform")]
        [Vector(NodeType.Float2)]
        [Promote(NodeType.Float2)]
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
        [Section(Section = "Transform")]
        [Vector(NodeType.Float2)]
        [Promote(NodeType.Float2)]
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
        [Section(Section = "Transform")]
        [Slider(IsInt = true, Max = 360, Min = 0)]
        [Promote(NodeType.Float)]
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
        [Section(Section = "Transform")]
        [Promote(NodeType.Float)]
        [Title(Title = "Pattern Pivot")]
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
        [Section(Section = "Effects")]
        [Slider(IsInt = false, Max = 1.0f, Min = 0.0f)]
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
        [Section(Section = "Effects")]
        [Title(Title = "Luminsosity Randomness")]
        [Slider(IsInt = false, Max = 1.0f, Min = 0.0f)]
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
                    Process();
                }

                return;
            }

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(25, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                RunInContext(() =>
                {
                    if (q1.HasInput || q2.HasInput || q3.HasInput || q4.HasInput)
                    {
                        Process();
                    }
                });
            });
        }

        float CalculateRandomLuminosity(float iter, float randLum, float maxLum)
        {
            MVector m2 = new MVector(randLum + iter + ParentGraph.RandomSeed, randLum + iter + ParentGraph.RandomSeed);
            float f = Utils.Rand(ref m2);
            f = f * (maxLum * randLum);
            return f;
        }

        void ProcessQuad1(float i, float imax, int quads)
        {
            if (!q1.HasInput) return;
            GLTextuer2D i1 = (GLTextuer2D)q1.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            MVector pTrans = translation;
            MVector pScale = scale;
            float pRot = rotation;
            FXPivot pivot = PatternPivot;
            float luminosity = Luminosity;
            float luminosityRandomness = LuminosityRandomness;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "Luminosity"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                luminosity = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Luminosity"));
            }

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "LuminosityRandomness"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                luminosityRandomness = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "LuminosityRandomness"));
            }

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "PatternPivot"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "PatternPivot"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "PatternPivot").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                float t = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "PatternPivot"));
                pivot = (FXPivot)((int)t);
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Translation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Translation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Translation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                object o = ParentGraph.GetParameterValue(Id, "Translation");
                if(o != null && o is MVector)
                {
                    pTrans = (MVector)o;
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Scale").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                object o = ParentGraph.GetParameterValue(Id, "Scale");
                if(o != null && o is MVector)
                {
                    pScale = (MVector)o;
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Rotation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                pRot = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Rotation"));
            }

            float rlum = CalculateRandomLuminosity(i, luminosityRandomness, luminosity);
            luminosity += rlum;
            processor.Luminosity = Math.Min(1.0f, Math.Max(0, luminosity));

            float angle = (float)(pRot * (Math.PI / 180.0f));
            processor.Scale = pScale;
            processor.Translation = pTrans;
            processor.Angle = angle;
            processor.Pivot = pivot;

            processor.Process(0, width, height, i1, buffer, quads);
        }

        void ProcessQuad2(float i, float imax, int quads)
        {
            if (!q2.HasInput) return;
            GLTextuer2D i1 = (GLTextuer2D)q2.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            MVector pTrans = translation;
            MVector pScale = scale;
            float pRot = rotation;

            FXPivot pivot = PatternPivot;

            float luminosity = Luminosity;
            float luminosityRandomness = LuminosityRandomness;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Luminosity"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                luminosity = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Luminosity"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "LuminosityRandomness"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                luminosityRandomness = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "LuminosityRandomness"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "PatternPivot"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "PatternPivot"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "PatternPivot").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                float t = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "PatternPivot"));
                pivot = (FXPivot)((int)t);
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Translation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Translation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Translation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                object o = ParentGraph.GetParameterValue(Id, "Translation");
                if (o != null && o is MVector)
                {
                    pTrans = (MVector)o;
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Scale").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                object o = ParentGraph.GetParameterValue(Id, "Scale");
                if (o != null && o is MVector)
                {
                    pScale = (MVector)o;
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Rotation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                pRot = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Rotation"));
            }

            float rlum = CalculateRandomLuminosity(i, luminosityRandomness, luminosity);
            luminosity += rlum;
            processor.Luminosity = Math.Min(1.0f, Math.Max(0, luminosity));

            float angle = (float)(pRot * (Math.PI / 180.0f));
            processor.Scale = pScale;
            processor.Translation = pTrans;
            processor.Angle = angle;
            processor.Pivot = pivot;

            processor.Process(1, width, height, i1, buffer, quads);
        }

        void ProcessQuad3(float i, float imax, int quads)
        {
            if (!q3.HasInput) return;
            GLTextuer2D i1 = (GLTextuer2D)q3.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            MVector pTrans = translation;
            MVector pScale = scale;
            float pRot = rotation;

            FXPivot pivot = PatternPivot;

            float luminosity = Luminosity;
            float luminosityRandomness = LuminosityRandomness;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Luminosity"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                luminosity = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Luminosity"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "LuminosityRandomness"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                luminosityRandomness = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "LuminosityRandomness"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "PatternPivot"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "PatternPivot"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "PatternPivot").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                float t = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "PatternPivot"));
                pivot = (FXPivot)((int)t);
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Translation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Translation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Translation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                object o = ParentGraph.GetParameterValue(Id, "Translation");
                if (o != null && o is MVector)
                {
                    pTrans = (MVector)o;
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Scale").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                object o = ParentGraph.GetParameterValue(Id, "Scale");
                if (o != null && o is MVector)
                {
                    pScale = (MVector)o;
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Rotation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                pRot = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Rotation"));
            }

            float rlum = CalculateRandomLuminosity(i, luminosityRandomness, luminosity);
            luminosity += rlum;
            processor.Luminosity = Math.Min(1.0f, Math.Max(0, luminosity));

            float angle = (float)(pRot * (Math.PI / 180.0f));
            processor.Scale = pScale;
            processor.Translation = pTrans;
            processor.Angle = angle;
            processor.Pivot = pivot;

            processor.Process(2, width, height, i1, buffer, quads);
        }

        void ProcessQuad4(float i, float imax, int quads)
        {
            if (!q4.HasInput) return;
            GLTextuer2D i1 = (GLTextuer2D)q4.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            MVector pTrans = translation;
            MVector pScale = scale;
            float pRot = rotation;

            FXPivot pivot = PatternPivot;

            float luminosity = Luminosity;
            float luminosityRandomness = LuminosityRandomness;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Luminosity"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                luminosity = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Luminosity"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "LuminosityRandomness"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                luminosityRandomness = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "LuminosityRandomness"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "PatternPivot"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "PatternPivot"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "PatternPivot").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                float t = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "PatternPivot"));
                pivot = (FXPivot)((int)t);
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Translation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Translation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Translation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                object o = ParentGraph.GetParameterValue(Id, "Translation");
                if (o != null && o is MVector)
                {
                    pTrans = (MVector)o;
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Scale").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                object o = ParentGraph.GetParameterValue(Id, "Scale");
                if (o != null && o is MVector)
                {
                    pScale = (MVector)o;
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Rotation").Value as FunctionGraph;
                    g.SetVar("pos", new MVector(i+1, i+1));
                    g.SetVar("iteration", i);
                    g.SetVar("maxIterations", imax);
                }

                pRot = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Rotation"));
            }

            float rlum = CalculateRandomLuminosity(i, luminosityRandomness, luminosity);
            luminosity += rlum;
            processor.Luminosity = Math.Min(1.0f, Math.Max(0, luminosity));

            float angle = (float)(pRot * (Math.PI / 180.0f));
            processor.Scale = pScale;
            processor.Translation = pTrans;
            processor.Angle = angle;
            processor.Pivot = pivot;

            processor.Process(3, width, height, i1, buffer, quads);
        }

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

            int quadsConnected = 0;

            if (q1.HasInput && q1.Input.Data != null) quadsConnected++;
            if (q2.HasInput && q2.Input.Data != null) quadsConnected++;
            if (q3.HasInput && q3.Input.Data != null) quadsConnected++;
            if (q4.HasInput && q4.Input.Data != null) quadsConnected++;

            if (processor == null) return;

            if (quadsConnected == 0) return;

            CreateBufferIfNeeded();

            processor.Prepare(width, height, null, buffer);

            float pmaxIter = iterations;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Iterations"))
            {
                pmaxIter = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Iterations"));
            }

            for(float i = 0; i < pmaxIter; i++)
            {
                if(q1.HasInput) ProcessQuad1(i, pmaxIter, quadsConnected);
                if(q2.HasInput) ProcessQuad2(i, pmaxIter, quadsConnected);
                if(q3.HasInput) ProcessQuad3(i, pmaxIter, quadsConnected);
                if(q4.HasInput) ProcessQuad4(i, pmaxIter, quadsConnected);
            }

            processor.Complete();

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
        }
    }
}
