using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes
{
    public enum NodeType
    {
        Color = 2,
        Gray = 4,
        Float = 8,
        Float2 = 16,
        Float3 = 32,
        Float4 = 64,
        Bool = 128,
        Execute = 256,
        Matrix = 512,
    }

    public class NodeInput
    {
        public delegate void InputRemovedEvent(NodeInput n);
        public delegate void InputAddedEvent(NodeInput n);
        public delegate void InputChangedEvent(NodeInput n);

        public event InputRemovedEvent OnInputRemoved;
        public event InputAddedEvent OnInputAdded;
        public event InputChangedEvent OnInputChanged;

        public Node Node { get; protected set; }

        public string Name { get; set; }

        NodeOutput input;
        public NodeOutput Input
        {
            get
            {
                return input;
            }
            set
            {
                if(value == null && input != null)
                {
                    input = value;
                    if (OnInputRemoved != null) OnInputRemoved.Invoke(this);
                }
                else if(value != null && input == null)
                {
                    input = value;
                    if (OnInputAdded != null) OnInputAdded.Invoke(this);
                }
                else
                {
                    input = value;
                    if (OnInputAdded != null) OnInputAdded.Invoke(this);
                }
            }
        }

        public bool HasInput
        {
            get
            {
                return input != null;
            }
        }

        public NodeType Type { get; protected set; }
        
        public NodeInput(NodeType t, Node n, string name = "")
        {
            Type = t;
            Name = name;
            Node = n;
        }

        public void AssignInput(NodeOutput oinp)
        {
            input = oinp;
        }

        public void InputDataChanged()
        {
            if(OnInputChanged != null)
            {
                OnInputChanged.Invoke(this);
            }
        }
    }
}
