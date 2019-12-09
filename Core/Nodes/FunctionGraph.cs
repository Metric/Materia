using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.MathNodes;
using Newtonsoft.Json;
using Materia.Imaging;
using Materia.Shaders;
using Materia.MathHelpers;
using Materia.Nodes.Attributes;
using System.Reflection;
using Materia.Math3D;
using Materia.GLInterfaces;
using NLog;
using Materia.Archive;
using Materia.Textures;
using Materia.Buffers;
using System.Runtime.InteropServices;
using Materia.Nodes.Atomic;
using Materia.Nodes.Helpers;

namespace Materia.Nodes
{
    public class FunctionGraph : Graph
    {
        public const int MAX_NODES_FOR_SHADER = 20;

        protected static GLShaderBuffer shaderBuffer;
        protected static float[] shaderBufferStorage = new float[4];
        protected static IntPtr mappedMemoryLocation;

        protected bool isDirty;
        protected bool modified;

        protected bool variablesModified;

        public new bool Modified
        {
            get
            {
                return modified;
            }
        }

        public string CodeName
        {
            get
            {
                return Name.Replace(" ", "").Replace("-", "_");
            }
        }

        protected List<Node> orderCache = new List<Node>();

        public delegate void OutputNodeSet(Node n);
        public event OutputNodeSet OnOutputSet;
        public event OutputNodeSet OnArgAdded;
        public event OutputNodeSet OnArgRemoved;

        public delegate void FunctionParentSet(FunctionGraph g);
        public event FunctionParentSet OnParentGraphSet;
        public event FunctionParentSet OnParentNodeSet;
        public event FunctionParentSet OnVariablesSet;

        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public const string GLSLHash = "float rand(vec2 co) {\r\n"
                                 + "return fract(sin(dot(co, vec2(12.9898,78.233))) * 43758.5453) * 2.0 - 1.0;\r\n"
                                 + "}\r\n\r\n";

        protected string lastShaderCode;
        protected string lastInternalCode;

        public ExecuteNode Execute { get; set; }

        public Node OutputNode { get; protected set; }

        public IGLProgram Shader { get; protected set; }

        public NodeType ExpectedOutput
        {
            get; set;
        }

        public bool HasExpectedOutput
        {
            get
            {
                var type = GetOutputType();

                if (type == null) return false;

                return (ExpectedOutput & type) != 0;
            }
        }

        public object Result
        {
            get; set;
        }

        protected List<ArgNode> args;
        public List<ArgNode> Args
        {
            get
            {
                return args;
            }
        }

        protected List<CallNode> calls;
        public List<CallNode> Calls
        {
            get
            {
                return calls;
            }
        }

        protected List<ForLoopNode> forLoops;
        public List<ForLoopNode> ForLoops
        {
            get
            {
                return forLoops;
            }
        }

        protected List<SamplerNode> samplers;
        public List<SamplerNode> Samplers
        {
            get
            {
                return samplers;
            }
        }

        public bool BuildAsShader
        {
            get
            {
                return Nodes.Count >= MAX_NODES_FOR_SHADER || Samplers.Count > 0 || ForLoops.Count > 0;
            }
        }

        public new Graph ParentGraph
        {
            get
            {
                return parentGraph;
            }
            set
            {
                parentGraph = value;

                if(OnParentGraphSet != null)
                {
                    OnParentGraphSet.Invoke(this);
                }
                
                if(parentGraph != null)
                {
                    //update randomSeed
                    randomSeed = parentGraph.RandomSeed;
                    SetVar("RandomSeed", randomSeed, NodeType.Float);
                }
            }
        }

        public override Node ParentNode
        {
            get
            {
                return parentNode;
            }
            set
            {
                parentNode = value;

                if(OnParentNodeSet != null)
                {
                    OnParentNodeSet.Invoke(this);
                }

                var g = parentNode.ParentGraph;
                if(g != null)
                {
                    //update randomSeed
                    randomSeed = g.RandomSeed;
                    SetVar("RandomSeed", randomSeed, NodeType.Float);
                }
            }
        }

        /// <summary>
        /// Need to assign to SetVar as well here
        /// on FunctionGraphs that do not use shader code
        /// </summary>
        /// <param name="seed"></param>
        public override void AssignSeed(int seed)
        {
            base.AssignSeed(seed);
            SetVar("RandomSeed", seed, NodeType.Float);
            randomSeed = seed;
        }

        public new int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public new int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }
        
        protected Dictionary<string, GraphParameterValue> uniforms; 

        public FunctionGraph(string name, int w = 256, int h = 256) : base(name, w, h)
        {
            samplers = new List<SamplerNode>();
            forLoops = new List<ForLoopNode>();
            uniforms = new Dictionary<string, GraphParameterValue>();
            orderCache = new List<Node>();
            calls = new List<CallNode>();
            args = new List<ArgNode>();
            Name = name;
            isDirty = true;
            SetBaseVars();
        }

        public virtual void SetAllVars()
        {
            Variables.Clear();
            SetBaseVars();
            Graph g = parentNode != null ? parentNode.ParentGraph : parentGraph;
            SetParentGraphVars(g);
            SetParentNodeVars(g);
            OnVariablesSet?.Invoke(this);
        }

