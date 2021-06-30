using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;

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

            defaultName = Name = "For Loop";

            shaderId = "S" + Id.Split('-')[0];

            startInput = new NodeInput(NodeType.Float, this, "Start");
            endInput = new NodeInput(NodeType.Float, this, "End");
            incrementInput = new NodeInput(NodeType.Float, this, "Increment By");

            incrementOutput = new NodeOutput(NodeType.Float, this, "Current");
            completeOutput = new NodeOutput(NodeType.Execute, this, "Done");

            Inputs.Add(startInput);
            Inputs.Add(endInput);
            Inputs.Add(incrementInput);


            Outputs.Add(incrementOutput);
            Outputs.Add(completeOutput);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if(!startInput.HasInput || !endInput.HasInput || !incrementInput.HasInput)
            {
                return "";
            }

            string startId = (startInput.Reference.Node as MathNode).ShaderId;
            string endId = (endInput.Reference.Node as MathNode).ShaderId;
            string incrementId = (incrementInput.Reference.Node as MathNode).ShaderId;

            var idx2 = startInput.Reference.Node.Outputs.IndexOf(startInput.Reference);
            var idx3 = endInput.Reference.Node.Outputs.IndexOf(endInput.Reference);
            var idx4 = incrementInput.Reference.Node.Outputs.IndexOf(incrementInput.Reference);

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
                            //we have to tell the node to update output type
                            (op.Reference.Node as MathNode).UpdateOutputType();
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
                            ++i;
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
            if(!startInput.IsValid || !incrementInput.IsValid || !endInput.IsValid)
            {
                return;
            }

            List<Node> loop = GetLoopNodes();

            float s = startInput.Data.ToFloat();
            float e = endInput.Data.ToFloat();
            float incr = incrementInput.Data.ToFloat();

            if (s <= e)
            {
                for (float i = s; i < e; i += incr)
                {
                    incrementOutput.Data = i;
                    for(int j = 0; j < loop.Count; ++j)
                    {
                        Node n = loop[j];
                        n.TryAndProcess();
                    }
                }
            }
            else
            {
                for (float i = s; i >= e; i -= incr)
                {
                    incrementOutput.Data = i;
                    for (int j = 0; j < loop.Count; ++j)
                    {
                        Node n = loop[j];
                        n.TryAndProcess();
                    }
                }
            }
        }
    }
}
