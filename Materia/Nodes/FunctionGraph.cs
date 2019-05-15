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
using OpenTK;

namespace Materia.Nodes
{
    public class FunctionGraph : Graph
    {
        static string GLSLHash = "float rand(vec2 co) {\r\n"
                                 + "return fract(sin(dot(co, vec2(12.9898,78.233))) * 43758.5453);\r\n"
                                 + "}\r\n\r\n";

        public Node OutputNode { get; protected set; }

        public GLShaderProgram Shader { get; protected set; }

        [HideProperty]
        public NodeType ExpectedOutput
        {
            get; set;
        }

        [HideProperty]
        public bool HasExpectedOutput
        {
            get
            {
                if (OutputNode == null) return false;
                if (OutputNode.Outputs == null) return false;
                if (OutputNode.Outputs.Count == 0) return false;

                return (OutputNode.Outputs[0].Type & ExpectedOutput) != 0;
            }
        }

        public object Result
        {
            get; set;
        }

        protected Node parentNode;
        public Node ParentNode
        {
            get
            {
                return parentNode;
            }
            set
            {
                parentNode = value;
                //update nodes
                int c = Nodes.Count;
                for(int i = 0; i < c; i++)
                {
                    MathNode n = (MathNode)Nodes[i];
                    n.ParentNode = parentNode;
                }
            }
        }

        [HideProperty]
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

        [HideProperty]
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

        public FunctionGraph(string name, int w = 256, int h = 256) : base(name, w, h)
        {
            Name = name;
            SetVar("PI", 3.14159265359f);
        }

        public virtual bool BuildShader()
        {
            if (OutputNode == null) return false;

            if(Shader != null)
            {
                Shader.Release();
                Shader = null;
            }

            Stack<Node> reverseStack = new Stack<Node>();
            Queue<Node> processStack = new Queue<Node>();

            string sizePart = "vec2 size = vec2(0);\r\n";

            if(parentNode != null)
            {
                int w = parentNode.Width;
                int h = parentNode.Height;

                sizePart = "vec2 size = vec2(" + w + "," + h + ");\r\n";
            }

            string frag = "#version 330 core\r\n"
                         + "out vec4 FragColor;\r\n"
                         + "in vec2 UV;\r\n"
                         + "const float PI = 3.14159265359;\r\n"
                         + "uniform sampler2D Input0;\r\n"
                         + "uniform sampler2D Input1;\r\n"
                         + GLSLHash
                         + "void main() {\r\n"
                         + sizePart
                         + "vec2 pos = UV;\r\n";

            processStack.Enqueue(OutputNode);
            reverseStack.Push(OutputNode);

            while(processStack.Count > 0)
            {
                var n = processStack.Dequeue();

                if (n.Inputs != null)
                {
                    for (int i = 0; i < n.Inputs.Count; i++)
                    {
                        var inp = n.Inputs[i];
                        if (inp.HasInput)
                        {
                            var n2 = inp.Input.Node;
                            reverseStack.Push(n2);
                            processStack.Enqueue(n2);
                        }
                    }
                }
            }

            while(reverseStack.Count > 0)
            {
                var n = reverseStack.Pop() as MathNode;
                string d = n.GetShaderPart();

      
                if (string.IsNullOrEmpty(d)) return false;

                if (frag.IndexOf(d) == -1)
                {
                    frag += d;
                }
            }

            var last = OutputNode as MathNode;

            frag += "FragColor = vec4(" + last.ShaderId + "0);\r\n}";

            //one last check to verify the output actually has the expected output
            if (!HasExpectedOutput) return false;

            Shader = Material.Material.CompileFragWithVert("image.glsl", frag);

            if (Shader == null) return false;

            return true;
        }

        public virtual void SetOutputNode(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                OutputNode = null;
                Updated();
                return;
            }

            Node n = null;
            if(NodeLookup.TryGetValue(id, out n))
            {
                OutputNode = n;
                Updated();
            }
        }

        public override bool Add(Node n)
        {
            if(n is MathNode)
            {
                return base.Add(n);
            }

            return false;
        }

        //a function graph does not allow embedded graph instances
        //and the type must be coming from MathNodes path
        public override Node CreateNode(string type)
        {
            if (type.Contains("MathNodes") && !type.Contains(System.IO.Path.PathSeparator))
            {
                MathNode n = base.CreateNode(type) as MathNode;
                n.ParentNode = parentNode;
                return n;
            }

            return null;
        }

        public override void ResizeWith(int width, int height)
        {
            //do nothing in this graph
        }

        public override void ReleaseIntermediateBuffers()
        {
            //do nothing in this graph
        }

