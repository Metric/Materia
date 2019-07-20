using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;

namespace Materia.Nodes
{
    public class MathNode : Node
    {
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
                OnParentNodeSet();
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

        [HideProperty]
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

        [HideProperty]
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

        [HideProperty]
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

        [HideProperty]
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

        [HideProperty]
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

        public MathNode()
        {
            CanPreview = false;

            Inputs = new List<NodeInput>();
            Outputs = new List<NodeOutput>();

            executeInput = new NodeInput(NodeType.Execute, this);
            Inputs.Add(executeInput);
            Outputs.Add(new NodeOutput(NodeType.Execute, this));
        }

        public Graph TopGraph()
        {
            var p = ParentGraph;

            if(p is FunctionGraph)
            {
                p = (p as FunctionGraph).TopGraph();
            }

            return p;
        }

        public override void FromJson(string data)
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

        protected override void OnWidthHeightSet()
        {
            
        }

        public virtual void OnFunctionParentSet()
        {

        }

        protected virtual void OnParentNodeSet()
        {

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
