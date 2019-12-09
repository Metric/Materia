using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using NLog;

namespace Materia.Nodes.MathNodes
{
    public class DistanceNode : MathNode
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        NodeOutput output;

        public DistanceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Distance";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float, this);

            for (int i = 0; i < 2; ++i)
            {
                var input = new NodeInput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Vector Input " + i);
                Inputs.Add(input);

            }
            Outputs.Add(output);
        }


        public override string GetShaderPart(string currentFrag)
        {
            if (!Inputs[1].HasInput || !Inputs[2].HasInput) return "";

            var s = shaderId + "1";
            var n1id = (Inputs[1].Reference.Node as MathNode).ShaderId;
            var n2id = (Inputs[2].Reference.Node as MathNode).ShaderId;

            var index = Inputs[1].Reference.Node.Outputs.IndexOf(Inputs[1].Reference);

            n1id += index;

            var index2 = Inputs[2].Reference.Node.Outputs.IndexOf(Inputs[2].Reference);

            n2id += index2;

            var t1 = Inputs[1].Reference.Type;
            var t2 = Inputs[2].Reference.Type;

            if (t1 == NodeType.Float2 && t2 == NodeType.Float2)
            {
                return "float " + s + " = distance(" + n1id + ", " + n2id + ");\r\n";
            }
            else if (t1 == NodeType.Float3 && t2 == NodeType.Float3)
            {
                return "float " + s + " = distance(" + n1id + ", " + n2id + ");\r\n";
            }
            else if (t1 == NodeType.Float4 && t2 == NodeType.Float4)
            {
                return "float " + s + " = distance(" + n1id + ", " + n2id + ");\r\n";
            }

            return "float " + s + " = 0;\r\n";
        }

        public override void TryAndProcess()
        {
            NodeInput input = Inputs[1];
            NodeInput input2 = Inputs[2];

            if (!input.IsValid || !input2.IsValid) return;

            try
            {
                MVector v = (MVector)input.Data;
                MVector v2 = (MVector)input2.Data;
                output.Data = v.Distance(v2);
                result = output.Data?.ToString();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
