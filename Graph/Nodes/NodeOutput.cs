using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Newtonsoft.Json;

namespace Materia.Nodes
{
    public struct NodeConnection
    {
        public string node;
        public int index;
        public int outIndex;

        public NodeConnection(string n, int oi, int i)
        {
            node = n;
            outIndex = oi;
            index = i;
        }
    }

    public class NodeOutput : INodePoint
    {
        public delegate void OutputChanged(NodeOutput output);
        public event OutputChanged OnOutputChanged;

        public List<NodeInput> To { get; protected set; }

        public NodeType Type { get; set; }

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

        public void Dispose()
        {
            for (int i = 0; i < To.Count; ++i)
            {
                //clear reference
                To[i].AssignReference(null);
            }
            To.Clear();
        }
    }
}
