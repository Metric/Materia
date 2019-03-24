using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes
{
    public struct NodeOutputConnection
    {
        public string node;
        public int index;
        public int outIndex;

        public NodeOutputConnection(string n, int oi, int i)
        {
            node = n;
            outIndex = oi;
            index = i;
        }
    }

    public class NodeOutput
    {
        public delegate void OutputChanged(NodeOutput inp);
        public event OutputChanged OnInputAdded;
        public event OutputChanged OnInputRemoved;

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

        public void Changed()
        {
            if (To != null && To.Count > 0)
            {
                int c = To.Count;
                for(int i = 0; i < c; i++)
                {
                    To[i].InputDataChanged();
                }
            }
        }

        public NodeOutput(NodeType t, Node n, string name = "")
        {
            Type = t;
            Node = n;
            Name = name;
            To = new List<NodeInput>();
        }

        public bool Add(NodeInput inp)
        {
            if((inp.Type & Type) != 0)
            {
                if(inp.Input != null )
                {
                    inp.Input.Remove(inp);
                }

                inp.Input = this;
                To.Add(inp);

                if (OnInputAdded != null)
                {
                    OnInputAdded(this);
                }

                return true;
            }

            return false;
        } 

        public void Remove(NodeInput inp)
        {
            To.Remove(inp);
            inp.Input = null;

            if (OnInputRemoved != null)
            {
                OnInputRemoved(this);
            }
        }
    }
}
