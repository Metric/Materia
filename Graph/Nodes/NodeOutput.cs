using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Newtonsoft.Json;

namespace Materia.Nodes
{
    public struct NodeConnection
    {
        //we ignore parent and order
        //as those are only used for
        //the undo redo system
        //so no need to save the
        //extra data on export
        [JsonIgnore]
        public string parent;

        public string node;
        public int index;
        public int outIndex;

        [JsonIgnore]
        public int order;

        public NodeConnection(string p, string n, int oi, int i, int ord)
        {
            parent = p;
            node = n;
            outIndex = oi;
            index = i;
            order = ord;
        }
    }

    public class NodeOutput : INodePoint
    {
        public delegate void OutputChanged(NodeOutput output);
        public event OutputChanged OnOutputChanged;

        public List<NodeInput> To { get; protected set; }

        public NodeType Type { get; set; }

        public Node ParentNode { get; protected set; }

        public Node Node { get; protected set; }

        public string Name { get; set; }

        protected object data;
        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        public NodeOutput(NodeType t, Node n, string name = "")
        {
            Type = t;
            Node = n;
            ParentNode = n;
            Name = name;
            To = new List<NodeInput>();
        }

        public NodeOutput(NodeType t, Node n, Node parent, string name = "")
        {
            Type = t;
            Node = n;
            ParentNode = parent;
            Name = name;
            To = new List<NodeInput>();
        }

        /// <summary>
        /// Gets the preview.
        /// A shortcut to Node?.Export()
        /// </summary>
        /// <returns></returns>
        public byte[] Export(int w = 0, int h = 0)
        {
            return Node?.Export(w, h);
        }

        /// <summary>
        /// Gets the active buffer.
        /// Basically a shortcut to Node?.GetActiveBuffer()
        /// </summary>
        /// <returns></returns>
        public GLTexture2D GetActiveBuffer()
        {
            return Node?.GetActiveBuffer();
        }

        public void InsertAt(int index, NodeInput inp, bool assign = false)
        {
            if (inp.Reference != null)
            {
                inp.Reference.Remove(inp);
            }

            if (assign)
            {
                inp.AssignReference(this);
            }
            else
            {
                inp.Reference = this;
            }

            if (index >= To.Count)
            {
                To.Add(inp);
            }
            else
            {
                To.Insert(index, inp);
            }

            OnOutputChanged?.Invoke(this);
        }

        public void Add(NodeInput inp, bool assign = false)
        {
            if(inp.Reference != null)
            {
                inp.Reference.Remove(inp);
            }

            if (assign)
            {
                inp.AssignReference(this);
            }
            else
            {
                inp.Reference = this;
                OnOutputChanged?.Invoke(this);
            }

            To.Add(inp);
        } 

        public void Remove(NodeInput inp)
        {
            if (To.Remove(inp))
            {
                inp.Reference = null;

                OnOutputChanged?.Invoke(this);
            }
        }
    }
}
