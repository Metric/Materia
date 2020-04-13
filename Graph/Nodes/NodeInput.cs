using Materia.Rendering.Attributes;

namespace Materia.Nodes
{
    public class NodeInput
    {
        public delegate void InputChanged(NodeInput n);
        public event InputChanged OnInputChanged;
        public Node ParentNode { get; protected set; }
        public Node Node { get; protected set; }

        public string Name { get; set; }

        NodeOutput reference;
        public NodeOutput Reference
        {
            get
            {
                return reference;
            }
            set
            {
                reference = value;
                OnInputChanged?.Invoke(this);
                if (Node != null)
                {
                    if (Node is MathNode)
                    {
                        MathNode n = Node as MathNode;
                        n.UpdateOutputType();
                    }
                    else
                    {
                        Node.TriggerValueChange();
                    }
                }
            }
        }

        public object Data
        {
            get
            {
                if (HasInput)
                {
                    return Reference.Data;
                }

                return null;
            }
        }

        public bool HasInput
        {
            get
            {
                return reference != null;
            }
        }

        public bool IsValid
        {
            get
            {
                return HasInput && Reference.Data != null;
            }
        }

        public NodeType Type { get; set; }
        
        public void AssignReference(NodeOutput output)
        {
            reference = output;
        }

        public NodeInput(NodeType t, Node n, string name = "")
        {
            Type = t;
            Name = name;
            Node = n;
            ParentNode = n;
        }

        public NodeInput(NodeType t, Node n, Node parent, string name = "")
        {
            Type = t;
            Name = name;
            Node = n;
            ParentNode = parent;
        }
    }
}