        public virtual void SetBaseVars()
        {
            SetVar("PI", 3.14159265359f, NodeType.Float);
            SetVar("Rad2Deg", (180.0f / 3.14159265359f), NodeType.Float);
            SetVar("Deg2Rad", (3.14159265359f / 180.0f), NodeType.Float);
            SetVar("RandomSeed", randomSeed, NodeType.Float);

            //just set these so they are available
            //as a drop down selection
            SetVar("pos", new MVector(0, 0), NodeType.Float2);

            if (parentNode != null)
            {
                SetVar("size", new MVector(parentNode.Width, parentNode.Height), NodeType.Float2);
            }
            else if(parentGraph != null)
            {
                SetVar("size", new MVector(parentGraph.Width, parentGraph.Height), NodeType.Float2);
            }
            else
            {
                SetVar("size", new MVector(0,0), NodeType.Float2);
            }

            //just make these available to all
            //graphs even if they do not apply
            //since custom functions can use these as well
            //these are all related to FX node specific
            //variables that are helpful
            SetVar("iteration", 0, NodeType.Float);
            SetVar("maxIterations", 0, NodeType.Float);
            SetVar("w_pos", new MVector(0, 0), NodeType.Float2);
            SetVar("quad", 0, NodeType.Float);

            foreach (ArgNode arg in args)
            {
                if (arg == null) continue;
                SetVar(arg.InputName, 0, arg.InputType);
            }
        }

        public override void Schedule(Node n)
        {
            Updated();
        }

        protected override void Updated()
        {
            base.Updated();

            modified = true;
        }

        public override bool Add(Node n)
        {
            if(n is ExecuteNode && Execute == null)
            {
                if(base.Add(n))
                {
                    Execute = n as ExecuteNode;
                    modified = true;
                    isDirty = true;
                    return true;
                }

                return false;
            }
            else if(n is ExecuteNode && Execute != null)
            {
                return false;
            }
            else if(n is ArgNode)
            {
                if(base.Add(n))
                {
                    args.Add(n as ArgNode);
                    OnArgAdded?.Invoke(n);
                    modified = true;
                    isDirty = true;
                    return true;
                }

                return false;
            }
            else if(n is CallNode)
            {
                if(base.Add(n))
                {
                    calls.Add(n as CallNode);
                    modified = true;
                    isDirty = true;
                    return true;
                }

                return false;
            }
            else if(n is ForLoopNode)
            {
                if (base.Add(n))
                {
                    forLoops.Add(n as ForLoopNode);
                    modified = true;
                    isDirty = true;
                    return true;
                }

                return false;
            }
            else if(n is SamplerNode)
            {
                if (base.Add(n))
                {
                    samplers.Add(n as SamplerNode);
                    modified = true;
                    isDirty = true;
                    return true;
                }

                return false;
            }
            else
            {
                if(base.Add(n))
                {
                    modified = true;
                    isDirty = true;
                    return true;
                }
            }

            return false;
        }

        public override void Remove(Node n)
        {
            if(Execute == n)
            {
                Execute = null;
            }

            if(n is ArgNode)
            {
                args.Remove(n as ArgNode);
                OnArgRemoved?.Invoke(n);
            }
            else if(n is CallNode)
            {
                calls.Remove(n as CallNode);
            } 
            else if(n is SamplerNode)
            {
                samplers.Remove(n as SamplerNode);
            }
            else if(n is ForLoopNode)
            {
                forLoops.Remove(n as ForLoopNode);
            }

            base.Remove(n);

            isDirty = true;
            modified = true;
        }

        public NodeType? GetOutputType()
        {
            if (OutputNode == null) return null;

            if (OutputNode is ExecuteNode) return null;

            if (OutputNode.Outputs.Count > 1)
            { 
                NodeOutput op = OutputNode.Outputs[1];

                return op.Type;
            }
            else if(OutputNode.Outputs.Count == 1)
            {
                NodeOutput op = OutputNode.Outputs[0];
                return op.Type;
            }

            return null;
        }

        public virtual string GetFunctionShaderCode()
        {
            if (OutputNode == null)
            {
                return "";
            }

            List<Node> ordered = OrderNodesForShader();

            //ensure output type is properly updated
            for (int i = 0; i < ordered.Count; ++i)
            {
                (ordered[i] as MathNode).UpdateOutputType();
                if (ordered[i] == OutputNode) break;
            }

            int count = 0;
            string frag = "";

            NodeType? outtype = GetOutputType();

            if (outtype == null)
            {
                return "";
            }

            if(outtype.Value == NodeType.Float4 
                || outtype.Value == NodeType.Color 
                || outtype.Value == NodeType.Gray)
            {
                frag += "vec4 ";
            }
            else if(outtype.Value == NodeType.Float3)
            {
                frag += "vec3 ";
            }
            else if(outtype.Value == NodeType.Float2)
            {
                frag += "vec2 ";
            }
            else if(outtype.Value == NodeType.Float)
            {
                frag += "float ";
            }
            else if(outtype.Value == NodeType.Bool)
            {
                frag += "float ";
            }
            else if(outtype.Value == NodeType.Matrix)
            {
                frag += "mat4 ";
            }
            else
            {
                return "";
            }

            frag += Name.Replace(" ", "").Replace("-", "_") + "(";

            count = args.Count;
            for(int i = 0; i < count; ++i)
            {
                ArgNode a = args[i];

                if (a.InputType == NodeType.Float)
                {
                    frag += "float " + a.InputName + ",";
                }
                else if (a.InputType == NodeType.Float2)
                {
                    frag += "vec2 " + a.InputName + ",";
                }
                else if (a.InputType == NodeType.Float3)
                {
                    frag += "vec3 " + a.InputName + ",";
                }
                else if (a.InputType == NodeType.Float4 || a.InputType == NodeType.Color || a.InputType == NodeType.Gray)
                {
                    frag += "vec4 " + a.InputName + ",";
                }
                else if (a.InputType == NodeType.Bool)
                {
                    frag += "float " + a.InputName + ",";
                }
                else if(a.InputType == NodeType.Matrix)
                {
                    frag += "mat4 " + a.InputName + ",";
                }
            }

            if (args.Count > 0)
            {
                frag = frag.Substring(0, frag.Length - 1) + ") {\r\n";
            }
            else
            {
                frag += ") {\r\n";
            }

            string intern = lastInternalCode = GetInternalShaderCode(ordered, frag, true);

            if (string.IsNullOrEmpty(intern))
            {
                return "";
            }

            frag = intern + "}\r\n\r\n";

            return frag;
        }

