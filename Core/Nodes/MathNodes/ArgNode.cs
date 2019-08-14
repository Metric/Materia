using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;

namespace Materia.Nodes.MathNodes
{
    public class ArgNode : MathNode
    {
        protected string inputName;

        [Editable(ParameterInputType.Text, "Input Name")]
        public string InputName
        {
            get
            {
                return inputName;
            }
            set
            {
                inputName = value;
                OnDescription(inputName);
            }
        }

        protected NodeType inputType;
        [Dropdown(null, "Bool", "Float", "Float2", "Float3", "Float4", "Matrix2", "Matrix3", "Matrix4")]
        [Editable(ParameterInputType.Dropdown, "Input Type")]
        public NodeType InputType
        {
            get
            {
                return inputType;
            }
            set
            {
                inputType = value;
            }
        }

        public ArgNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            inputType = NodeType.Float;

            inputName = "arg";

            CanPreview = false;

            Name = "Arg";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            Inputs = new List<NodeInput>();
            Outputs = new List<NodeOutput>();
        }

        public override string GetDescription()
        {
            return inputName;
        }

        public class ArgNodeData: NodeData
        {
            public string inputName;
            public int inputType;
        }

        public override string GetJson()
        {
            ArgNodeData d = new ArgNodeData();
            FillBaseNodeData(d);
            d.inputName = inputName;
            d.inputType = (int)inputType;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            ArgNodeData d = JsonConvert.DeserializeObject<ArgNodeData>(data);
            SetBaseNodeDate(d);
            inputName = d.inputName;
            inputType = (NodeType)d.inputType;
        }
    }
}
