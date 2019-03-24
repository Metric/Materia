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
        public Node ParentNode
        {
            get; set;
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

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            NodeData d = JsonConvert.DeserializeObject<NodeData>(data);
            SetBaseNodeDate(d);
            SetConnections(nodes, d.outputs);
            TryAndProcess();
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

        public virtual string GetShaderPart()
        {
            return "";
        }
    }
}
