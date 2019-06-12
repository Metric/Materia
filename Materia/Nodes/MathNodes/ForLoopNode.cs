using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;
using System.Text.RegularExpressions;

namespace Materia.Nodes.MathNodes
{
    /// <summary>
    /// This is one of the most complex nodes
    /// to generate its shader code
    /// </summary>
    public class ForLoopNode : MathNode
    {
        protected NodeInput initialInput;
        protected NodeInput startInput;
        protected NodeInput endInput;
        protected NodeInput incrementInput;

        protected NodeOutput loopOutput;
        protected NodeOutput incrementOutput;
        protected NodeOutput completeOutput;

        public ForLoopNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "For Loop";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            initialInput = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Type");
            startInput = new NodeInput(NodeType.Float, this, "Start");
            endInput = new NodeInput(NodeType.Float, this, "End");
            incrementInput = new NodeInput(NodeType.Float, this, "Increment By");

            loopOutput = new NodeOutput(NodeType.Float | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Loop");
            incrementOutput = new NodeOutput(NodeType.Float, this, "Current");
            completeOutput = new NodeOutput(NodeType.Execute, this, "Done");

            Inputs.Add(initialInput);
            Inputs.Add(startInput);
            Inputs.Add(endInput);
            Inputs.Add(incrementInput);

            initialInput.OnInputAdded += Input_OnInputAdded;
            initialInput.OnInputChanged += Input_OnInputChanged;

            startInput.OnInputAdded += Input_OnInputAdded;
            startInput.OnInputChanged += Input_OnInputChanged;

            endInput.OnInputAdded += Input_OnInputAdded;
            endInput.OnInputChanged += Input_OnInputChanged;

            incrementInput.OnInputAdded += Input_OnInputAdded;
            incrementInput.OnInputChanged += Input_OnInputChanged;

            Outputs.Add(loopOutput);
            Outputs.Add(incrementOutput);
            Outputs.Add(completeOutput);
        }

        private void Input_OnInputAdded(NodeInput input)
        {
            UpdateOutputType();
            Updated();
        }

        private void Input_OnInputChanged(NodeInput input)
        {
            TryAndProcess();
        }