        protected void SetParentNodeVars()
        {
            if(parentNode != null)
            {
                var props = parentNode.GetType().GetProperties();

                var p = parentNode.ParentGraph;

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

                if (p != null)
                {
                    foreach (var prop in props)
                    {
                        if(!prop.PropertyType.Equals(typeof(int))
                            && !prop.PropertyType.Equals(typeof(float))
                            && !prop.PropertyType.Equals(typeof(MVector))
                            && !prop.PropertyType.Equals(typeof(bool))
                            && !prop.PropertyType.Equals(typeof(double))
                            && !prop.PropertyType.Equals(typeof(Vector4)))
                        {
                            continue;
                        }

                        try
                        {
                            HidePropertyAttribute hb = prop.GetCustomAttribute<HidePropertyAttribute>();

                            if(hb != null)
                            {
                                continue;
                            }
                        }
                        catch
                        {

                        }

                        object v = null;
                        if (p.HasParameterValue(parentNode.Id, prop.Name))
                        {
                            var gp = p.GetParameterRaw(parentNode.Id, prop.Name);
                            if (!gp.IsFunction())
                            {
                                v = gp.Value;
                            }
                            else
                            {
                                v = prop.GetValue(parentNode);
                            }
                        }
                        else
                        {
                            v = prop.GetValue(parentNode);
                        }

                        string varName = "";

                        try
                        {
                            TitleAttribute t = prop.GetCustomAttribute<TitleAttribute>();

                            if(t != null)
                            {
                                varName = t.Title.Replace(" ", "");
                            }
                            else
                            {
                                varName = prop.Name;
                            }
                        }
                        catch
                        {
                            varName = prop.Name;
                        }

                        if(v != null)
                        {
                            if(v is Vector4)
                            {
                                Vector4 vec = (Vector4)v;
                                v = new MVector(vec.X, vec.Y, vec.Z, vec.W);
                            }

                            SetVar(varName, v);
                        }
                    }
                }
                else
                {
                    foreach(var prop in props)
                    {
                        if (!prop.PropertyType.Equals(typeof(int))
                          && !prop.PropertyType.Equals(typeof(float))
                          && !prop.PropertyType.Equals(typeof(MVector))
                          && !prop.PropertyType.Equals(typeof(bool))
                          && !prop.PropertyType.Equals(typeof(double))
                          && !prop.PropertyType.Equals(typeof(Vector4)))
                        {
                            continue;
                        }

                        try
                        {
                            HidePropertyAttribute hb = prop.GetCustomAttribute<HidePropertyAttribute>();

                            if (hb != null)
                            {
                                continue;
                            }
                        }
                        catch
                        {

                        }

                        object v = prop.GetValue(parentNode);
                        string varName = "";

                        try
                        {
                            TitleAttribute t = prop.GetCustomAttribute<TitleAttribute>();

                            if (t != null)
                            {
                                varName = t.Title.Replace(" ", "");
                            }
                            else
                            {
                                varName = prop.Name;
                            }
                        }
                        catch
                        {
                            varName = prop.Name;
                        }

                        if (v != null)
                        {
                            if (v is Vector4)
                            {
                                Vector4 vec = (Vector4)v;
                                v = new MVector(vec.X, vec.Y, vec.Z, vec.W);
                            }

                            SetVar(varName, v);
                        }
                    }
                }
            }
        }

        public override void TryAndProcess()
        {
            //if (!HasExpectedOutput) return;

            SetParentNodeVars();

            if(parentNode != null)
            {
                int w = parentNode.Width;
                int h = parentNode.Height;

                SetVar("size", new MVector(w, h));
            }
            else
            {
                SetVar("size", new MVector());
            }

            if (OutputNode == null) return;

            Stack<Node> reverseStack = new Stack<Node>();
            Queue<Node> processStack = new Queue<Node>();

            processStack.Enqueue(OutputNode);
            reverseStack.Push(OutputNode);

            while (processStack.Count > 0)
            {
                var n = processStack.Dequeue();

                if (n.Inputs != null)
                {
                    for (int i = 0; i < n.Inputs.Count; i++)
                    {
                        var inp = n.Inputs[i];
                        if (inp.HasInput)
                        {
                            var n2 = inp.Input.Node;
                            if (!reverseStack.Contains(n2))
                            {
                                reverseStack.Push(n2);
                                processStack.Enqueue(n2);
                            }
                        }
                    }
                }
            }

            List<Node> nodes = reverseStack.ToList();
            int count = nodes.Count;
            for(int i = 0; i < count; i++)
            {
                nodes[i].TryAndProcess();
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

            foreach(Node n in Nodes)
            {
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
            base.FromJson(data);

            FunctionGraphData d = JsonConvert.DeserializeObject<FunctionGraphData>(data);

            Node n = null;

            if (d.outputNode != null)
            {
                NodeLookup.TryGetValue(d.outputNode, out n);
                OutputNode = n;
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
