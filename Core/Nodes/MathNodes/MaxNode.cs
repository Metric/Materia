using Materia.MathHelpers;
using Materia.Nodes.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class MaxNode : MathNode
    {
        NodeOutput output;

        public MaxNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Max";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float, this);


            for (int i = 0; i < 2; ++i)
            {
                var input = new NodeInput(NodeType.Float, this, "Float Input " + Inputs.Count);
                Inputs.Add(input);
            }

            Outputs.Add(output);
        }

        protected override void AddPlaceholderInput()
        {
            var input = new NodeInput(NodeType.Float, this, "Float Input " + Inputs.Count);
            Inputs.Add(input);
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

            return "float " + s + " = max(" + n1id + "," + n2id + ");\r\n";
        }

        public override void TryAndProcess()
        {
            float result = float.NegativeInfinity;
            foreach(NodeInput inp in Inputs)
            {
                if (inp != executeInput && inp.IsValid)
                {
                    float f = inp.Data.ToFloat();
                    result = Math.Max(result, f);
                }
            }

            output.Data = result;
            this.result = output.Data?.ToString();
        }
    }
}