        public override void UpdateOutputType()
        {
            if (initialInput.HasInput)
            {
                loopOutput.Type = initialInput.Input.Type;
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (ParentGraph == null)
            {
                return "";
            }

            if(!initialInput.HasInput || !startInput.HasInput || !endInput.HasInput || !incrementInput.HasInput)
            {
                return "";
            }

            NodeType inputType = initialInput.Input.Type;

            loopOutput.Type = inputType;

            string initialId = (initialInput.Input.Node as MathNode).ShaderId;
            string startId = (startInput.Input.Node as MathNode).ShaderId;
            string endId = (endInput.Input.Node as MathNode).ShaderId;
            string incrementId = (incrementInput.Input.Node as MathNode).ShaderId;

            var idx1 = initialInput.Input.Node.Outputs.IndexOf(initialInput.Input);
            var idx2 = startInput.Input.Node.Outputs.IndexOf(startInput.Input);
            var idx3 = endInput.Input.Node.Outputs.IndexOf(endInput.Input);
            var idx4 = incrementInput.Input.Node.Outputs.IndexOf(incrementInput.Input);

            initialId += idx1.ToString();
            startId += idx2.ToString();
            endId += idx3.ToString();
            incrementId += idx4.ToString();

            string id = shaderId;

            string loopId = id + "1";
            string incId = id + "2";
            string innerBody = "";

            string frag = "";

            if(inputType == NodeType.Float)
            {
                frag += "float " + loopId + " = " + initialId + ";\r\n";
            }
            else if(inputType == NodeType.Float2)
            {
                frag += "vec2 " + loopId + " = " + initialId + ";\r\n";
            }
            else if(inputType == NodeType.Float3)
            {
                frag += "vec3 " + loopId + " = " + initialId + ";\r\n";
            }
            else if(inputType == NodeType.Float4 
                || inputType == NodeType.Color || inputType == NodeType.Gray)
            {
                frag += "vec4 " + loopId + " = " + initialId + ";\r\n";
            }

            frag += "\r\nif (" + startId + " <= " + endId + ") {\r\n";

            frag += "for (float " + incId + " = " + startId + "; " + incId + " < " + endId + "; " + incId + " += " + incrementId + ") {\r\n";

            innerBody = GetLoopBodyShaderCode(currentFrag + frag);

            if (string.IsNullOrEmpty(innerBody))
            {
                return "";
            }

            innerBody += "}\r\n}\r\n";

            frag += innerBody;

            frag += "else {\r\n";

            frag += "for (float " + incId + " = " + startId + "; " + incId + " >= " + endId + "; " + incId + " -= " + incrementId + ") {\r\n";

            frag += innerBody + "\r\n";

            return frag;
        }

        protected List<Node> TravelBranch(Node parent, HashSet<Node> seen)
        {
            List<Node> forward = new List<Node>();
            Queue<Node> queue = new Queue<Node>();

            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                Node n = queue.Dequeue();

                if (seen.Contains(n))
                {
                    continue;
                }

                seen.Add(n);

                foreach (var op in n.Inputs)
                {
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

                if (n.Outputs.Count > 0)
                {
                    int i = 0;
                    foreach (var op in n.Outputs)
                    {
                        //we don't care about the actual for loop internals at the momemnt
                        //as each for loop will handle it
                        if (n is ForLoopNode && i == 0)
                        {
                            i++;
                            continue;
                        }

                        if (op.Type == NodeType.Execute)
                        {
                            if (op.To.Count > 1)
                            {
                                foreach (var t in op.To)
                                {
                                    var branch = TravelBranch(t.Node, seen);
                                    forward.AddRange(branch);
                                }
                            }
                            else if(op.To.Count > 0)
                            {
                                queue.Enqueue(op.To[0].Node);
                            }
                        }
                    }
                }
            }

            return forward;
        }

        //this is for the cpu route
        protected List<Node> GetLoopNodes()
        {
            List<Node> forward = new List<Node>();

            //output 0 on a for loop should
            //always be the loop execution pin
            NodeOutput exc = Outputs[0];
            HashSet<Node> seen = new HashSet<Node>();

            foreach (var t in exc.To)
            {
                var branch = TravelBranch(t.Node, seen);
                forward.AddRange(branch);
            }

            return forward;
        }

        protected string GetLoopBodyShaderCode(string parentFrag)
        {
            string frag = "";

            List<Node> forward = new List<Node>();

            //output 0 on a for loop should
            //always be the loop execution pin
            NodeOutput exc = Outputs[0];

            HashSet<Node> seen = new HashSet<Node>();

            foreach (var t in exc.To)
            {
                var branch = TravelBranch(t.Node, seen);
                forward.AddRange(branch);
            }

            var l = forward;

            foreach(var n in l)
            {
                string tmpFrag = parentFrag + frag;
                string part = (n as MathNode).GetShaderPart(tmpFrag);

                if(string.IsNullOrEmpty(part))
                {
                    return "";
                }

                if(tmpFrag.IndexOf(part) == -1)
                {
                    frag += part;
                }
            }

            return frag;
        }

        public override void TryAndProcess()
        {
            if(initialInput.HasInput && startInput.HasInput
                && endInput.HasInput && incrementInput.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            if (initialInput.Input.Data == null || startInput.Input.Data == null
                || endInput.Input.Data == null
                || incrementInput.Input.Data == null || ParentGraph == null)
            {
                return;
            }

            object d = initialInput.Input.Data;
            float s = Convert.ToSingle(startInput.Input.Data);
            float e = Convert.ToSingle(endInput.Input.Data);
            float incr = Convert.ToSingle(incrementInput.Input.Data);

            List<Node> loop = GetLoopNodes();

            //handle forwards or backwards loops
            if (s <= e)
            {
                for (float i = s; i < e; i += incr)
                {
                    incrementOutput.Data = i;
                    loopOutput.Data = d;

                    foreach(var n in loop)
                    {
                        n.TryAndProcess();
                    }
                }
            }
            else
            {
                for(float i = s; i >= e; i-=incr)
                {
                    incrementOutput.Data = i;
                    loopOutput.Data = d;

                    foreach(var n in loop)
                    {
                        n.TryAndProcess();
                    }
                }
            }
        }

        public class ForLoopNodeData : NodeData
        {
            public string resultVariable;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            ForLoopNodeData d = JsonConvert.DeserializeObject<ForLoopNodeData>(data);
            SetBaseNodeDate(d);
        }

        public override string GetJson()
        {
            ForLoopNodeData d = new ForLoopNodeData();
            FillBaseNodeData(d);

            return JsonConvert.SerializeObject(d);
        }
    }
}
