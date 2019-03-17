using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.MathNodes;
using Newtonsoft.Json;
using Materia.Imaging;
using Materia.Shaders;

namespace Materia.Nodes
{
    public class FunctionGraph : Graph
    {
        static string GLSLHash = "float rand(vec2 co) {\r\n"
                                 + "return fract(sin(dot(co, vec2(12.9898,78.233))) * 43758.5453);\r\n"
                                 + "}\r\n\r\n";

        public Node OutputNode { get; protected set; }

        public GLShaderProgram Shader { get; protected set; }

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

                return (OutputNode.Outputs[0].Type == ExpectedOutput);
            }
        }

        public object Result
        {
            get; set;
        }

        public FunctionGraph(string name, int w = 256, int h = 256) : base(name, w, h)
        {
            Name = name;
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

            string frag = "#version 330 core\r\n"
                         + "out vec4 FragColor;\r\n"
                         + "in vec2 UV;\r\n"
                         + "uniform sampler2D Input0;\r\n"
                         + "uniform sampler2D Input1;\r\n"
                         + GLSLHash
                         + "void main() {\r\n"
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
                            if (!reverseStack.Contains(n2))
                            {
                                reverseStack.Push(n2);
                                processStack.Enqueue(n2);
                            }
                        }
                    }
                }
            }

            while(reverseStack.Count > 0)
            {
                var n = reverseStack.Pop() as MathNode;
                string d = n.GetShaderPart();

      
                if (string.IsNullOrEmpty(d)) return false;

                frag += d;
            }

            var last = OutputNode as MathNode;

            frag += "FragColor = vec4(" + last.ShaderId + "0);\r\n}";
            Console.Write(frag);

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
        //or must be a sequence node
        //as a sequence node is simply a pass through to multiple branches
        public override Node CreateNode(string type)
        {
            if (type.Contains("MathNodes") && !type.Contains(System.IO.Path.PathSeparator))
            {
                return base.CreateNode(type);
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

        public override void TryAndProcess()
        {
            //if (!HasExpectedOutput) return;

            if(Nodes.Count == 1)
            {
                Nodes[0].TryAndProcess();
                return;
            }
            else if(Nodes.Count == 2)
            {
                Nodes[0].TryAndProcess();
                Nodes[1].TryAndProcess();
                return;
            }
            else if(Nodes.Count == 3)
            {
                Nodes[0].TryAndProcess();
                Nodes[1].TryAndProcess();
                Nodes[2].TryAndProcess();
                return;
            }

            int len = Nodes.Count;

            for (int i = 0; i < len; i++)
            {
                Nodes[i].TryAndProcess();
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

            NodeLookup.TryGetValue(d.outputNode, out n);

            OutputNode = n;
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
