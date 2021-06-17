﻿using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;

namespace Materia.Nodes
{
    public class NodeInput : INodePoint
    {
        public delegate void InputChanged(NodeInput n);
        public event InputChanged OnInputChanged;

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

        /// <summary>
        /// Gets the active buffer.
        /// Basically a shortcut to Node?.GetActiveBuffer()
        /// </summary>
        /// <returns></returns>
        public GLTexture2D GetActiveBuffer()
        {
            return Node?.GetActiveBuffer();
        }

        public NodeInput(NodeType t, Node n, string name = "")
        {
            Type = t;
            Name = name;
            Node = n;
        }

        public void Dispose()
        {
            reference?.Remove(this);
            reference = null;
        }
    }
}