        protected List<Node> TravelBranch(Node parent, HashSet<Node> seen)
        {
            List<Node> forward = new List<Node>();
            Queue<Node> queue = new Queue<Node>();

            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                int count = 0;
                Node n = queue.Dequeue();

                if (seen.Contains(n))
                {
                    continue;
                }

                seen.Add(n);

                count = n.Inputs.Count;
                for (int i = 0; i < count; ++i)
                {
                    NodeInput op = n.Inputs[i];

                    if (op.HasInput)
                    {
                        bool nodeHasExecute = op.Reference.Node.Outputs.Find(m => m.Type == NodeType.Execute) != null;

                        if (!nodeHasExecute)
                        {
                            //we don't trigger seen as
                            //it may be shared by another node
                            //further up the chain
                            //this type of node can only be
                            //one of two node types: Get Var
                            //and Constant types
                            //everything else requires
                            //an execute flow
                            forward.Add(op.Reference.Node);
                        }
                    }
                }

                forward.Add(n);

                //prevent going past outputnode
                //if there is is one
                //this saves some time
                //in case the graphs have
                //nodes after the output
                //for some reason
                if (n == OutputNode)
                {
                    continue;
                }

                if (n.Outputs.Count > 0)
                {
                    int i = 0;
                    count = n.Outputs.Count;
                    for (i = 0; i < count; ++i)
                    {
                        NodeOutput op = n.Outputs[i];

                        //we don't care about the actual for loop internals at the momemnt
                        //as each for loop will handle it
                        if (n is ForLoopNode && i == 0)
                        {
                            continue;
                        }

                        if (op.Type == NodeType.Execute)
                        {
                            if (op.To.Count > 1)
                            {
                                //we use recursion if there are multiple links
                                //from one output
                                //otherwise we queue up in queue
                                //and proceed in order
                                int count2 = op.To.Count;
                                for (int j = 0; j < count2; ++j)
                                {
                                    NodeInput t = op.To[j];
                                    var branch = TravelBranch(t.Node, seen);
                                    forward.AddRange(branch);
                                }
                            }
                            else if (op.To.Count > 0)
                            {
                                queue.Enqueue(op.To[0].Node);
                            }
                        }
                    }
                }
            }

