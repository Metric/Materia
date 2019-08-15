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
        protected NodeInput startInput;
        protected NodeInput endInput;
        protected NodeInput incrementInput;

        protected NodeOutput incrementOutput;
        protected NodeOutput completeOutput;

        public ForLoopNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "For Loop";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            startInput = new NodeInput(NodeType.Float, this, "Start");
            endInput = new NodeInput(NodeType.Float, this, "End");
            incrementInput = new NodeInput(NodeType.Float, this, "Increment By");

            incrementOutput = new NodeOutput(NodeType.Float, this, "Current");
            completeOutput = new NodeOutput(NodeType.Execute, this, "Done");

            Inputs.Add(startInput);
            Inputs.Add(endInput);
            Inputs.Add(incrementInput);


            startInput.OnInputAdded += Input_OnInputAdded;
            startInput.OnInputChanged += Input_OnInputChanged;

            endInput.OnInputAdded += Input_OnInputAdded;
            endInput.OnInputChanged += Input_OnInputChanged;

            incrementInput.OnInputAdded += Input_OnInputAdded;
            incrementInput.OnInputChanged += Input_OnInputChanged;

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

        }

        public override string GetShaderPart(string currentFrag)
        {
            if (ParentGraph == null)
            {
                return "";
            }

            if(!startInput.HasInput || !endInput.HasInput || !incrementInput.HasInput)
            {
                return "";
            }

            string startId = (startInput.Input.Node as MathNode).ShaderId;
            string endId = (endInput.Input.Node as MathNode).ShaderId;
            string incrementId = (incrementInput.Input.Node as MathNode).ShaderId;

            var idx2 = startInput.Input.Node.Outputs.IndexOf(startInput.Input);
            var idx3 = endInput.Input.Node.Outputs.IndexOf(endInput.Input);
            var idx4 = incrementInput.Input.Node.Outputs.IndexOf(incrementInput.Input);

            startId += idx2.ToString();
            endId += idx3.ToString();
            incrementId += idx4.ToString();

            string id = shaderId;

            string incId = id + "1";
            string innerBody = "";

            string frag = "";

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
                            //we have to tell the node to update output type
                            (op.Input.Node as MathNode).UpdateOutputType();
                        }
                    }
                }

                forward.Add(n);
                //tell the node to update output type
                //this handles a case where
                //the default function graph updateoutputtype
                //when loaded does not reach the for loop inner
                (n as MathNode).UpdateOutputType();

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
            if(startInput.HasInput
                && endInput.HasInput && incrementInput.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            if (startInput.Input.Data == null
                || endInput.Input.Data == null
                || incrementInput.Input.Data == null || ParentGraph == null)
            {
                return;
            }

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

                    foreach(var n in loop)
                    {
                        n.TryAndProcess();
                    }
                }
            }
        }
    }
}
