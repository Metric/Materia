using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class NodeOutput
    {
        public delegate void OutputChanged(NodeOutput inp);
        public event OutputChanged OnInputAdded;
        public event OutputChanged OnInputRemoved;

        public event OutputChanged OnTypeChanged;

        public List<NodeInput> To { get; protected set; }

        protected NodeType type;
        public NodeType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                if(OnTypeChanged != null)
                {
                    OnTypeChanged.Invoke(this);
                }
            }
        }

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
            type = t;
            Node = n;
            Name = name;
            To = new List<NodeInput>();
        }

        public bool InsertAt(int index, NodeInput inp, bool triggerAddEvent = true)
        {
            if ((inp.Type & Type) != 0)
            {
                if (inp.Input != null)
                {
                    inp.Input.Remove(inp);
                }

                inp.Input = this;
                if (index >= To.Count)
                {
                    To.Add(inp);
                }
                else
                {
                    To.Insert(index, inp);
                }

                if (triggerAddEvent)
                {
                    if (OnInputAdded != null)
                    {
                        OnInputAdded(this);
                    }
                }

                return true;
            }

            return false;
        }

        public bool Add(NodeInput inp, bool triggerAddEvent = true)
        {
            if((inp.Type & Type) != 0)
            {
                if(inp.Input != null)
                {
                    inp.Input.Remove(inp);
                }

                if (triggerAddEvent)
                {
                    inp.Input = this;
                }
                else
                {
                    inp.AssignInput(this);
                }

                To.Add(inp);

                if (triggerAddEvent)
                {
                    if (OnInputAdded != null)
                    {
                        OnInputAdded(this);
                    }
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
