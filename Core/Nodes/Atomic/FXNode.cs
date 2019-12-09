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
using Materia.GLInterfaces;
using NLog;
using Materia.Nodes.MathNodes;

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
        Max = 2,
        AddSub = 3
    }

    public class FXNode : ImageNode
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        NodeInput q1;
        NodeInput q2;
        NodeInput q3;
        NodeInput q4;

        NodeOutput Output;

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
                TriggerValueChange();
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
                TriggerValueChange();
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
                TriggerValueChange();
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
                TriggerValueChange();
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
                TriggerValueChange();
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
                TriggerValueChange();
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
                TriggerValueChange();
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
                TriggerValueChange();
            }
        }

        protected bool clamp;
        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Clamp", "Effects")]
        public bool Clamp
        {
            get
            {
                return clamp;
            }
            set
            {
                clamp = value;
                TriggerValueChange();
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

        private string translationFuncName;
        private bool translationIsFunc;
        private string translationCode;

        private string rotationFuncName;
        private bool rotationIsFunc;
        private string rotationCode;

        private string scaleFuncName;
        private bool scaleIsFunc;
        private string scaleCode;

        private string pivotFuncName;
        private bool pivotIsFunc;
        private string pivotCode;

        private string luminFuncName;
        private bool luminIsFunc;
        private string luminCode;

        private string luminRandomFuncName;
        private bool luminRandomIsFunc;
        private string luminRandomCode;

        private bool blendIsFunc;
        private string blendFuncName;
        private string blendCode;

        private bool clampIsFunc;
        private string clampFuncName;
        private string clampCode;

        private bool rebuildShader;
        private IGLProgram shader;

        private string previousCalls = "";
        private HashSet<string> previousCallsSeen;

        private string uniformParamCode;
        private Dictionary<string, GraphParameterValue> uniformParams;

        public FXNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "FX";

            uniformParamCode = "";
            previousCalls = "";
            uniformParams = new Dictionary<string, GraphParameterValue>();
            previousCallsSeen = new HashSet<string>();

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            width = w;
            height = h;

            clamp = true;

            internalPixelType = p;
            luminosity = 1.0f;
            luminosityRandomness = 0.0f;

            iterations = 1;
            translation = new MVector();
            scale = new MVector(1, 1);
            rotation = 0;

            blending = FXBlend.Blend;

            previewProcessor = new BasicImageRenderer();

            q1 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Quadrant");
            q2 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Quadrant");
            q3 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Quadrant");
            q4 = new NodeInput(NodeType.Color | NodeType.Gray, this, "Quadrant");

            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(q1);
            Inputs.Add(q2);
            Inputs.Add(q3);
            Inputs.Add(q4);
            Outputs.Add(Output);
        }

        void RebuildCustomFuncs()
        {
            if (ParentGraph != null)
            {
                foreach (FunctionGraph f in ParentGraph.CustomFunctions)
                {
                    if (previousCallsSeen.Contains(f.CodeName))
                    {
                        continue;
                    }

                    //get call stack
                    Stack<CallNode> finalStack = f.GetFullCallStack();

                    while(finalStack.Count > 0)
                    {
                        CallNode m = finalStack.Pop();
                        if (m.selectedFunction == null) continue;
                        if (previousCallsSeen.Contains(m.selectedFunction.CodeName)) continue;
                        previousCallsSeen.Add(m.selectedFunction.CodeName);

                        string mf = m.GetFunctionShaderCode();

                        if(previousCalls.IndexOf(mf) == -1)
                        {
                            previousCalls += mf;
                        }
                    }

                    if (!previousCallsSeen.Contains(f.CodeName))
                    {
                        previousCallsSeen.Add(f.CodeName);

                        string s = f.GetFunctionShaderCode();

                        if (previousCalls.IndexOf(s) == -1)
                        {
                            previousCalls += s;
                        }
                    }
                }
            }
        }

        void CollectCalls(FunctionGraph g)
        {
            Stack<CallNode> calls = g.GetFullCallStack();

            while(calls.Count > 0)
            {
                CallNode n = calls.Pop();
                FunctionGraph f = n.selectedFunction;

                if (f == null) continue;
                if (previousCallsSeen.Contains(f.CodeName)) continue;
                previousCallsSeen.Add(f.CodeName);

                string mf = f.GetFunctionShaderCode();
                if(previousCalls.IndexOf(mf) == -1)
                {
                    previousCalls += mf;
                }
            }
        }

        bool IsRebuildRequired()
        {
            if (rebuildShader) return true;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Blending"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Blending"))
                {
                    FunctionGraph g = parentGraph.GetParameterRaw(Id, "Blending").Value as FunctionGraph;
                    if (g.Modified || !blendIsFunc)
                    {
                        return true;
                    }
                }
                else if(string.IsNullOrEmpty(blendCode) || blendIsFunc)
                {
                    return true;
                }
            }
            else if(string.IsNullOrEmpty(blendCode) || blendIsFunc)
            {
                return true;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Luminosity"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = parentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;
                    if (g.Modified || !luminIsFunc)
                    {
                        return true;
                    }
                }
                else if (string.IsNullOrEmpty(luminCode) || luminIsFunc)
                {
                    return true;
                }
            }
            else if (string.IsNullOrEmpty(luminCode) || luminIsFunc)
            {
                return true;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "LuminosityRandomness"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "LuminosityRandomness"))
                {
                    FunctionGraph g = parentGraph.GetParameterRaw(Id, "LuminosityRandomness").Value as FunctionGraph;
                    if (g.Modified || !luminRandomIsFunc)
                    {
                        return true;
                    }
                }
                else if (string.IsNullOrEmpty(luminRandomCode) || luminRandomIsFunc)
                {
                    return true;
                }
            }
            else if (string.IsNullOrEmpty(luminRandomCode) || luminRandomIsFunc)
            {
                return true;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "PatternPivot"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "PatternPivot"))
                {
                    FunctionGraph g = parentGraph.GetParameterRaw(Id, "PatternPivot").Value as FunctionGraph;
                    if (g.Modified || !pivotIsFunc)
                    {
                        return true;
                    }
                }
                else if (string.IsNullOrEmpty(pivotCode) || pivotIsFunc)
                {
                    return true;
                }
            }
            else if (string.IsNullOrEmpty(pivotCode) || pivotIsFunc)
            {
                return true;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Translation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Translation"))
                {
                    FunctionGraph g = parentGraph.GetParameterRaw(Id, "Translation").Value as FunctionGraph;
                    if (g.Modified || !translationIsFunc)
                    {
                        return true;
                    }
                }
                else if (string.IsNullOrEmpty(translationCode) || translationIsFunc)
                {
                    return true;
                }
            }
            else if (string.IsNullOrEmpty(translationCode) || translationIsFunc)
            {
                return true;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                {
                    FunctionGraph g = parentGraph.GetParameterRaw(Id, "Scale").Value as FunctionGraph;
                    if (g.Modified || !scaleIsFunc)
                    {
                        return true;
                    }
                }
                else if (string.IsNullOrEmpty(scaleCode) || scaleIsFunc)
                {
                    return true;
                }
            }
            else if (string.IsNullOrEmpty(scaleCode) || scaleIsFunc)
            {
                return true;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                {
                    FunctionGraph g = parentGraph.GetParameterRaw(Id, "Rotation").Value as FunctionGraph;
                    if (g.Modified || !rotationIsFunc)
                    {
                        return true;
                    }
                }
                else if (string.IsNullOrEmpty(rotationCode) || rotationIsFunc)
                {
                    return true;
                }
            }
            else if (string.IsNullOrEmpty(rotationCode) || rotationIsFunc)
            {
                return true;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Clamp"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Clamp"))
                {
                    FunctionGraph g = parentGraph.GetParameterRaw(Id, "Clamp").Value as FunctionGraph;
                    if (g.Modified || !clampIsFunc)
                    {
                        return true;
                    }
                }
                else if (string.IsNullOrEmpty(clampCode) || clampIsFunc)
                {
                    return true;
                }
            }
            else if (string.IsNullOrEmpty(clampCode) || clampIsFunc)
            {
                return true;
            }

            return false;
        }

        void GetParamCode()
        {
            if (quadsConnected == 0) return;

            if (!IsRebuildRequired()) return;

            blendCode = "";
            luminCode = "";
            luminRandomCode = "";
            rotationCode = "";
            translationCode = "";
            scaleCode = "";
            pivotCode = "";
            clampCode = "";

            uniformParamCode = "";
            uniformParams.Clear();
            previousCalls = "";
            previousCallsSeen.Clear();

            rebuildShader = true;

            RebuildCustomFuncs();

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Blending"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Blending"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Blending").Value as FunctionGraph;

                    CollectCalls(g);

                    uniformParamCode += g.GetParentGraphShaderParams(true, uniformParams);

                    blendCode = g.GetFunctionShaderCode();

                    if (previousCalls.IndexOf(blendCode) > -1)
                    {
                        blendCode = "";
                    }
                    else
                    {
                        previousCalls += blendCode;
                    }

                    blendIsFunc = true;
                    blendFuncName = g.CodeName;
                }
                else
                {
                    blendIsFunc = false;
                    blendCode = "uniform float Blending = " + Utils.ConvertToInt(ParentGraph.GetParameterValue(Id, "Blending")).ToCodeString() + ";";
                    previousCalls += blendCode;
                }
            }
            else
            {
                blendIsFunc = false;
                blendCode = "uniform float Blending = " + ((int)blending).ToCodeString() + ";";
                previousCalls += blendCode;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Luminosity"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Luminosity").Value as FunctionGraph;

                    CollectCalls(g);

                    uniformParamCode += g.GetParentGraphShaderParams(true, uniformParams);

                    luminCode = g.GetFunctionShaderCode();

                    if (previousCalls.IndexOf(luminCode) > -1)
                    {
                        luminCode = "";
                    }
                    else
                    {
                        previousCalls += luminCode;
                    }

                    luminIsFunc = true;
                    luminFuncName = g.CodeName;
                }
                else
                {
                    luminIsFunc = false;
                    luminCode = "uniform float Luminosity = " + Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Luminosity")).ToCodeString() + ";";
                    previousCalls += luminCode;
                }
            }
            else
            {
                luminIsFunc = false;
                luminCode = "uniform float Luminosity = " + luminosity.ToCodeString() + ";";
                previousCalls += luminCode;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "LuminosityRandomness"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "LuminosityRandomness"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "LuminosityRandomness").Value as FunctionGraph;

                    CollectCalls(g);

                    uniformParamCode += g.GetParentGraphShaderParams(true, uniformParams);

                    luminRandomCode = g.GetFunctionShaderCode();

                    if (previousCalls.IndexOf(luminRandomCode) > -1)
                    {
                        luminRandomCode = "";
                    }
                    else
                    {
                        previousCalls += luminRandomCode;
                    }

                    luminRandomIsFunc = true;
                    luminRandomFuncName = g.CodeName;
                }
                else
                {
                    luminRandomIsFunc = false;
                    luminRandomCode = "uniform float LuminosityRandomness = " + Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "LuminosityRandomness")).ToCodeString() + ";";
                    previousCalls += luminRandomCode;
                }
            }
            else
            {
                luminRandomIsFunc = false;
                luminRandomCode = "uniform float LuminosityRandomness = " + luminosityRandomness.ToCodeString() + ";";
                previousCalls += luminRandomCode;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "PatternPivot"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "PatternPivot"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "PatternPivot").Value as FunctionGraph;

                    CollectCalls(g);

                    uniformParamCode += g.GetParentGraphShaderParams(true, uniformParams);

                    pivotCode = g.GetFunctionShaderCode();

                    if (previousCalls.IndexOf(pivotCode) > -1)
                    {
                        pivotCode = "";
                    }
                    else
                    {
                        previousCalls += pivotCode;
                    }

                    pivotIsFunc = true;
                    pivotFuncName = g.CodeName;
                }
                else
                {
                    pivotIsFunc = false;
                    pivotCode = "uniform float PatternPivot = " + Utils.ConvertToInt(ParentGraph.GetParameterValue(Id, "PatternPivot")).ToCodeString() + ";";
                    previousCalls += pivotCode;
                }
            }
            else
            {
                pivotIsFunc = false;
                pivotCode = "uniform float PatternPivot = " + ((int)patternPivot).ToCodeString() + ";";
                previousCalls += pivotCode;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Translation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Translation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Translation").Value as FunctionGraph;

                    CollectCalls(g);

                    uniformParamCode += g.GetParentGraphShaderParams(true, uniformParams);

                    translationCode = g.GetFunctionShaderCode();

                    if (previousCalls.IndexOf(translationCode) > -1)
                    {
                        translationCode = "";
                    }
                    else
                    {
                        previousCalls += translationCode;
                    }

                    translationIsFunc = true;
                    translationFuncName = g.CodeName;
                }
                else
                {
                    translationIsFunc = false;
                    MVector v = ParentGraph.GetParameterValue<MVector>(Id, "Translation");
                    translationCode = "uniform vec2 Translation = vec2(" + v.X + "," + v.Y + ");";
                    previousCalls += translationCode;
                }
            }
            else
            {
                translationIsFunc = false;
                translationCode = "uniform vec2 Translation = vec2(" + translation.X + "," + translation.Y + ");";
                previousCalls += translationCode;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Scale").Value as FunctionGraph;

                    CollectCalls(g);

                    uniformParamCode += g.GetParentGraphShaderParams(true, uniformParams);

                    scaleCode = g.GetFunctionShaderCode();

                    if (previousCalls.IndexOf(scaleCode) > -1)
                    {
                        scaleCode = "";
                    }
                    else
                    {
                        previousCalls += scaleCode;
                    }

                    scaleIsFunc = true;
                    scaleFuncName = g.CodeName;
                }
                else
                {
                    scaleIsFunc = false;
                    MVector v = ParentGraph.GetParameterValue<MVector>(Id, "Scale");
                    scaleCode = "uniform vec2 Scale = vec2(" + v.X + "," + v.Y + ");";
                    previousCalls += scaleCode;
                }
            }
            else
            {
                scaleIsFunc = false;
                scaleCode = "uniform vec2 Scale = vec2(" + scale.X + "," + scale.Y + ");";
                previousCalls += scaleCode;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Rotation").Value as FunctionGraph;

                    CollectCalls(g);

                    uniformParamCode += g.GetParentGraphShaderParams(true, uniformParams);

                    rotationCode = g.GetFunctionShaderCode();

                    if (previousCalls.IndexOf(rotationCode) > -1)
                    {
                        rotationCode = "";
                    }
                    else
                    {
                        previousCalls += rotationCode;
                    }

                    rotationIsFunc = true;
                    rotationFuncName = g.CodeName;
                }
                else
                {
                    rotationIsFunc = false;
                    rotationCode = "uniform float Rotation = " + Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Rotation")).ToCodeString() + ";";
                    previousCalls += rotationCode;
                }
            }
            else
            {
                rotationIsFunc = false;
                rotationCode = "uniform float Rotation = " + rotation.ToCodeString() + ";";
                previousCalls += rotationCode;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Clamp"))
            {
                if (ParentGraph.IsParameterValueFunction(Id, "Clamp"))
                {
                    FunctionGraph g = ParentGraph.GetParameterRaw(Id, "Clamp").Value as FunctionGraph;

                    CollectCalls(g);

                    uniformParamCode += g.GetParentGraphShaderParams(true, uniformParams);

                    clampCode = g.GetFunctionShaderCode();

                    if (previousCalls.IndexOf(clampCode) > -1)
                    {
                        clampCode = "";
                    }
                    else
                    {
                        previousCalls += clampCode;
                    }

                    clampIsFunc = true;
                    clampFuncName = g.CodeName;
                }
                else
                {
                    clampIsFunc = false;
                    clampCode = "uniform float Clamp = " + Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Clamp")).ToCodeString() + ";";
                    previousCalls += clampCode;
                }
            }
            else
            {
                clampIsFunc = false;
                clampCode = "uniform float Clamp = " + rotation.ToCodeString() + ";";
                previousCalls += clampCode;
            }
        }

        private float urot;
        private MVector uscale;
        private MVector utranslate;
        private float upivot;
        private float uluminrand;
        private float ulumin;
        private float ublend;
        private float uclamp;
        private void GetUniformValues()
        {
            if (quadsConnected == 0) return;

            ublend = (int)blending;
            upivot = (int)patternPivot;
            urot = (float)rotation;
            uscale = scale;
            utranslate = translation;
            ulumin = luminosity;
            uluminrand = luminosityRandomness;
            uclamp = clamp ? 1 : 0;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Blending"))
            {
                if (!ParentGraph.IsParameterValueFunction(Id, "Blending"))
                {
                    ublend = Utils.ConvertToInt(ParentGraph.GetParameterValue(Id, "Blending"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Luminosity"))
            {
                if (!ParentGraph.IsParameterValueFunction(Id, "Luminosity"))
                {
                    ulumin = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Luminosity"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "LuminosityRandomness"))
            {
                if (!ParentGraph.IsParameterValueFunction(Id, "LuminosityRandomness"))
                {
                    uluminrand = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "LuminosityRandomness"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "PatternPivot"))
            {
                if (!ParentGraph.IsParameterValueFunction(Id, "PatternPivot"))
                {
                    upivot = Utils.ConvertToInt(ParentGraph.GetParameterValue(Id, "PatternPivot"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Translation"))
            {
                if (!ParentGraph.IsParameterValueFunction(Id, "Translation"))
                {
                    utranslate = ParentGraph.GetParameterValue<MVector>(Id, "Translation");
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                if (!ParentGraph.IsParameterValueFunction(Id, "Scale"))
                {
                     uscale = ParentGraph.GetParameterValue<MVector>(Id, "Scale");
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
            {
                if (!ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                {
                    urot = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Rotation"));
                }
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Clamp"))
            {
                if (!ParentGraph.IsParameterValueFunction(Id, "Clamp"))
                {
                    uclamp = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Clamp"));
                }
            }
        }

        private void GetParams()
        {
            quadsConnected = 0;

            if (q1.HasInput && q1.Reference.Data != null) quadsConnected++;
            if (q2.HasInput && q2.Reference.Data != null) quadsConnected++;
            if (q3.HasInput && q3.Reference.Data != null) quadsConnected++;
            if (q4.HasInput && q4.Reference.Data != null) quadsConnected++;

            if (quadsConnected == 0) return;

            pmaxIter = iterations;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Iterations"))
            {
                pmaxIter = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Iterations"));
            }

            if (float.IsNaN(pmaxIter) || float.IsInfinity(pmaxIter))
            {
                pmaxIter = 0;
            }

            //also we are capping to a maximum of 512
            //for performance reasons
            pmaxIter = Math.Min(pmaxIter, 512);
        }

        /// <summary>
        /// We have to rebuild the FX graph pixel
        /// shader when it changes due to
        /// specifying the underling
        /// pixel format in the shader
        /// itself
        /// </summary>
        protected override void OnPixelFormatChange()
        {
            base.OnPixelFormatChange();
            rebuildShader = true;
        }

        public override void AssignPixelType(GraphPixelType pix)
        {
            base.AssignPixelType(pix);
            rebuildShader = true;
        }

        private void BuildShader()
        {
            if (!rebuildShader) return;

            if(shader != null)
            {
                shader.Release();
                shader = null;
            }

            GraphPixelType type = internalPixelType;
            string outputType = "rgba32f";

            if (type == GraphPixelType.RGBA16F || type == GraphPixelType.RGB16F)
            {
                outputType = "rgba16f";
            }
            else if (type == GraphPixelType.RGBA || type == GraphPixelType.RGB)
            {
                outputType = "rgba8";
            }
            else if (type == GraphPixelType.Luminance32F)
            {
                outputType = "r32f";
            }
            else if (type == GraphPixelType.Luminance16F)
            {
                outputType = "r16f";
            }

            string frag = "#version 430 core\r\n"
             + "layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;\r\n"
             + $"layout({outputType}, binding = 0) uniform image2D _out_put;\r\n"
             + "uniform sampler2D Input0;\r\n"
             + $"uniform float inWidth = {width};\r\n"
             + $"uniform float inHeight = {height};\r\n"
             + "uniform float quadCount = 0;\r\n"
             + "const float PI = 3.14159265359;\r\n"
             + "const float Rad2Deg = (180.0 / PI);\r\n"
             + "const float Deg2Rad = (PI / 180.0);\r\n"
             + "uniform float RandomSeed = " + parentGraph.RandomSeed.ToCodeString() + ";\r\n"
             + $"const vec2 size = vec2({width},{height});\r\n"
             + FunctionGraph.GLSLHash + "\r\n"
             + $"const float maxIterations = {pmaxIter};\r\n"
             + "uniform vec2 w_pos = vec2(0,0);\r\n"
             + "uniform float quad = 0;\r\n"
             + "uniform float _iteration_z = 0;\r\n"
             + "vec2 pos = vec2(0,0);\r\n"
             + "float iteration = 0;\r\n"
             + "vec2 uv = vec2(0,0);\r\n"
             + "float AddSub(float a, float b) {\r\n"
                + "if (a >= 0.5) { return min(1, max(0, a + b)); }\r\n"
                + "else { return min(1, max(0, b - a)); }\r\n}\r\n"
             + "vec4 BlendColors(float blendIdx, vec4 c1, vec4 c2) {\r\n"
                + "vec4 fc = vec4(0);\r\n"
                + "blendIdx = floor(blendIdx);\r\n"
                + "if (blendIdx <= 0) { fc.rgb = min(vec3(1), max(vec3(0), c1.rgb)) * min(1, max(0, c1.a)) + min(vec3(1), max(vec3(0), c2.rgb)) * (1.0 - min(1, max(0, c1.a)));  fc.a = max(c1.a, c2.a); }\r\n"
                + "else if(blendIdx <= 1) { fc.rgb = min(vec3(1), max(vec3(0), c1.rgb)) + min(vec3(1), max(vec3(0), c2.rgb)); fc.a = max(c1.a, c2.a); }\r\n"
                + "else if(blendIdx <= 2) { fc.rgb = vec3(max(c1.r, c2.r), max(c1.g, c2.g), max(c1.b, c2.b)); fc.a = max(c1.a, c2.a); }\r\n"
                + "else if(blendIdx <= 3) { fc.rgb = vec3(AddSub(min(1, max(0, c1.r)), min(1, max(0, c2.r))), AddSub(min(1, max(0, c1.g)), min(1, max(0, c2.g))), AddSub(min(1, max(0, c1.b)), min(1, max(0, c2.b)))); fc.a = max(c1.a, c2.a); }\r\n"
                + "return fc; }\r\n"
                + $"{uniformParamCode}\r\n"
            + $"{previousCalls}\r\n";

            string fragMain = "void main() {\r\n"
                            + "iteration = float(gl_GlobalInvocationID.z);\r\n"
                            + "ivec2 c_pos = ivec2(gl_GlobalInvocationID.xy);\r\n"
                            + "uv = vec2(gl_GlobalInvocationID.xy) / size;\r\n"
                            + "pos = w_pos * iteration;\r\n"
                            + "ivec2 i_pos = ivec2(inWidth * uv.x, inHeight * uv.y);\r\n"
                            + "vec2 pivotPoint = vec2(0,0);\r\n"
                            + "vec2 quadOffset = vec2(0,0);\r\n"
                            + "float qx = 0.5;\r\n"
                            + "float qy = 0.5;\r\n"
                            + "if (quadCount <= 1) { qx = 0; qy = 0; }\r\n"
                            + "else if(quadCount <= 2) { qy = 0; }\r\n"
                            + "if (w_pos.x == 0 && w_pos.y == 0) { quadOffset = vec2(-qx, -qy); }\r\n"
                            + "else if(w_pos.x == 1 && w_pos.y == 0) { quadOffset = vec2(qx, -qy); }\r\n"
                            + "else if(w_pos.x == 0 && w_pos.y == 1) {\r\n"
                            + "quadOffset.x = -qx;\r\n"
                            + "quadOffset.y = qy; }\r\n"
                            + "else if(w_pos.x == 1 && w_pos.y == 1) { quadOffset = vec2(qx,qy); }\r\n"
                            + "float mw = size.x * 0.5;\r\n"
                            + "float mh = size.y * 0.5;\r\n"
                            + "if (quadCount <= 1) { mw = size.x; mh = size.y; }\r\n"
                            + "else if(quadCount == 2) { mh = size.y; }\r\n"
                            + "float ww = mw / inWidth;\r\n"
                            + "float wh = mh / inHeight;\r\n";

            //we are doing this so everything is standardized
            //for final calculations
            if (blendIsFunc)
            {
                fragMain += $"float blendIdx = {blendFuncName}();\r\n";
            }
            else
            {
                fragMain += "float blendIdx = Blending;\r\n";
            }

            if (pivotIsFunc)
            {
                fragMain += $"float pivotIdx = {pivotFuncName}();\r\n";
            }
            else
            {
                fragMain += "float pivotIdx = PatternPivot;\r\n";
            }

            fragMain += "if (pivotIdx == 0) { pivotPoint = vec2(0.5,0.5); }\r\n"
                       + "else if(pivotIdx == 1) {pivotPoint = vec2(0.25,0.25); }\r\n"
                       + "else if(pivotIdx == 2) {pivotPoint = vec2(0.75,0.75); }\r\n"
                       + "else if(pivotIdx == 3) {pivotPoint = vec2(0.25, 0.5); }\r\n"
                       + "else if(pivotIdx == 4) {pivotPoint = vec2(0.75, 0.5); }\r\n"
                       + "else if(pivotIdx == 5) {pivotPoint = vec2(0.5, 0.25); }\r\n"
                       + "else if(pivotIdx == 6) {pivotPoint = vec2(0.5, 0.75); }\r\n";

            if (rotationIsFunc)
            {
                fragMain += $"float angle = {rotationFuncName}() * Deg2Rad;\r\n";
            }
            else
            {
                fragMain += "float angle = Rotation * Deg2Rad;\r\n";
            }

            if (scaleIsFunc)
            {
                fragMain += $"vec2 scale = {scaleFuncName}() * vec2(ww, wh);\r\n";
            }
            else
            {
                fragMain += "vec2 scale = Scale * vec2(ww, wh);\r\n";
            }

            if (translationIsFunc)
            {
                fragMain += $"vec2 trans = {translationFuncName}() + quadOffset;\r\n";
            }
            else
            {
                fragMain += "vec2 trans = Translation + quadOffset;\r\n";
            }

            fragMain += "trans.x = trans.x * (inWidth * ww);\r\n"
                        + "trans.y = trans.y * (inHeight * wh);\r\n";

            if(luminIsFunc)
            {
                fragMain += $"float lumin = {luminFuncName}();\r\n";
            } 
            else
            {
                fragMain += "float lumin = Luminosity;\r\n";
            }

            if(luminRandomIsFunc)
            {
                fragMain += $"float luminRand = {luminRandomFuncName}();\r\n";
            }
            else
            {
                fragMain += "float luminRand = LuminosityRandomness;\r\n";
            }

            if(clampIsFunc)
            {
                fragMain += $"float clamp = {clampFuncName}();\r\n";
            }
            else
            {
                fragMain += "float clamp = Clamp;\r\n";
            }

            fragMain += "float sina = sin(angle);\r\n"
                        + "float cosa = cos(angle);\r\n";

            //calculate new scale, rotation, etc
            fragMain += "ivec2 p1 = ivec2(i_pos.x - pivotPoint.x * inWidth, i_pos.y - pivotPoint.y * inHeight);\r\n"
                        + "p1 = ivec2(p1.x * cosa - p1.y * sina, p1.x * sina + p1.y * cosa);\r\n"
                        + "p1 = ivec2(p1.x / scale.x, p1.y / scale.y);\r\n"
                        + "p1 = ivec2(p1.x + pivotPoint.x * inWidth, p1.y + pivotPoint.y * inHeight);\r\n";

            fragMain += "vec4 c1 = texelFetch(Input0, p1, 0);\r\n"
                        + "if (p1.x < 0 || p1.y < 0 || p1.x >= inWidth || p1.y >= inHeight) { c1 = vec4(0); }\r\n";

            fragMain += "ivec2 finalpos = c_pos + ivec2(trans);\r\n";
            fragMain += "if (finalpos.x > size.x && clamp == 0) { finalpos.x = int(mod(finalpos.x, size.x)); }\r\n"
                       + "else if(finalpos.x < 0 && clamp == 0) { finalpos.x = int(mod(size.x + finalpos.x, size.x)); }\r\n"
                       + "if (finalpos.y > size.y && clamp == 0) { finalpos.y = int(mod(finalpos.y, size.y)); }\r\n"
                       + "else if(finalpos.y < 0 && clamp == 0) { finalpos.y = int(mod(size.y + finalpos.y, size.y)); }\r\n";

            fragMain += "vec4 c2 = imageLoad(_out_put, finalpos);\r\n";

            fragMain += "float r1 = rand(vec2(luminRand + RandomSeed + iteration, luminRand + RandomSeed + iteration)) * luminRand;\r\n"
                     + "float flum = min(1, max(0, lumin + r1));\r\n"
                     + "c1.rgb = c1.rgb * flum;\r\n";

            //blending now!
            fragMain += "vec4 fc = BlendColors(blendIdx, c1, c2);\r\n";

            //calculate lumin

            //store pixel and close main
            fragMain += "imageStore(_out_put, finalpos, fc);\r\n}\r\n";

            //Log.Debug(frag + fragMain);
            shader = Material.Material.CompileCompute(frag + fragMain);

            if (shader != null)
            {
                rebuildShader = false;
            }

            if (Graph.ShaderLogging)
            {
                Log.Debug(frag + fragMain);
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            GetParamCode();
            GetUniformValues();
            BuildShader();
            Process();
        }

        protected virtual void SetUniform(string k, object value, NodeType type)
        {
            if (value == null || shader == null) return;

            try
            {
                if (type == NodeType.Bool)
                {
                    shader.SetUniform(k, Utils.ConvertToBool(value) ? 1.0f : 0.0f);
                }
                else if (type == NodeType.Float)
                {
                    shader.SetUniform(k, Utils.ConvertToFloat(value));
                }
                else if (type == NodeType.Float2)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Math3D.Vector2 vec2 = new Math3D.Vector2(mv.X, mv.Y);
                        shader.SetUniform2(k, ref vec2);
                    }
                }
                else if (type == NodeType.Float3)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Math3D.Vector3 vec3 = new Math3D.Vector3(mv.X, mv.Y, mv.Z);
                        shader.SetUniform3(k, ref vec3);
                    }
                }
                else if (type == NodeType.Float4 || type == NodeType.Color || type == NodeType.Gray)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Math3D.Vector4 vec4 = new Math3D.Vector4(mv.X, mv.Y, mv.Z, mv.W);
                        shader.SetUniform4F(k, ref vec4);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        int quadsConnected;
        float pmaxIter;
        void Process()
        {
            if (quadsConnected == 0) return;

            if (shader == null) return;

            GLTextuer2D i1 = null;
            GLTextuer2D i2 = null;
            GLTextuer2D i3 = null;
            GLTextuer2D i4 = null;

            if (q1.HasInput) i1 = (GLTextuer2D)q1.Reference.Data;
            if (q2.HasInput) i2 = (GLTextuer2D)q2.Reference.Data;
            if (q3.HasInput) i3 = (GLTextuer2D)q3.Reference.Data;
            if (q4.HasInput) i4 = (GLTextuer2D)q4.Reference.Data;

            if (quadsConnected == 0 || pmaxIter == 0) return;

            CreateBufferIfNeeded();

            buffer.Bind();

            IGL.Primary.ClearTexImage(buffer.Id, (int)PixelFormat.Rgba, (int)PixelType.Float);

            GLTextuer2D.Unbind();

            //before we bind this shader we need to collect from the other shaders first
            //if it is a function
            foreach (string k in uniformParams.Keys)
            {
                GraphParameterValue v = uniformParams[k];

                object value = v.Value;

                //we ignore functions on the FX node
                //as we have already taken them into account
                //for all the FX node variables
                if (v.IsFunction())
                {
                    FunctionGraph temp = value as FunctionGraph;

                    if (temp.ParentNode != this)
                    {
                        if (temp.BuildAsShader)
                        {
                            temp.ComputeResult();
                        }
                        else
                        {
                            temp.TryAndProcess();
                        }
                    }
                }
            }

            shader.Use();
            shader.SetUniform("quadCount", (float)quadsConnected);
            buffer.Bind();
            buffer.BindAsImage(0, true, true);

            List<GLTextuer2D> quads = new List<GLTextuer2D>();

            if (i1 != null)
            {
                quads.Add(i1);
            }

            if (i2 != null)
            {
                quads.Add(i2);
            }

            if (i3 != null)
            {
                quads.Add(i3);
            }

            if (i4 != null)
            {
                quads.Add(i4);
            }

            shader.SetUniform("RandomSeed", (float)parentGraph.RandomSeed);

            if(!rotationIsFunc)
            {
                shader.SetUniform("Rotation", (float)urot);
            }

            if(!clampIsFunc)
            {
                shader.SetUniform("Clamp", uclamp);
            }

            if(!blendIsFunc)
            {
                shader.SetUniform("Blending", (float)ublend);
            }

            if(!translationIsFunc)
            {
                Math3D.Vector2 v2 = new Math3D.Vector2(utranslate.X, utranslate.Y);
                shader.SetUniform2("Translation", ref v2);
            }

            if(!scaleIsFunc)
            {
                Math3D.Vector2 v2 = new Math3D.Vector2(uscale.X, uscale.Y);
                shader.SetUniform2("Scale", ref v2);
            }

            if (!luminIsFunc)
            {
                shader.SetUniform("Luminosity", (float)ulumin);
            }

            if(!luminRandomIsFunc)
            {
                shader.SetUniform("LuminosityRandomness", (float)uluminrand);
            }

            if(!pivotIsFunc)
            {
                shader.SetUniform("PatternPivot", (float)upivot);
            }

            //set other uniform params
            foreach(string k in uniformParams.Keys)
            {
                GraphParameterValue v = uniformParams[k];

                object value = v.Value;

                //we ignore functions on the FX node
                //as we have already taken them into account
                //for all the FX node variables
                if (v.IsFunction())
                {
                    FunctionGraph temp = value as FunctionGraph;

                    if(temp.ParentNode != this)
                    {
                        value = temp.Result;
                    }
                    else
                    {
                        continue;
                    }
                }

                SetUniform(k, value, v.Type);
            }

            Math3D.Vector2 rpos = new Math3D.Vector2(0,0);
            for (int i = 0; i < quads.Count; ++i)
            {
                GLTextuer2D target = quads[i];
                shader.SetUniform("quad", (float)i);
                shader.SetUniform("Input0", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                target.Bind();

                shader.SetUniform("inWidth", (float)target.Width);
                shader.SetUniform("inHeight", (float)target.Height);

                if (i == 0)
                {
                    rpos.X = 0;
                    rpos.Y = 0;
                    shader.SetUniform2("w_pos", ref rpos);
                }
                else if (i == 1)
                {
                    rpos.X = 1;
                    rpos.Y = 0;
                    shader.SetUniform2("w_pos", ref rpos);
                }
                else if (i == 2)
                {
                    rpos.X = 0;
                    rpos.Y = 1;
                    shader.SetUniform2("w_pos", ref rpos);
                }
                else if (i == 3)
                {
                    rpos.X = 1;
                    rpos.Y = 1;
                    shader.SetUniform2("w_pos", ref rpos);
                }

                IGL.Primary.DispatchCompute(width / 8, height / 8, (int)pmaxIter);
            }

            GLTextuer2D.UnbindAsImage(0);
            GLTextuer2D.Unbind();
            shader.Unbind();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (shader != null)
            {
                shader.Release();
                shader = null;
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
            public bool clamp;
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
            d.clamp = clamp;

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
            clamp = d.clamp;
        }
    }
}
