using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Newtonsoft.Json;
using Materia.Rendering.Interfaces;
using MLog;
using Materia.Nodes.MathNodes;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Extensions;
using Materia.Rendering.Shaders;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class FXNode : ImageNode
    {
        NodeInput q1;
        NodeInput q2;
        NodeInput q3;
        NodeInput q4;

        NodeOutput Output;

        protected int iterations = 1;
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

        protected MVector translation = MVector.Zero;
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

        protected MVector scale = new MVector(1,1);
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

        protected int rotation = 0;
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

        protected FXPivot patternPivot = FXPivot.Center;
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

        protected float luminosity = 1;
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

        protected float luminosityRandomness = 0;
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

        protected FXBlend blending = FXBlend.Blend;
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

        protected bool clamp = true;
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

        private string rotationFuncName;
        private bool rotationIsFunc;

        private string scaleFuncName;
        private bool scaleIsFunc;

        private string pivotFuncName;
        private bool pivotIsFunc;

        private string luminFuncName;
        private bool luminIsFunc;

        private string luminRandomFuncName;
        private bool luminRandomIsFunc;

        private bool blendIsFunc;
        private string blendFuncName;

        private bool clampIsFunc;
        private string clampFuncName;

        private bool rebuildShader;
        private IGLProgram shader;

        private string previousCalls = "";
        private HashSet<string> previousCallsSeen = new HashSet<string>();

        private string uniformParamCode = "";
        private Dictionary<string, object> uniformParams = new Dictionary<string, object>();

        private Dictionary<string, bool> previousModified = new Dictionary<string, bool>();

        public FXNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            defaultName = Name = "FX";

            width = w;
            height = h;

            internalPixelType = p;

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

        void RebuildCustomFunctions()
        {
            if (isDisposing) return;
            if (ParentGraph == null) return;

            string fcode;

            foreach (Function f in ParentGraph.CustomFunctions)
            {
                if (previousCallsSeen.Contains(f.CodeName)) continue;

                //get call stack
                Stack<CallNode> finalStack = f.GetFullCallStack();

                while(finalStack.Count > 0)
                {
                    CallNode m = finalStack.Pop();
                    if (m.selectedFunction == null) continue;
                    if (previousCallsSeen.Contains(m.selectedFunction.CodeName)) continue;
                    previousCallsSeen.Add(m.selectedFunction.CodeName);

                    fcode = m.GetFunctionShaderCode();

                    if (string.IsNullOrEmpty(fcode)) continue;

                    if(previousCalls.IndexOf(fcode) == -1)
                    {
                        previousCalls += fcode;
                    }
                }

                if (previousCallsSeen.Contains(f.CodeName)) continue;

                previousCallsSeen.Add(f.CodeName);

                fcode = f.GetFunctionShaderCode();

                if (string.IsNullOrEmpty(fcode)) continue;

                if (previousCalls.IndexOf(fcode) == -1)
                {
                    previousCalls += fcode;
                }
            }
        }

        void GetParameterCode()
        {
            if (isDisposing) return;
            if (quadsConnected == 0) return;

            if (!rebuildShader) return;

            uniformParamCode = "";
            uniformParams.Clear();
            previousCalls = "";
            previousCallsSeen.Clear();

            RebuildCustomFunctions();

            blendIsFunc = GetParameterCode("Blending", ref blendFuncName, ref uniformParamCode, ref previousCalls, previousCallsSeen, uniformParams) == ParameterCodeResult.Function;
            pivotIsFunc = GetParameterCode("PatternPivot", ref pivotFuncName, ref uniformParamCode, ref previousCalls, previousCallsSeen, uniformParams) == ParameterCodeResult.Function;
            rotationIsFunc = GetParameterCode("Rotation", ref rotationFuncName, ref uniformParamCode, ref previousCalls, previousCallsSeen, uniformParams) == ParameterCodeResult.Function;
            scaleIsFunc = GetParameterCode("Scale", ref scaleFuncName, ref uniformParamCode, ref previousCalls, previousCallsSeen, uniformParams) == ParameterCodeResult.Function;
            translationIsFunc = GetParameterCode("Translation", ref translationFuncName, ref uniformParamCode, ref previousCalls, previousCallsSeen, uniformParams) == ParameterCodeResult.Function;
            luminIsFunc = GetParameterCode("Luminosity", ref luminFuncName, ref uniformParamCode, ref previousCalls, previousCallsSeen, uniformParams) == ParameterCodeResult.Function;
            luminRandomIsFunc = GetParameterCode("LuminosityRandomness", ref luminRandomFuncName, ref uniformParamCode, ref previousCalls, previousCallsSeen, uniformParams) == ParameterCodeResult.Function;
            clampIsFunc = GetParameterCode("Clamp", ref clampFuncName, ref uniformParamCode, ref previousCalls, previousCallsSeen, uniformParams) == ParameterCodeResult.Function;
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
            if (isDisposing) return;
            if (quadsConnected == 0) return;

            ublend = GetParameter("Blending", (int)blending, ParameterMode.NoFunction);
            if (!blendIsFunc && rebuildShader) uniformParamCode += $"uniform float Blending = {ublend.ToCodeString()};\r\n";

            upivot = GetParameter("PatternPivot", (int)patternPivot, ParameterMode.NoFunction);
            if (!pivotIsFunc && rebuildShader) uniformParamCode += $"uniform float PatternPivot = {upivot.ToCodeString()};\r\n";

            urot = GetParameter("Rotation", rotation, ParameterMode.NoFunction);
            if (!rotationIsFunc && rebuildShader) uniformParamCode += $"uniform float Rotation = {urot.ToCodeString()};\r\n";

            uscale = GetParameter("Scale", scale, ParameterMode.NoFunction);
            if (!scaleIsFunc && rebuildShader) uniformParamCode += $"uniform vec2 Scale = vec2({uscale.X.ToCodeString()},{uscale.Y.ToCodeString()});\r\n";

            utranslate = GetParameter("Translation", translation, ParameterMode.NoFunction);
            if (!translationIsFunc && rebuildShader) uniformParamCode += $"uniform vec2 Translation = vec2({utranslate.X.ToCodeString()},{utranslate.Y.ToCodeString()});\r\n";

            ulumin = GetParameter("Luminosity", luminosity, ParameterMode.NoFunction);
            if (!luminIsFunc && rebuildShader) uniformParamCode += $"uniform float Luminosity = {ulumin.ToCodeString()};\r\n";

            uluminrand = GetParameter("LuminosityRandomness", luminosityRandomness, ParameterMode.NoFunction);
            if (!luminRandomIsFunc && rebuildShader) uniformParamCode += $"uniform float LuminosityRandomness = {uluminrand.ToCodeString()};\r\n";

            uclamp = GetParameter("Clamp", clamp, ParameterMode.NoFunction) ? 1 : 0;
            if (!clampIsFunc && rebuildShader) uniformParamCode += $"uniform float Clamp = {uclamp.ToCodeString()};\r\n";
        }

        private void GetParams()
        {
            if (isDisposing) return;

            quadsConnected = 0;

            if (q1.HasInput && q1.Reference.Data != null) quadsConnected++;
            if (q2.HasInput && q2.Reference.Data != null) quadsConnected++;
            if (q3.HasInput && q3.Reference.Data != null) quadsConnected++;
            if (q4.HasInput && q4.Reference.Data != null) quadsConnected++;

            if (quadsConnected == 0) return;

            pmaxIter = GetParameter("Iterations", iterations);

            if (float.IsNaN(pmaxIter) || float.IsInfinity(pmaxIter))
            {
                pmaxIter = 0;
            }

            //also we are capping to a maximum of 512
            //for performance reasons
            pmaxIter = Math.Min(pmaxIter, 512);
        }

        private void NeedsUpdate()
        {
            if (isDisposing) return;
            if (rebuildShader) return;
            if (IsParameterFunctionsModified(previousModified))
            {
                rebuildShader = true;
            }
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
            if (isDisposing) return;
            if (!rebuildShader) return;

            shader?.Dispose();

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
             + Function.GLSLHash + "\r\n"
             + $"const float maxIterations = {pmaxIter};\r\n"
             + "uniform vec2 w_pos = vec2(0,0);\r\n"
             + "uniform float quad = 0;\r\n"
             + "uniform float _iteration_z = 0;\r\n"
             + "vec2 pos = vec2(0,0);\r\n"
             + "float iteration = 0;\r\n"
             + "vec2 uv = vec2(0,0);\r\n"
             + "float AddSub(float a, float b) {\r\n"
                + "if (a >= 0.5) { return a + b; }\r\n"
                + "else { return b - a; }\r\n}\r\n"
             + "vec4 BlendColors(float blendIdx, vec4 c1, vec4 c2) {\r\n"
                + "vec4 fc = vec4(0);\r\n"
                + "blendIdx = floor(blendIdx);\r\n"
                + "if (blendIdx <= 0) { fc.rgb = c1.rgb + c2.rgb * (1.0 - clamp(c1.a, 0, 1));  fc.a = c1.a + c2.a; }\r\n"
                + "else if(blendIdx <= 1) { fc.rgb = c1.rgb + c2.rgb; fc.a = c1.a + c2.a; }\r\n"
                + "else if(blendIdx <= 2) { fc.rgb = vec3(max(c1.r, c2.r), max(c1.g, c2.g), max(c1.b, c2.b)); fc.a = c1.a + c2.a; }\r\n"
                + "else if(blendIdx <= 3) { fc.rgb = vec3(AddSub(c1.r, c2.r), AddSub(c1.g, c2.g), AddSub(c1.b, c2.b)); fc.a = c1.a + c2.a; }\r\n"
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
                fragMain += $"float cclamp = {clampFuncName}();\r\n";
            }
            else
            {
                fragMain += "float cclamp = Clamp;\r\n";
            }

            fragMain += "float sina = sin(angle);\r\n"
                        + "float cosa = cos(angle);\r\n";

            //calculate new scale, rotation, etc
            fragMain += "ivec2 p1 = ivec2(i_pos.x - pivotPoint.x * inWidth, i_pos.y - pivotPoint.y * inHeight);\r\n"
                        + "p1 = ivec2(p1.x * cosa - p1.y * sina, p1.x * sina + p1.y * cosa);\r\n"
                        + "p1 = ivec2(p1.x / scale.x, p1.y / scale.y);\r\n"
                        + "p1 = ivec2(p1.x + pivotPoint.x * inWidth, p1.y + pivotPoint.y * inHeight);\r\n";

            fragMain += "vec4 c1 = texelFetch(Input0, p1, 0);\r\n"
                        + "if ((p1.x < 0 || p1.y < 0 || p1.x >= inWidth || p1.y >= inHeight) && cclamp > 0) { c1 = vec4(0); }\r\n";

            fragMain += "ivec2 finalpos = c_pos + ivec2(trans);\r\n";
            fragMain += "if (finalpos.x >= size.x && cclamp == 0) { finalpos.x = int(mod(finalpos.x, size.x)); }\r\n"
                       + "else if(finalpos.x < 0 && cclamp == 0) { finalpos.x = int(mod(size.x + finalpos.x, size.x)); }\r\n"
                       + "if (finalpos.y >= size.y && cclamp == 0) { finalpos.y = int(mod(finalpos.y, size.y)); }\r\n"
                       + "else if(finalpos.y < 0 && cclamp == 0) { finalpos.y = int(mod(size.y + finalpos.y, size.y)); }\r\n";

            fragMain += "vec4 c2 = imageLoad(_out_put, finalpos);\r\n";

            //calculate lumin
            fragMain += "float r1 = rand(vec2(luminRand + RandomSeed + iteration, luminRand + RandomSeed + iteration)) * luminRand;\r\n"
                     + "float flum = min(1, max(0, lumin + r1));\r\n"
                     + "c1.rgb = c1.rgb * flum;\r\n";

            //blending now + clamp!
            fragMain += "vec4 fc = clamp(BlendColors(blendIdx, c1, c2), vec4(0), vec4(1));\r\n";

            //store pixel and close main
            fragMain += "imageStore(_out_put, finalpos, fc);\r\n}\r\n";

            //Log.Debug(frag + fragMain);
            shader = GLShaderCache.CompileCompute(frag + fragMain);

            if (shader != null)
            {
                rebuildShader = false;
            }

            if (Graph.Graph.ShaderLogging)
            {
                Log.Debug(frag + fragMain);
            }
        }

        public override void TryAndProcess()
        {
            NeedsUpdate();
            GetParams();
            GetParameterCode();
            GetUniformValues();
            BuildShader();
            Process();
        }

        protected virtual void SetUniform(string k, object value, NodeType type)
        {
            if (isDisposing) return;
            if (value == null || shader == null) return;

            try
            {
                if (type == NodeType.Bool)
                {
                    shader.SetUniform(k, value.ToBool() ? 1.0f : 0.0f);
                }
                else if (type == NodeType.Float)
                {
                    shader.SetUniform(k, value.ToFloat());
                }
                else if (type == NodeType.Float2)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Vector2 vec2 = mv.ToVector2();
                        shader.SetUniform2(k, ref vec2);
                    }
                }
                else if (type == NodeType.Float3)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Vector3 vec3 = mv.ToVector3();
                        shader.SetUniform3(k, ref vec3);
                    }
                }
                else if (type == NodeType.Float4 || type == NodeType.Color || type == NodeType.Gray)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Vector4 vec4 = mv.ToVector4();
                        shader.SetUniform4(k, ref vec4);
                    }
                }
                else if(type == NodeType.Matrix)
                {
                    if (value is Matrix4)
                    {
                        Matrix4 m = (Matrix4)value;
                        shader.SetUniformMatrix4(k, ref m);
                    }
                    else if(value is Matrix3)
                    {
                        Matrix3 m = (Matrix3)value;
                        shader.SetUniformMatrix3(k, ref m);
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
            if (isDisposing) return;
            if (quadsConnected == 0) return;
            if (shader == null) return;

            GLTexture2D i1 = null;
            GLTexture2D i2 = null;
            GLTexture2D i3 = null;
            GLTexture2D i4 = null;

            if (q1.HasInput) i1 = (GLTexture2D)q1.Reference.Data;
            if (q2.HasInput) i2 = (GLTexture2D)q2.Reference.Data;
            if (q3.HasInput) i3 = (GLTexture2D)q3.Reference.Data;
            if (q4.HasInput) i4 = (GLTexture2D)q4.Reference.Data;

            if (quadsConnected == 0 || pmaxIter == 0) return;

            CreateBufferIfNeeded();

            buffer.Bind();

            IGL.Primary.ClearTexImage(buffer.Id, (int)PixelFormat.Rgba, (int)PixelType.Float);

            GLTexture2D.Unbind();

            //before we bind this shader we need to collect from the other shaders first
            //if it is a function
            foreach (string k in uniformParams.Keys)
            {
                ParameterValue v = uniformParams[k] as ParameterValue;

                if (v == null) continue;

                object value = v.Value;

                if (!v.IsFunction()) continue;

                //we ignore functions on the FX node
                //as we have already taken them into account
                //for all the FX node variables
                Function temp = value as Function;

                if (temp.ParentNode == this) continue;

                if (temp.BuildAsShader)
                {
                    temp.ComputeResult();
                }
                else
                {
                    temp.TryAndProcess();
                }
            }

            shader.Use();
            shader.SetUniform("quadCount", (float)quadsConnected);
            buffer.Bind();
            buffer.BindAsImage(0, true, true);

            List<GLTexture2D> quads = new List<GLTexture2D>();

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
                shader.SetUniform("Rotation", urot);
            }

            if(!clampIsFunc)
            {
                shader.SetUniform("Clamp", uclamp);
            }

            if(!blendIsFunc)
            {
                shader.SetUniform("Blending", ublend);
            }

            if(!translationIsFunc)
            {
                Vector2 v2 = utranslate.ToVector2();
                shader.SetUniform2("Translation", ref v2);
            }

            if(!scaleIsFunc)
            {
                Vector2 v2 = uscale.ToVector2();
                shader.SetUniform2("Scale", ref v2);
            }

            if (!luminIsFunc)
            {
                shader.SetUniform("Luminosity", ulumin);
            }

            if(!luminRandomIsFunc)
            {
                shader.SetUniform("LuminosityRandomness", uluminrand);
            }

            if(!pivotIsFunc)
            {
                shader.SetUniform("PatternPivot", upivot);
            }

            //set other uniform params
            foreach(string k in uniformParams.Keys)
            {
                ParameterValue v = uniformParams[k] as ParameterValue;

                //we ignore anything that isn't a parameter value
                if (v == null) continue;

                object value = v.Value;

                if (v.IsFunction())
                {
                    Function f = value as Function;
                    SetUniform(k, f.Result, v.Type);
                }
                else
                {
                    SetUniform(k, value, v.Type);
                }
            }

            Vector2 rpos = new Vector2(0,0);
            for (int i = 0; i < quads.Count; ++i)
            {
                GLTexture2D target = quads[i];
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
                IGL.Primary.MemoryBarrier((int)MemoryBarrierFlags.AllBarrierBits);
            }

            GLTexture2D.UnbindAsImage(0);
            GLTexture2D.Unbind();
            shader.Unbind();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();
            shader?.Dispose();
            shader = null;
        }

        public class FXNodeData : NodeData
        {
            public int iterations;
            public int rotation;
            public float tx;
            public float ty;
            public float sx;
            public float sy;
            public byte pivot;
            public byte blending;
            public bool clamp;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(iterations);
                w.Write(rotation);
                w.Write(tx);
                w.Write(ty);
                w.Write(sx);
                w.Write(sy);
                w.Write(pivot);
                w.Write(blending);
                w.Write(clamp);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                iterations = r.NextInt();
                rotation = r.NextInt();
                tx = r.NextFloat();
                ty = r.NextFloat();
                sx = r.NextFloat();
                sy = r.NextFloat();
                pivot = r.NextByte();
                blending = r.NextByte();
                clamp = r.NextBool();
            }
        }

        private void FillData(FXNodeData d)
        {
            d.iterations = iterations;
            d.rotation = rotation;
            d.tx = translation.X;
            d.ty = translation.Y;
            d.sx = scale.X;
            d.sy = scale.Y;
            d.pivot = (byte)patternPivot;
            d.blending = (byte)blending;
            d.clamp = clamp;
        }

        private void SetData(FXNodeData d)
        {
            iterations = d.iterations;
            rotation = d.rotation;
            translation = new MVector(d.tx, d.ty);
            scale = new MVector(d.sx, d.sy);
            patternPivot = (FXPivot)d.pivot;
            blending = (FXBlend)d.blending;
            clamp = d.clamp;
        }

        public override void GetBinary(Writer w)
        {
            FXNodeData d = new FXNodeData();
            FillBaseNodeData(d);
            FillData(d);
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            FXNodeData d = new FXNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            SetData(d);
        }

        public override string GetJson()
        {
            FXNodeData d = new FXNodeData();
            FillBaseNodeData(d);
            FillData(d);
            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            FXNodeData d = JsonConvert.DeserializeObject<FXNodeData>(data);
            SetBaseNodeDate(d);
            SetData(d);
        }
    }
}
