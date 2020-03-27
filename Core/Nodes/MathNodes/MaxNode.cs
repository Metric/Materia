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

        static int MIN_INPUTS = 2;

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
                input.OnInputChanged += Input_OnInputChanged;
                Inputs.Add(input);
            }

            Outputs.Add(output);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            int inputsFilled = 0;
            for (int i = 0; i < Inputs.Count; ++i)
            {
                var input = Inputs[i];
                if (input.HasInput)
                {
                    ++inputsFilled;
                }
            }

            if (inputsFilled >= Inputs.Count - 1)
            {
                AddPlaceholderInput();
            }
            // minus 2 to account for an empty one
            // +1 to account for execute pin
            else if (inputsFilled < Inputs.Count - 2 && inputsFilled > MIN_INPUTS + 1)
            {
                for (int i = MIN_INPUTS + 1; i < Inputs.Count; ++i)
                {
                    var input = Inputs[i];
                    Inputs.RemoveAt(i);
                    RemovedInput(input);
                    --i;
                }
            }
        }

        protected override void AddPlaceholderInput()
        {
            var input = new NodeInput(NodeType.Float, this, "Float Input " + Inputs.Count);
            Inputs.Add(input);
            AddedInput(input);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "1";
            string maxShaderString = "";
            for (int i = 1; i < Inputs.Count - 1; i+=2)
            {
                var input = Inputs[i];
                var input2 = Inputs[i + 1];
                if (input.HasInput && input2.HasInput)
                {
                    var n1id = (input.Reference.Node as MathNode).ShaderId;
                    var n2id = (input2.Reference.Node as MathNode).ShaderId;

                    var index1 = input.Reference.Node.Outputs.IndexOf(input.Reference);
                    var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);

                    n1id += index1;
                    n2id += index2;

                    if (i == 1 || string.IsNullOrEmpty(maxShaderString))
                    {
                        maxShaderString += "float " + s + " = max(" + n1id + "," + n2id + ");\r\n";
                    }
                    else
                    {
                        maxShaderString += s + " = max(" + s + ", " + n1id + ");\r\n";
                        maxShaderString += s + " = max(" + s + ", " + n2id + ");\r\n";
                    }
                }
            }

            return maxShaderString;
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
