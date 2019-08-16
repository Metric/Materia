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

namespace Materia.Nodes
{
    public class FunctionGraph : Graph
    {
        public delegate void OutputNodeSet(Node n);
        public event OutputNodeSet OnOutputSet;

        public delegate void FunctionParentSet(FunctionGraph g);
        public event FunctionParentSet OnParentGraphSet;
        public event FunctionParentSet OnParentNodeSet;



        private static ILogger Log = LogManager.GetCurrentClassLogger();

        static string GLSLHash = "float rand(vec2 co) {\r\n"
                                 + "return fract(sin(dot(co, vec2(12.9898,78.233))) * 43758.5453) * 2.0 - 1.0;\r\n"
                                 + "}\r\n\r\n";

        protected string lastShaderCode;

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
                if (OutputNode == null) return false;
                if (OutputNode.Outputs == null) return false;
                if (OutputNode.Outputs.Count == 0) return false;

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

        protected Graph parentGraph;
        public Graph ParentGraph
        {
            get
            {
                return parentGraph;
            }
            set
            {
                if(parentGraph != null)
                {
                    parentGraph.OnGraphUpdated -= G_OnGraphUpdated;
                }

                parentGraph = value;

                if(OnParentGraphSet != null)
                {
                    OnParentGraphSet.Invoke(this);
                }
                
                if(parentGraph != null)
                {
                    parentGraph.OnGraphUpdated += G_OnGraphUpdated;
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
                Graph g = TopGraph();

                if(g != null)
                {
                    g.OnGraphUpdated -= G_OnGraphUpdated;
                }

                parentNode = value;

                if(OnParentGraphSet != null)
                {
                    OnParentNodeSet.Invoke(this);
                }

                g = TopGraph();
                if(g != null)
                {
                    g.OnGraphUpdated += G_OnGraphUpdated;
                }
            }
        }

        private void G_OnGraphUpdated(Graph g)
        {
            randomSeed = g.RandomSeed;
            SetVar("RandomSeed", randomSeed, NodeType.Float);
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

        public FunctionGraph(string name, int w = 256, int h = 256) : base(name, w, h, false)
        {
            calls = new List<CallNode>();
            args = new List<ArgNode>();
            Name = name;
            SetVar("PI", 3.14159265359f, NodeType.Float);
            SetVar("Rad2Deg", (180.0f / 3.14159265359f), NodeType.Float);
            SetVar("Deg2Rad", (3.14159265359f / 180.0f), NodeType.Float);
            SetVar("RandomSeed", randomSeed, NodeType.Float);

            //just set these so they are available
            //as a drop down selection
            SetVar("pos", new MVector(0, 0), NodeType.Float2);
            SetVar("size", new MVector(w, h), NodeType.Float2);
        }

        public override bool Add(Node n)
        {
            if(n is ExecuteNode && Execute == null)
            {
                if(base.Add(n))
                {
                    Execute = n as ExecuteNode;
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
                    return true;
                }

                return false;
            }
            else if(n is CallNode)
            {
                if(base.Add(n))
                {
                    calls.Add(n as CallNode);
                    return true;
                }

                return false;
            }
            else
            {
                return base.Add(n);
            }
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
            }
            else if(n is CallNode)
            {
                calls.Remove(n as CallNode);
            }  

            base.Remove(n);
        }

        public override Graph TopGraph()
        {
            Graph p = null;
            //parentGraph takes priority!
            if(parentGraph != null)
            {
                return parentGraph;
            }

            if (ParentNode != null)
            {
                p = ParentNode.ParentGraph;

                while(p != null && p is FunctionGraph)
                {
                    var np = (p as FunctionGraph).parentNode;
                    if(np != null)
                    {
                        p = np.ParentGraph;
                    }
                    else
                    {
                        p = null;
                    }
                }
            }

            return p;
        }

        public NodeType? GetOutputType()
        {
            if (OutputNode == null) return null;

            if (OutputNode.Outputs.Count == 0) return null;

            NodeOutput op = OutputNode.Outputs.Find(m => m.Type != NodeType.Execute);

            return op.Type;
        }

        public virtual string GetFunctionShaderCode()
        {
            if (OutputNode == null)
            {
                return "";
            }

            string otherCalls = "";
            string frag = "";

            List<Node> ordered = OrderNodesForShader();

            //this is in case this function references
            //other functions
            int count = calls.Count;
            for(int i = 0; i < count; i++)
            {
                CallNode m = calls[i];

                //no need to recreate the function
                //if it is a recursive function!
                if (m.selectedFunction == this)
                {
                    continue;
                }

                string s = m.GetFunctionShaderCode();

                if (string.IsNullOrEmpty(s))
                {
                    return "";
                }

                if (otherCalls.IndexOf(s) == -1)
                {
                    otherCalls += s;
                }
            }

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
                frag += "bool ";
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
            for(int i = 0; i < count; i++)
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
                    frag += "bool " + a.InputName + ",";
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

            string intern = GetInternalShaderCode(ordered, frag, true);

            if (string.IsNullOrEmpty(intern))
            {
                return "";
            }

            frag = intern + "}\r\n\r\n";

            return otherCalls + frag;
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
                for(int i = 0; i < count; i++)
                {
                    NodeInput op = n.Inputs[i];

                    if (op.HasInput)
                    {
                        bool nodeHasExecute = op.Input.Node.Outputs.Find(m => m.Type == NodeType.Execute) != null;

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
                            forward.Add(op.Input.Node);
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
                if(n == OutputNode)
                {
                    continue;
                }

                if (n.Outputs.Count > 0)
                {
                    int i = 0;
                    count = n.Outputs.Count;
                    for(i = 0; i < count; i++)
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
                                for(int j = 0; j < count2; j++)
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
            Stack<Node> reverse = new Stack<Node>();
            Stack<Node> stack = new Stack<Node>();
            List<Node> forward = new List<Node>();

            //oops forgot to check this!
            if(OutputNode == null)
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

                    for (int i = 0; i < n.Inputs.Count; i++)
                    {
                        NodeInput op = n.Inputs[i];
                        if (op.HasInput)
                        {
                            if (op.Type == NodeType.Execute)
                            {
                                reverse.Push(op.Input.Node);
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

            return forward;
        }

        public void UpdateOutputTypes()
        {
            var nodes = OrderNodesForShader();

            int count = nodes.Count;
            for(int i = 0; i < count; i++)
            {
                Node n = nodes[i];
                (n as MathNode).UpdateOutputType();
            }
        }

        protected string GetInternalShaderCode(List<Node> nodes, string frag = "", bool asFunc = false)
        {
            if (OutputNode == null)
            {
                return "";
            }

            string sizePart = "vec2 size = vec2(0);\r\n";

            if (parentNode != null)
            {
                Node n = parentNode.TopNode();
                int w = n.Width;
                int h = n.Height;

                sizePart = "vec2 size = vec2(" + w + "," + h + ");\r\n";
            }
            else if(parentGraph != null)
            {
                int w = parentGraph.Width;
                int h = parentGraph.Height;

                sizePart = "vec2 size = vec2(" + w + "," + h + ");\r\n";
            }

            frag += sizePart
                        + "vec2 pos = UV;\r\n"
                        + GetParentGraphShaderParams();

            int count = nodes.Count;
            for(int i = 0; i < count; i++)
            {
                var n = nodes[i] as MathNode;

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
                
                frag += "FragColor = vec4(" + last.ShaderId + endIndex.ToString() + ");\r\n";
            }
            else
            {
                frag += "return " + last.ShaderId + endIndex.ToString() + ";\r\n";
            }

            return frag;
        }

        public virtual void PrepareShader()
        {
            lastShaderCode = null;
            if (OutputNode == null)
            {
                return;
            }

            List<Node> ordered = OrderNodesForShader();

            string frag = "#version 330 core\r\n"
                         + "out vec4 FragColor;\r\n"
                         + "in vec2 UV;\r\n"
                         + "const float PI = 3.14159265359;\r\n"
                         + "const float Rad2Deg = (180.0 / PI);\r\n"
                         + "const float Deg2Rad = (PI / 180.0);\r\n"
                         + "const float RandomSeed = " + randomSeed + ";\r\n"
                         + "uniform sampler2D Input0;\r\n"
                         + "uniform sampler2D Input1;\r\n"
                         + "uniform sampler2D Input2;\r\n"
                         + "uniform sampler2D Input3;\r\n"
                         + GLSLHash;

            int count = calls.Count;
            for (int i = 0; i < count; i++)
            {
                CallNode m = calls[i];
                if (m.selectedFunction == this)
                {
                    continue;
                }

                string s = m.GetFunctionShaderCode();

                if (string.IsNullOrEmpty(s))
                {
                    return;
                }

                if (frag.IndexOf(s) == -1)
                {
                    frag += s;
                }
            }

            frag += "void main() {\r\n";

            string intern = GetInternalShaderCode(ordered);

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

            //one last check to verify the output actually has the expected output
            if (!HasExpectedOutput)
            {
                return false;
            }

            //Log.Debug(frag);

            Shader = Material.Material.CompileFragWithVert("image.glsl", lastShaderCode);

            if (Shader == null)
            {
                lastShaderCode = null;
                return false;
            }

            lastShaderCode = null;
            return true;
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
                Updated();
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
                Updated();
            }
        }

        //this was not needed in the long run
       /* public override bool Add(Node n)
        {
            if(n is MathNode)
            {
                MathNode mn = n as MathNode;
                bool suc = base.Add(n);

                //this handles a case where a node
                //is added from undo / redo / paste
                //for certain nodes such as call node
                if(suc)
                {
                    if(parentNode != null && mn.ParentNode == null)
                    {
                        mn.ParentNode = parentNode;
                    }
                    else if(parentGraph != null)
                    {
                        mn.OnFunctionParentSet();
                    }
                }

                return suc;
            }
            else if(n is ItemNode)
            {
                return base.Add(n);
            }

            return false;
        }*/

        //a function graph does not allow embedded graph instances
        //and the type must be coming from MathNodes path
        public override Node CreateNode(string type)
        {
            if (type.Contains("MathNodes") && !type.Contains(System.IO.Path.DirectorySeparatorChar))
            {
                MathNode n = base.CreateNode(type) as MathNode;
                n.AssignParentNode(parentNode);
                n.AssignParentGraph(this);
                return n;
            }
            else if(type.Contains("Items") && !type.Contains(System.IO.Path.DirectorySeparatorChar))
            {
                return base.CreateNode(type);
            }

            return null;
        }

        public void AssignParentGraph(Graph g)
        {
            parentGraph = g;

            var top = TopGraph();
            SetParentGraphVars(top);
        }

        public override void AssignParentNode(Node n)
        {
            base.AssignParentNode(n);

            var top = TopGraph();
            SetParentNodeVars(top);
        }

        public override void ResizeWith(int width, int height)
        {
            //do nothing in this graph
        }

        public override void ReleaseIntermediateBuffers()
        {
            //do nothing in this graph
        }

        protected void BuildShaderParam(GraphParameterValue param, StringBuilder builder, bool useMinMaxValue = false)
        {
            string type = "";

            if (param.Type == NodeType.Bool)
            {
                type = "bool ";
            }
            else if (param.Type == NodeType.Float)
            {
                type = "float ";
            }
            else if (param.Type == NodeType.Color || param.Type == NodeType.Float4 || param.Type == NodeType.Gray)
            {
                type = "vec4 ";
            }
            else if (param.Type == NodeType.Float2)
            {
                type = "vec2 ";
            }
            else if (param.Type == NodeType.Float3)
            {
                type = "vec3 ";
            }
            else if(param.Type == NodeType.Matrix)
            {
                type = "mat4 ";
            }
            else
            {
                return;
            }

            string prefix = "p_";
            string s1 = prefix + param.Name.Replace(" ", "").Replace("-", "") + " = ";

            builder.Append(type);
            builder.Append(s1);

            if (param.IsFunction())
            {
                BuildShaderFunctionValue(param, builder);
            }
            else
            {
                BuildShaderParamValue(param, builder, useMinMaxValue);
            }
        }

        protected void BuildShaderParamValue(GraphParameterValue param, StringBuilder builder, bool useMinMaxValue = false)
        {
            if (param.Type == NodeType.Bool)
            {
                builder.Append(Convert.ToBoolean(param.Value).ToString().ToLower() + ";\r\n");
            }
            else if (param.Type == NodeType.Float)
            {
                builder.Append(param.FloatValue.ToString() + ";\r\n");
            }
            else if (param.Type == NodeType.Float4 || param.Type == NodeType.Gray || param.Type == NodeType.Color)
            {
                MVector vec = new MVector();

                if (param.Value is MVector)
                {
                    if (!useMinMaxValue)
                    {
                        vec = (MVector)param.Value;
                    }
                    else
                    {
                        vec = param.VectorValue;
                    }
                }

                builder.Append("vec4(" + vec.X + "," + vec.Y + "," + vec.Z + "," + vec.W + ");\r\n");
            }
            else if (param.Type == NodeType.Float2)
            {
                MVector vec = new MVector();

                if (param.Value is MVector)
                {
                    if (!useMinMaxValue)
                    {
                        vec = (MVector)param.Value;
                    }
                    else
                    {
                        vec = param.VectorValue;
                    }
                }

                builder.Append("vec2(" + vec.X + "," + vec.Y + ");\r\n");
            }
            else if (param.Type == NodeType.Float3)
            {
                MVector vec = new MVector();

                if (param.Value is MVector)
                {
                    if (!useMinMaxValue)
                    {
                        vec = (MVector)param.Value;
                    }
                    else
                    {
                        vec = param.VectorValue;
                    }
                }

                builder.Append("vec3(" + vec.X + "," + vec.Y + "," + vec.Z + ");\r\n");
            }
            else if(param.Type == NodeType.Matrix)
            {
                Matrix4 m4 = param.Matrix4Value;
                //glsl matrices are column major order
                builder.Append("mat3(" + m4.Column0.X + ", " + m4.Column0.Y + ", " + m4.Column0.Z + ", " + m4.Column0.W + ", "
                                        + m4.Column1.X + ", " + m4.Column1.Y + ", " + m4.Column1.Z + ", " + m4.Column1.W + ", "
                                        + m4.Column2.X + ", " + m4.Column2.Y + ", " + m4.Column2.Z + ", " + m4.Column2.W + "," 
                                        + m4.Column3.X + ", " + m4.Column3.Y + ", " + m4.Column3.Z + ", " + m4.Column3.W + ");\r\n");
            }
        }

        protected void BuildShaderFunctionValue(GraphParameterValue param, StringBuilder builder)
        {
            FunctionGraph fn = param.Value as FunctionGraph;
            fn.TryAndProcess();
            object value = fn.Result;

            if (param.Type == NodeType.Bool)
            {
                builder.Append(Convert.ToBoolean(param.Value).ToString().ToLower() + ";\r\n");
            }
            else if (param.Type == NodeType.Float)
            {
                builder.Append(Convert.ToSingle(value).ToString() + ";\r\n");
            }
            else if (param.Type == NodeType.Float4 || param.Type == NodeType.Gray || param.Type == NodeType.Color)
            {
                MVector vec = new MVector();

                if (value is MVector)
                {
                    vec = (MVector)value;
                }

                builder.Append("vec4(" + vec.X + "," + vec.Y + "," + vec.Z + "," + vec.W + ");\r\n");
            }
            else if (param.Type == NodeType.Float2)
            {
                MVector vec = new MVector();

                if (value is MVector)
                {
                    vec = (MVector)value;
                }

                builder.Append("vec2(" + vec.X + "," + vec.Y + ");\r\n");
            }
            else if (param.Type == NodeType.Float3)
            {
                MVector vec = new MVector();

                if (value is MVector)
                {
                    vec = (MVector)value;
                }

                builder.Append("vec3(" + vec.X + "," + vec.Y + "," + vec.Z + ");\r\n");
            }
            else if (param.Type == NodeType.Matrix && value is Matrix4)
            {
                Matrix4 m4 = (Matrix4)value;
                //glsl matrices are column major order
                builder.Append("mat3(" + m4.Column0.X + ", " + m4.Column0.Y + ", " + m4.Column0.Z + ", " + m4.Column0.W + ", "
                                        + m4.Column1.X + ", " + m4.Column1.Y + ", " + m4.Column1.Z + ", " + m4.Column1.W + ", "
                                        + m4.Column2.X + ", " + m4.Column2.Y + ", " + m4.Column2.Z + ", " + m4.Column2.W + ","
                                        + m4.Column3.X + ", " + m4.Column3.Y + ", " + m4.Column3.Z + ", " + m4.Column3.W + ");\r\n");
            }
        }

        protected string GetParentGraphShaderParams()
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                var p = TopGraph();

                if (p != null)
                {
                    foreach (var param in p.Parameters.Values)
                    {
                        BuildShaderParam(param, builder);
                    }

                    int count = p.CustomParameters.Count;
                    for(int i = 0; i < count; i++)
                    {
                        GraphParameterValue param = p.CustomParameters[i];
                        if (param.IsFunction()) continue;
                        BuildShaderParam(param, builder, true);
                    }
                }
            }
            catch (StackOverflowException e)
            {
                Log.Error(e);
                Log.Error("There is an infinite function reference loop in promoted graph parameters.");
                return "";
            }

            return builder.ToString();
        }

        protected void SetParentGraphVars(Graph g)
        {
            if (g == null) return;

            try
            {
                var p = g;

                if (p != null)
                {
                    foreach (var k in p.Parameters.Keys)
                    {
                        var param = p.Parameters[k];

                        if (!param.IsFunction())
                        {
                            SetVar("p_" + param.Name.Replace(" ", "").Replace("-", ""), param.Value, param.Type);
                        }
                    }

                    int count = p.CustomParameters.Count;
                    for(int i = 0; i < count; i++)
                    {
                        GraphParameterValue param = p.CustomParameters[i];

                        if (!param.IsFunction())
                        {
                            SetVar("p_" + param.Name.Replace(" ", "").Replace("-", ""), param.Value, param.Type);
                        }
                    }
                }
            }
            catch (StackOverflowException e)
            {
                //possible
                Log.Error(e);
                Log.Error("There is an infinite function reference loop in promoted graph parameters.");
            }
        }

        protected void SetParentNodeVars(Graph g)
        {
            try
            {
                if (g == null || parentNode == null) return;

                var props = parentNode.GetType().GetProperties();

                var p = g;

                if (p != null)
                {
                    int count = props.Length;
                    for(int i = 0; i < count; i++)
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
                    for(int i = 0; i < count; i++)
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
            }
        }

        public override void TryAndProcess()
        {
            //if (!HasExpectedOutput) return;

            //small optimization
            var top = TopGraph();

            SetParentNodeVars(top);
            SetParentGraphVars(top);

            if(parentNode != null)
            {
                var n = parentNode.TopNode();
                int w = n.Width;
                int h = n.Height;

                SetVar("size", new MVector(w, h), NodeType.Float2);
            }
            else if(parentGraph != null)
            {
                SetVar("size", new MVector(parentGraph.Width, parentGraph.Height), NodeType.Float2);
            }
            else
            {
                SetVar("size", new MVector(), NodeType.Float2);
            }

            if (OutputNode == null) return;

            List<Node> ordered = OrderNodesForShader();
            //this ensures the function graph is processed
            //in the proper order
            //just as if it was running in the shader code
            if(ordered.Count > 0)
            {
                int count = ordered.Count;
                for(int i = 0; i < count; i++)
                {
                    ordered[i].TryAndProcess();
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
            for(int i = 0; i < count; i++)
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

        public override void FromJson(string data)
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
                if(arg.InputType == NodeType.Float)
                {
                    temp = 0;
                }
                else if(arg.InputType == NodeType.Bool)
                {
                    temp = false;
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