            return forward;
        }

        protected List<Node> OrderNodesForShader()
        {
            //is dirty is only for order node caches
            //where as modified is for shader rebuild
            if (isDirty || orderCache.Count == 0)
            {
                Stack<Node> reverse = new Stack<Node>();
                Stack<Node> stack = new Stack<Node>();
                List<Node> forward = new List<Node>();

                //oops forgot to check this!
                if (OutputNode == null)
                {
                    return forward;
                }

                if (Execute != null)
                {
                    stack.Push(Execute);
                }

                //If we do not have an execute node
                //then fall back to old method
                //of starting from output
                //to find first node
                if (stack.Count == 0)
                {
                    reverse.Push(OutputNode);

                    while (reverse.Count > 0)
                    {
                        Node n = reverse.Pop();
                        stack.Push(n);

                        for (int i = 0; i < n.Inputs.Count; ++i)
                        {
                            NodeInput op = n.Inputs[i];
                            if (op.HasInput)
                            {
                                if (op.Type == NodeType.Execute)
                                {
                                    reverse.Push(op.Reference.Node);
                                }
                            }
                        }
                    }
                }

                HashSet<Node> seen = new HashSet<Node>();
                var sc = stack.ToList();
                stack.Clear();

                var branch = TravelBranch(sc[0], seen);
                forward.AddRange(branch);

                //remove execute from it
                //so it does not interfere with the shader generation
                //etc
                if (Execute != null)
                {
                    forward.Remove(Execute);
                }

                orderCache = forward;
                isDirty = false;

                return forward;
            }

            return orderCache;
        }

        protected string GetInternalShaderCode(List<Node> nodes, string frag = "", bool asFunc = false, bool asBufferShader = false)
        {
            if (OutputNode == null)
            {
                return "";
            }

            NodeType? type = GetOutputType();

            if (type == null)
            {
                return "";
            }

            int count = nodes.Count;
            for(int i = 0; i < count; ++i)
            {
                var n = nodes[i] as MathNode;

                n.UpdateOutputType();
                string d = n.GetShaderPart(frag);

                if (string.IsNullOrEmpty(d))
                {
                    return "";
                }

                if (frag.IndexOf(d) == -1)
                {
                    frag += d;
                }
            }

            type = GetOutputType();

            var last = OutputNode as MathNode;

            int endIndex = 1;

            //constants and get vars do not have an execute node
            //so their first output starts at 0, rather than 1
            if(last is FloatConstantNode || last is Float2ConstantNode 
                || last is Float3ConstantNode || last is Float4ConstantNode
                || last is BooleanConstantNode || last is GetVarNode)
            {
                endIndex = 0;
            }

            if (!asFunc)
            {
                if (asBufferShader)
                {
                    if(type.Value == NodeType.Float4 || type.Value == NodeType.Color || type.Value == NodeType.Gray)
                    {
                        frag += "vec4 _f_shader_output = " + last.ShaderId + endIndex.ToCodeString() + ";\r\n";
                        frag += "_out_put[0] = _f_shader_output.x;\r\n"
                             + "_out_put[1] = _f_shader_output.y;\r\n"
                             + "_out_put[2] = _f_shader_output.z;\r\n"
                             + "_out_put[3] = _f_shader_output.w;\r\n";
                    }
                    else if(type.Value == NodeType.Bool || type.Value == NodeType.Float)
                    {
                        frag += "vec4 _f_shader_output = vec4(" + last.ShaderId + endIndex.ToCodeString() + ");\r\n";
                        frag += "_out_put[0] = _f_shader_output.x;\r\n"
                            + "_out_put[1] = _f_shader_output.y;\r\n"
                            + "_out_put[2] = _f_shader_output.z;\r\n"
                            + "_out_put[3] = _f_shader_output.w;\r\n";
                    }
                    else if(type.Value == NodeType.Float2)
                    {
                        frag += "vec2 _f_shader_output = " + last.ShaderId + endIndex.ToCodeString() + ";\r\n";
                        frag += "_out_put[0] = _f_shader_output.x;\r\n"
                            + "_out_put[1] = _f_shader_output.y;\r\n";
                    }
                    else if(type.Value == NodeType.Float3)
                    {
                        frag += "vec3 _f_shader_output = " + last.ShaderId + endIndex.ToCodeString() + ";\r\n";
                        frag += "_out_put[0] = _f_shader_output.x;\r\n"
                           + "_out_put[1] = _f_shader_output.y;\r\n"
                           + "_out_put[2] = _f_shader_output.z;\r\n";
                    }
                }
                else
                {

                    frag += "imageStore(_out_put, opos, vec4(" + last.ShaderId + endIndex.ToCodeString() + "));\r\n";
                }
            }
            else
            {
                frag += "return " + last.ShaderId + endIndex.ToCodeString() + ";\r\n";
            }

            return frag;
        }

        public virtual void PrepareShader(GraphPixelType type = GraphPixelType.RGBA32F, bool asBufferShader = true)
        {
            uniforms.Clear();
            lastShaderCode = null;

            List<Node> ordered = OrderNodesForShader();

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

            string sizePart = "vec2 size = vec2(0);\r\n";

            if (parentNode != null)
            {
                Node n = parentNode;
                int w = n.Width;
                int h = n.Height;

                sizePart = "vec2 size = vec2(" + w.ToCodeString() + "," + h.ToCodeString() + ");\r\n";
            }
            else if (parentGraph != null)
            {
                int w = parentGraph.Width;
                int h = parentGraph.Height;

                sizePart = "vec2 size = vec2(" + w.ToCodeString() + "," + h.ToCodeString() + ");\r\n";
            }

            string frag = "";
            if (asBufferShader)
            {
                frag = "#version 430 core\r\n"
                             + "layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;\r\n"
                             + "layout(std430, binding = 0) writeonly buffer Pos{\r\n"
                             + " float _out_put[];\r\n"
                             + "};\r\n"
                             + "uniform sampler2D Input0;\r\n"
                             + "uniform sampler2D Input1;\r\n"
                             + "uniform sampler2D Input2;\r\n"
                             + "uniform sampler2D Input3;\r\n"
                             + sizePart
                             + "vec2 uv = vec2(0);\r\n"
                             + "vec2 pos = uv = vec2(gl_GlobalInvocationID.xy) / size;\r\n"
                             + "ivec2 opos = ivec2(gl_GlobalInvocationID.xy);\r\n"
                             + "const float PI = 3.14159265359;\r\n"
                             + "const float Rad2Deg = (180.0 / PI);\r\n"
                             + "const float Deg2Rad = (PI / 180.0);\r\n"
                             + "uniform float RandomSeed = " + randomSeed.ToCodeString() + ";\r\n"
                             + GLSLHash
                             + GetParentGraphShaderParams(true, uniforms);
            }
            else
            {
                frag = "#version 430 core\r\n"
                             + "layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;\r\n"
                             + $"layout({outputType}, binding = 0) uniform writeonly image2D _out_put;\r\n"
                             + "uniform sampler2D Input0;\r\n"
                             + "uniform sampler2D Input1;\r\n"
                             + "uniform sampler2D Input2;\r\n"
                             + "uniform sampler2D Input3;\r\n"
                             + sizePart
                             + "vec2 uv = vec2(0);\r\n"
                             + "vec2 pos = uv = vec2(gl_GlobalInvocationID.xy) / size;\r\n"
                             + "ivec2 opos = ivec2(gl_GlobalInvocationID.xy);\r\n"
                             + "const float PI = 3.14159265359;\r\n"
                             + "const float Rad2Deg = (180.0 / PI);\r\n"
                             + "const float Deg2Rad = (PI / 180.0);\r\n"
                             + "uniform float RandomSeed = " + randomSeed.ToCodeString() + ";\r\n"
                             + GLSLHash
                             + GetParentGraphShaderParams(true, uniforms);
            }

            string previousCalls = "";
            Stack<CallNode> finalStack = GetFullCallStack();

            while (finalStack.Count > 0)
            {
                CallNode m = finalStack.Pop();

                string s = m.GetFunctionShaderCode();

                if (string.IsNullOrEmpty(s))
                {
                    return;
                }

                if(previousCalls.IndexOf(s) == -1)
                {
                    previousCalls += s;
                }
            }

            frag += previousCalls + "void main() {\r\n";

            string intern = lastInternalCode = GetInternalShaderCode(ordered, "", false, asBufferShader);

            if (string.IsNullOrEmpty(intern))
            {
                return;
            }

            frag += intern + "}";

            //one last check to verify the output actually has the expected output
            if (!HasExpectedOutput)
            {
                return;
            }

            lastShaderCode = frag;
        }

        public virtual Stack<CallNode> GetFullCallStack()
        {
            int count = calls.Count;
            Queue<CallNode> stack = new Queue<CallNode>();
            Stack<CallNode> finalStack = new Stack<CallNode>();
            HashSet<string> seenCalls = new HashSet<string>();

            for (int i = 0; i < count; ++i)
            {
                CallNode m = calls[i];
                if (m.selectedFunction == null || m.selectedFunction == this
                    || seenCalls.Contains(m.selectedFunction.CodeName))
                {
                    continue;
                }
                seenCalls.Add(m.selectedFunction.CodeName);
                stack.Enqueue(m);
            }

            while (stack.Count > 0)
            {
                CallNode m = stack.Dequeue();
                finalStack.Push(m);
                List<CallNode> nextCalls = m.selectedFunction.calls;
                for (int i = 0; i < nextCalls.Count; ++i)
                {
                    CallNode next = nextCalls[i];
                    if (next.selectedFunction == null || next.selectedFunction == this
                        || seenCalls.Contains(next.selectedFunction.CodeName))
                    {
                        continue;
                    };
                    seenCalls.Add(next.selectedFunction.CodeName);
                    stack.Enqueue(next);
                }
            }

            return finalStack;
        }

        public virtual bool BuildShader()
        {
            if (OutputNode == null || string.IsNullOrEmpty(lastShaderCode))
            {
                return false;
            }

            if(Shader != null)
            {
                Shader.Release();
                Shader = null;
            }

            if (!HasExpectedOutput)
            {
                return false;
            }

            if (ShaderLogging)
            {
                Log.Debug(lastShaderCode);
            }

            Shader = Material.Material.CompileCompute(lastShaderCode);

            if (Shader == null)
            {
                lastInternalCode = null;
                lastShaderCode = null;
                return false;
            }

            lastInternalCode = null;
            lastShaderCode = null;
            return true;
        }

        protected virtual void SetUniform(string k, object value, NodeType type)
        {
            if (value == null) return;

            try
            {
                if (type == NodeType.Bool)
                {
                    Shader.SetUniform(k, Utils.ConvertToBool(value) ? 1.0f : 0.0f);
                }
                else if (type == NodeType.Float)
                {
                    Shader.SetUniform(k, Utils.ConvertToFloat(value));
                }
                else if (type == NodeType.Float2)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Math3D.Vector2 vec2 = new Math3D.Vector2(mv.X, mv.Y);
                        Shader.SetUniform2(k, ref vec2);
                    }
                }
                else if (type == NodeType.Float3)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Math3D.Vector3 vec3 = new Math3D.Vector3(mv.X, mv.Y, mv.Z);
                        Shader.SetUniform3(k, ref vec3);
                    }
                }
                else if (type == NodeType.Float4 || type == NodeType.Color || type == NodeType.Gray)
                {
                    if (value is MVector)
                    {
                        MVector mv = (MVector)value;
                        Math3D.Vector4 vec4 = new Math3D.Vector4(mv.X, mv.Y, mv.Z, mv.W);
                        Shader.SetUniform4F(k, ref vec4);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public virtual void PrepareUniforms()
        {
            foreach (string k in uniforms.Keys)
            {
                GraphParameterValue v = uniforms[k];

                object value = v.Value;

                if (v.IsFunction())
                {
                    FunctionGraph temp = value as FunctionGraph;

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

        public virtual void AssignUniforms()
        {
            if (Shader == null) return;

            //set other uniform params
            foreach (string k in uniforms.Keys)
            {
                GraphParameterValue v = uniforms[k];

                object value = v.Value;

                if (v.IsFunction())
                {
                    FunctionGraph temp = value as FunctionGraph;
                    value = temp.Result;
                }

                SetUniform(k, value, v.Type);
            }

            SetUniform("RandomSeed", randomSeed, NodeType.Float);
        }

        /// <summary>
        /// This is used for param computation
        /// </summary>
        public virtual void ComputeResult()
        {
            if (Shader == null || modified)
            {
                try
                {
                    PrepareShader(GraphPixelType.RGB32F);
                    if(BuildShader() && Shader != null)
                    {
                        modified = false;
                    }
                    Log.Debug("Created shader for: " + Name);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            if (Shader == null) return;

            if(shaderBuffer == null || shaderBuffer.Id == 0)
            {
                shaderBufferStorage = new float[4];
                shaderBuffer = new GLShaderBuffer();
                shaderBuffer.Bind();
                shaderBuffer.Storage(shaderBufferStorage.Length);
                mappedMemoryLocation = shaderBuffer.MapRange(BufferAccessMask.MapCoherentBit | BufferAccessMask.MapReadBit | BufferAccessMask.MapPersistentBit);
                if (mappedMemoryLocation.ToInt64() == 0)
                {
                    shaderBuffer.Unmap();
                    shaderBuffer.Release();
                    shaderBuffer.Unbind();
                    return;
                }
                shaderBuffer.Unbind();
            }


            Shader.Use();
            shaderBuffer.Bind();

            AssignUniforms();

            IGL.Primary?.DispatchCompute(1, 1, 1);
            IGL.Primary?.Finish();

            Marshal.Copy(mappedMemoryLocation, shaderBufferStorage, 0, shaderBufferStorage.Length);
            shaderBuffer.Unbind();

            var type = GetOutputType();
            if (type == NodeType.Bool)
            {
                Result = shaderBufferStorage[0] <= 0 ? false : true;
            }
            else if(type == NodeType.Float)
            {
                Result = shaderBufferStorage[0];
            }
            else if(type == NodeType.Float2)
            {
                Result = new MVector(shaderBufferStorage[0], shaderBufferStorage[1]);
            }
            else if(type == NodeType.Float3)
            {
                Result = new MVector(shaderBufferStorage[0], shaderBufferStorage[1], shaderBufferStorage[2]);
            }
            else if(type == NodeType.Float4 || type == NodeType.Gray || type == NodeType.Color)
            {
                Result = new MVector(shaderBufferStorage[0], shaderBufferStorage[1], shaderBufferStorage[2], shaderBufferStorage[3]);               
            }
            else
            {
                Result = null;
            }

            Shader.Unbind();
        }

        public virtual void SetOutputNode(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                OutputNode = null;
                if(OnOutputSet != null)
                {
                    OnOutputSet.Invoke(null);
                }
                return;
            }

            Node n = null;
            if(NodeLookup.TryGetValue(id, out n))
            {
                OutputNode = n;
                if (OnOutputSet != null)
                {
                    OnOutputSet.Invoke(n);
                }
            }
        }

        //a function graph does not allow embedded graph instances
        //and the type must be coming from MathNodes path
        public override Node CreateNode(string type)
        {
            if (type.Contains("MathNodes") && !type.Contains("/") && !type.Contains("\\"))
            {
                MathNode n = base.CreateNode(type) as MathNode;

                if(n == null)
                {
                    return null;
                }

                n.AssignParentNode(parentNode);
                n.AssignParentGraph(this);
                return n;
            }
            else if(type.Contains("Items") && !type.Contains("/") && !type.Contains("\\"))
            {
                return base.CreateNode(type);
            }

            return null;
        }

        public void AssignParentGraph(Graph g)
        {
            parentGraph = g;
            SetParentGraphVars(parentGraph);
            OnVariablesSet?.Invoke(this);
        }

        public override void AssignParentNode(Node n)
        {
            base.AssignParentNode(n);

            Graph g = null;
            if (n != null) g = n.ParentGraph;

            SetParentNodeVars(g);
            SetParentGraphVars(g);
            OnVariablesSet?.Invoke(this);
        }

        public override void ResizeWith(int width, int height)
        {
            //do nothing in this graph
        }

        public static string BuildShaderParam(GraphParameterValue param, bool isUniform = false, bool isCustom = false)
        {
            string type = "";

            if (param.Type == NodeType.Bool)
            {
                type = isUniform ? "uniform float " : "float ";
            }
            else if (param.Type == NodeType.Float)
            {
                type = isUniform ? "uniform float " : "float ";
            }
            else if (param.Type == NodeType.Color || param.Type == NodeType.Float4 || param.Type == NodeType.Gray)
            {
                type = isUniform ? "uniform vec4 " : "vec4 ";
            }
            else if (param.Type == NodeType.Float2)
            {
                type = isUniform ? "uniform vec2 " : "vec2 ";
            }
            else if (param.Type == NodeType.Float3)
            {
                type = isUniform ? "uniform vec3 " : "vec3 ";
            }
            else
            {
                return "";
            }

            string result = BuildShaderParamValue(param);

            if (string.IsNullOrEmpty(result)) return "";

            string s1 = type + (isCustom ? param.CustomCodeName : param.CodeName) + " = ";
            return s1 + result;
        }

        public static void BuildShaderParam(GraphParameterValue param, StringBuilder builder, bool isUniform = false, bool isCustom = false)
        {
            string type = "";

            if (param.Type == NodeType.Bool)
            {
                type = isUniform ? "uniform float " : "float ";
            }
            else if (param.Type == NodeType.Float)
            {
                type = isUniform ? "uniform float " : "float ";
            }
            else if (param.Type == NodeType.Color || param.Type == NodeType.Float4 || param.Type == NodeType.Gray)
            {
                type = isUniform ? "uniform vec4 " : "vec4 ";
            }
            else if (param.Type == NodeType.Float2)
            {
                type = isUniform ? "uniform vec2 " : "vec2 ";
            }
            else if (param.Type == NodeType.Float3)
            {
                type = isUniform ? "uniform vec3 " : "vec3 ";
            }
            else
            {
                return;
            }

            string result = BuildShaderParamValue(param);

            if (string.IsNullOrEmpty(result)) return;

            string s1 = type + (isCustom ? param.CustomCodeName : param.CodeName) + " = " ;

            if (builder.ToString().Contains(s1)) return;

            builder.Append(s1);
            builder.Append(result);
        }

        protected static string BuildShaderParamValue(GraphParameterValue param)
        {
            object value = param.Value;

            //if a param is a function
            //it is considered a shared uniform param
            //and thus we just do place holders
            //as these are calculated 
            //when the node processes
            if(param.IsFunction())
            {
                if (param.Type == NodeType.Bool)
                {
                    return "0;\r\n";
                }
                else if(param.Type == NodeType.Float)
                {
                    return "0;\r\n";
                }
                else if (param.Type == NodeType.Float4 || param.Type == NodeType.Gray || param.Type == NodeType.Color)
                {
                    return "vec4(0);\r\n";
                }
                else if (param.Type == NodeType.Float2)
                {
                    return "vec2(0);\r\n";
                }
                else if (param.Type == NodeType.Float3)
                {
                    return "vec3(0);\r\n";
                }

                return "";
            }

            if (param.Type == NodeType.Bool)
            {
                try
                {
                    return (Convert.ToBoolean(value) ? 1 : 0) + ";\r\n";
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Info("Defaulting to false for parameter " + param.Name);
                    return "0;\r\n";
                }
            }
            else if (param.Type == NodeType.Float)
            {
                try
                {
                    return Utils.ConvertToFloat(param.Value).ToCodeString() + ";\r\n";
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    Log.Info("Defaulting to 0 for parameter " + param.Name);
                    return "0;\r\n";
                }
            }
            else if (param.Type == NodeType.Float4 || param.Type == NodeType.Gray || param.Type == NodeType.Color)
            {

                MVector vec = new MVector();

                if (value is MVector)
                {
                    vec = (MVector)value;
                }

                return "vec4(" + vec.X.ToCodeString() + "," + vec.Y.ToCodeString() + "," + vec.Z.ToCodeString() + "," + vec.W.ToCodeString() + ");\r\n";
            }
            else if (param.Type == NodeType.Float2)
            {
                MVector vec = new MVector();

                if (value is MVector)
                {
                    vec = (MVector)value;
                }

                return "vec2(" + vec.X.ToCodeString() + "," + vec.Y.ToCodeString() + ");\r\n";
            }
            else if (param.Type == NodeType.Float3)
            {
                MVector vec = new MVector();

                if (value is MVector)
                {
                    vec = (MVector)value;
                }

                return "vec3(" + vec.X.ToCodeString() + "," + vec.Y.ToCodeString() + "," + vec.Z.ToCodeString() + ");\r\n";
            }

            return "";
        }

        public string GetParentGraphShaderParams(bool isUniform = false, Dictionary<string, GraphParameterValue> seen = null)
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                var p = parentNode != null ? parentNode.ParentGraph : parentGraph;

                if (p != null)
                {
                    int count = p.CustomParameters.Count;
                    for (int i = 0; i < count; ++i)
                    {
                        GraphParameterValue param = p.CustomParameters[i];
                        if (param.Value == this) continue;
                        string paramId = param.CustomCodeName;
                        if (seen != null && seen.ContainsKey(paramId)) continue;
                        BuildShaderParam(param, builder, isUniform, true);
                        if (seen != null) seen.Add(paramId, param);
                    }

                    foreach (var param in p.Parameters.Values)
                    {
                        string paramId = param.CodeName;
                        if (param.Value == this) continue;
                        if (seen != null && seen.ContainsKey(paramId)) continue;
                        BuildShaderParam(param, builder, isUniform);
                        if (seen != null) seen.Add(paramId, param);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Error("There is an infinite function reference loop in promoted graph parameters.");
                return "";
            }

            return builder.ToString();
        }

        protected object GetVar(Dictionary<string, VariableDefinition> vars, string key)
        {
            VariableDefinition vd;
            if (vars.TryGetValue(key, out vd))
            {
                return vd.Value;
            }

            return null;
        }

        protected void SetVar(Dictionary<string, VariableDefinition> vars, string key, object value, NodeType type)
        {

            VariableDefinition vd;

            if (vars.TryGetValue(key, out vd))
            {
                vd.Type = type;
                vd.Value = value;
            }
            else
            {
                vd = new VariableDefinition(value, type);
                vars[key] = vd;
            }
        }

        protected void GetParentGraphVars(Graph g, Dictionary<string, VariableDefinition> vars)
        {
            if (g == null) return;

            try
            {
                //parameters can be function or constant
                foreach (var k in g.Parameters.Keys)
                {
                    var param = g.Parameters[k];
                    if (param.Value == this) continue;
                    if (!param.IsFunction())
                    {
                        SetVar(vars, param.CodeName, param.Value, param.Type);
                    }
                    else
                    {
                        FunctionGraph gf = param.Value as FunctionGraph;
                        SetVar(vars, param.CodeName, gf.Result, param.Type);
                    }
                }

                int count = g.CustomParameters.Count;
                for (int i = 0; i < count; ++i)
                {
                    GraphParameterValue param = g.CustomParameters[i];
                    if (param.Value == this) continue;
                    SetVar(vars, param.CustomCodeName, param.Value, param.Type);
                }
            }
            catch (StackOverflowException e)
            {
                //possible
                Log.Error(e);
                Log.Error("There is an infinite function reference loop in promoted graph parameters.");
            }
        }

        protected void SetParentGraphVars(Graph g)
        {
            if (g == null) return;

            try
            {
                //parameters can be function or constant
                foreach (var k in g.Parameters.Keys)
                {
                    var param = g.Parameters[k];
                    if (param.Value == this) continue;
                    if (!param.IsFunction())
                    {
                        SetVar(param.CodeName, param.Value, param.Type);
                    }
                    else
                    {
                        FunctionGraph gf = param.Value as FunctionGraph;
                        SetVar(param.CodeName, gf.Result, param.Type);
                    }
                }

                int count = g.CustomParameters.Count;
                for(int i = 0; i < count; ++i)
                {
                    GraphParameterValue param = g.CustomParameters[i];
                    if (param.Value == this) continue;
                    SetVar(param.CustomCodeName, param.Value, param.Type);
                }
            }
            catch (StackOverflowException e)
            {
                //possible
                Log.Error(e);
                Log.Error("There is an infinite function reference loop in promoted graph parameters.");
            }
        }

        //conver this code to building as part of shader
        //will readd in the future when
        //these are available via shaders as well
        protected void SetParentNodeVars(Graph g)
        {
            /*try
            {
                if (g == null || parentNode == null) return;

                var props = parentNode.GetType().GetProperties();

                var p = g;

                if (p != null)
                {
                    int count = props.Length;
                    for(int i = 0; i < count; ++i)
                    {
                        var prop = props[i];
                        PromoteAttribute promote = prop.GetCustomAttribute<PromoteAttribute>();
                        EditableAttribute editable = prop.GetCustomAttribute<EditableAttribute>();

                        if(editable == null)
                        {
                            continue;
                        }

                        object v = null;

                        NodeType pType = NodeType.Float;

                        if (p.HasParameterValue(parentNode.Id, prop.Name))
                        {
                            var gp = p.GetParameterRaw(parentNode.Id, prop.Name);
                            if (!gp.IsFunction())
                            {
                                v = gp.Value;
                                pType = gp.Type;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            v = prop.GetValue(parentNode);

                            if(promote != null)
                            {
                                pType = promote.ExpectedType;
                            }
                            else
                            {
                                if(Helpers.Utils.IsNumber(v))
                                {
                                    pType = NodeType.Float;
                                }
                                else if(v != null && v is MVector)
                                {
                                    pType = NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Gray | NodeType.Color;
                                }
                                else if(v != null && v is Vector4)
                                {
                                    pType = NodeType.Float4;
                                }
                                else if(v != null && v is bool)
                                {
                                    pType = NodeType.Bool;
                                }
                                else
                                {
                                    //do not add it
                                    continue;
                                }
                            }
                        }


                        if (v != null)
                        {
                            if (v is Vector4)
                            {
                                Vector4 vec = (Vector4)v;
                                v = new MVector(vec.X, vec.Y, vec.Z, vec.W);
                            }

                            SetVar(prop.Name, v, pType);
                        }
                    }
                }
                else
                {
                    int count = props.Length;
                    for(int i = 0; i < count; ++i)
                    {
                        NodeType pType = NodeType.Float;
                        var prop = props[i];
                        PromoteAttribute promote = prop.GetCustomAttribute<PromoteAttribute>();
                        EditableAttribute editable = prop.GetCustomAttribute<EditableAttribute>();

                        if(editable == null)
                        {
                            continue;
                        }

                        object v = prop.GetValue(parentNode);


                        if(promote != null)
                        {
                            pType = promote.ExpectedType;
                        }
                        else
                        {
                            if (Helpers.Utils.IsNumber(v))
                            {
                                pType = NodeType.Float;
                            }
                            else if (v != null && v is MVector)
                            {
                                pType = NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Gray | NodeType.Color;
                            }
                            else if(v != null && v is Vector4)
                            {
                                pType = NodeType.Float4;
                            }
                            else if(v != null &&  v is bool)
                            {
                                pType = NodeType.Bool;
                            }
                            else
                            {
                                //do not add
                                continue;
                            }
                        }


                        if (v != null)
                        {
                            if (v is Vector4)
                            {
                                Vector4 vec = (Vector4)v;
                                v = new MVector(vec.X, vec.Y, vec.Z, vec.W);
                            }

                            SetVar(prop.Name, v, pType);
                        }
                    }
                }
            }
            catch (StackOverflowException e)
            {
                Log.Error(e);
                //stackoverflow possible if you do a loop of function parameter values
                Log.Error("There is an infinite function reference loop between node parameters");
            }*/
        }

        public override void TryAndProcess()
        {
            SetAllVars();
            List<Node> nodes = OrderNodesForShader();

            for(int i = 0; i < nodes.Count; ++i)
            {
                Node n = nodes[i];
                n.TryAndProcess();
                if (n == OutputNode)
                {
                    NodeOutput output = null;
                    if (n.Outputs.Count == 1)
                    {
                        output = n.Outputs[0];
                    }
                    else if(n.Outputs.Count > 1)
                    {
                        output = n.Outputs[1];
                    }
                    else
                    {
                        break;
                    }

                    Result = output.Data;
                    break;
                }
            }
        }

        public class FunctionGraphData : GraphData
        {
            public string outputNode;
        }

        public override string GetJson()
        {
            FunctionGraphData d = new FunctionGraphData();

            List<string> data = new List<string>();

            int count = Nodes.Count;
            for(int i = 0; i < count; ++i)
            {
                Node n = Nodes[i];
                data.Add(n.GetJson());
            }

            d.name = Name;
            d.nodes = data;
            d.outputs = new List<string>();
            d.inputs = new List<string>();

            d.outputNode = OutputNode != null ? OutputNode.Id : null;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data, MTGArchive archive = null)
        {
            FromJson(data);
        }

        public virtual void FromJson(string data)
        {
            FunctionGraphData d = JsonConvert.DeserializeObject<FunctionGraphData>(data);
            base.FromJson(d);

            Node n = null;

            if (d.outputNode != null)
            {
                NodeLookup.TryGetValue(d.outputNode, out n);
                OutputNode = n;
            }

            //we also want to set vars for argument if we have any
            //so they appear in the dropdown

            foreach(ArgNode arg in args)
            {
                object temp = 0;
                if(arg.InputType == NodeType.Float || arg.InputType == NodeType.Bool)
                {
                    temp = 0;
                }
                else if(arg.InputType == NodeType.Matrix)
                {
                    temp = Matrix4.Identity;
                }
                else
                {
                    temp = new MVector();
                }

                SetVar(arg.InputName, temp, arg.InputType);
            }
        }

        public static void ReleaseShaderBuffer()
        {
            if(shaderBuffer != null)
            {
                if(mappedMemoryLocation.ToInt64() != 0)
                {
                    shaderBuffer.Bind();
                    shaderBuffer.Unmap();
                    shaderBuffer.Unbind();
                    mappedMemoryLocation = IntPtr.Zero;
                }
                shaderBuffer.Release();
                shaderBuffer = null;
            }
        }

        public override void Dispose()
        {
            if(Shader != null)
            {
                Shader.Release();
                Shader = null;
            }

            base.Dispose();
        }
    }
}
