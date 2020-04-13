using System.Collections.Generic;
using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Graph;

namespace Materia.Nodes
{
    public class MathNode : Node
    {
        protected string result;

        [ReadOnly]
        [Editable(ParameterInputType.Text, "Current Output Result", "Debug")]
        public string Result
        {
            get
            {
                return result;
            }
        }

        public NodeInput executeInput;

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
            }
        }

        protected string shaderId;
        public string ShaderId
        {
            get
            {
                return shaderId;
            }
        }

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

        public new float TileX
        {
            get
            {
                return tileX;
            }
            set
            {
                tileX = value;
            }
        }

        public new float TileY
        {
            get
            {
                return tileY;
            }
            set
            {
                tileY = value;
            }
        }

        public new GraphPixelType InternalPixelFormat
        {
            get
            {
                return internalPixelType;
            }
            set
            {
                internalPixelType = value;
            }
        }

        //redefine so we remove the attribute from it
        public new bool AbsoluteSize { get; set; }

        public MathNode()
        {
            //math nodes are always absolute size
            //since we ignore their size competely already
            AbsoluteSize = true;
            CanPreview = false;

            Inputs = new List<NodeInput>();
            Outputs = new List<NodeOutput>();

            executeInput = new NodeInput(NodeType.Execute, this);
            Inputs.Add(executeInput);
            Outputs.Add(new NodeOutput(NodeType.Execute, this));
        }

        public override void AssignParentNode(Node n)
        {
            parentNode = n;
        }

        public Graph.Graph TopGraph()
        {
            var p = parentGraph;

            if(p is Function)
            {
                Function g = p as Function;
                return g.ParentNode != null ? g.ParentNode.ParentGraph : g.ParentGraph;
            }

            return p;
        }

        public override void FromJson(string data, Archive archive = null)
        {
            FromJson(data);
        }

        public virtual void FromJson(string data)
        {
            NodeData d = JsonConvert.DeserializeObject<NodeData>(data);
            SetBaseNodeDate(d);
        }

        public override string GetJson()
        {
            NodeData d = new NodeData();
            FillBaseNodeData(d);

            return JsonConvert.SerializeObject(d);
        }

        public virtual void UpdateOutputType()
        {

        }

        public override bool IsRoot()
        {
            return executeInput == null || !executeInput.HasInput;
        }

        public virtual string GetShaderPart(string currentFrag)
        {
            return "";
        }
    }
}
