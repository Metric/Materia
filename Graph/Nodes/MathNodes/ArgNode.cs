using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.MathNodes
{
    public class ArgNode : MathNode
    {
        protected string inputName = "arg";

        [Editable(ParameterInputType.Text, "Input Name")]
        public string InputName
        {
            get
            {
                return inputName;
            }
            set
            {
                if(!string.IsNullOrEmpty(inputName))
                {
                    if(ParentGraph != null && ParentGraph is Function)
                    {
                        Function g = ParentGraph as Function;
                        g.RemoveVar(inputName);
                    }
                }

                inputName = value;
                TriggerValueChange();
            }
        }

        protected NodeType inputType = NodeType.Float;
        [Dropdown(null, false, "Bool", "Float", "Float2", "Float3", "Float4", "Matrix")]
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

            CanPreview = false;

            Name = "Arg";

            shaderId = "S" + Id.Split('-')[0];

            //remove default execution pins
            Inputs.Clear();
            Outputs.Clear();

            ExecuteInput = null;
        }


        public override string GetDescription()
        {
            return inputName;
        }

        public class ArgNodeData: NodeData
        {
            public string inputName;
            public int inputType;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(inputName);
                w.Write(inputType);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                inputName = r.NextString();
                inputType = r.NextInt();
            }
        }

        public override void Dispose()
        {
            if (ParentGraph != null && ParentGraph is Function)
            {
                Function g = ParentGraph as Function;

                if(!string.IsNullOrEmpty(inputName))
                {
                    g.RemoveVar(inputName);
                }
            }
            base.Dispose();
        }

        public override void GetBinary(Writer w)
        {
            ArgNodeData d = new ArgNodeData();
            FillBaseNodeData(d);
            d.inputName = inputName;
            d.inputType = (int)inputType;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            ArgNodeData d = new ArgNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            inputName = d.inputName;
            inputType = (NodeType)d.inputType;
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
