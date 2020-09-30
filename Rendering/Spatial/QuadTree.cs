using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows;

namespace Materia.Rendering.Spatial
{
    public interface IQuadComparable
    {
        Box2 Rect { get; }
        string ID { get; }
    }

    public class QuadNode
    {
        public IQuadComparable Node { get; protected set; }
        public QuadNode Parent { get; set; }
        public QuadNode Next { get; set; }

        public QuadNode(IQuadComparable n, QuadNode parent = null)
        {
            Parent = parent;
            Node = n;
        }
    }

    public class Quadrant
    {
        public Quadrant Parent { get; protected set; }
        public Box2 Rect { get; protected set; }
        public uint Depth { get; protected set; }
        public QuadNode Nodes { get; protected set; }
        public Quadrant TopLeft { get; protected set; }
        public Quadrant TopRight { get; protected set; }
        public Quadrant BottomLeft { get; protected set; }
        public Quadrant BottomRight { get; protected set; }

        public Quadrant(Quadrant parent, Box2 rect, uint depth = 0)
        {
            Parent = parent;
            Rect = rect;
            Depth = depth;
        }

        public bool Remove(IQuadComparable node)
        {
            bool rc = false;
            if (Nodes == null) return rc;
            QuadNode n = Nodes;
            while (n != null && n.Node != node)
            {
                n = n.Parent;
            }
            if (n != null)
            {
                rc = true;
                if (n == Nodes)
                {
                    Nodes = n.Parent;
                }
                if (n.Parent != null)
                {
                    n.Parent.Next = n.Next;
                }
                if (n.Next != null)
                {
                    n.Next.Parent = n.Parent;
                }

                n.Parent = null;
                n.Next = null;
            }
            return rc;
        }

        public void GetIntersectingNodes<T>(List<T> nodes, ref Box2 rect) where T : IQuadComparable
        {
            float w = Rect.Width * 0.5f;
            float h = Rect.Height * 0.5f;
            float x = (Rect.Left + Rect.Right) * 0.5f;
            float y = (Rect.Top + Rect.Bottom) * 0.5f;

            float l = Rect.Left;
            float t = Rect.Top;

            Box2 topLeft = Box2.FromDimensions(l, t, w, h);
            Box2 topRight = Box2.FromDimensions(x, t, w, h);
            Box2 bottomLeft = Box2.FromDimensions(l, y, w, h);
            Box2 bottomRight = Box2.FromDimensions(x, y, w, h);

            if (topLeft.Intersects(rect) && TopLeft != null)
            {
                TopLeft.GetIntersectingNodes(nodes, ref rect);
            }
            if (topRight.Intersects(rect) && TopRight != null)
            {
                TopRight.GetIntersectingNodes(nodes, ref rect);
            }
            if (bottomLeft.Intersects(rect) && BottomLeft != null)
            {
                BottomLeft.GetIntersectingNodes(nodes, ref rect);
            }
            if (bottomRight.Intersects(rect) && BottomRight != null)
            {
                BottomRight.GetIntersectingNodes(nodes, ref rect);
            }

            GetIntersectingQuadNodes<T>(Nodes, nodes, ref rect);
        }

        protected void GetIntersectingQuadNodes<T>(QuadNode first, List<T> nodes, ref Box2 rect) where T : IQuadComparable
        {
            if (first == null)
            {
                return;
            }
            QuadNode n = first;
            do
            {
                if (n.Node.Rect.Intersects(rect) && (n.Node is T))
                {
                    nodes.Add((T)n.Node);
                }
                n = n.Parent;
            } while (n != null);
        }

        public Quadrant Insert(IQuadComparable node)
        {
            Quadrant toInsert = this;
            while (true)
            {
                if (toInsert.Depth >= QuadTree.MAX_DEPTH)
                {
                    if (toInsert.Nodes == null)
                    {
                        toInsert.Nodes = new QuadNode(node);
                    }
                    else
                    {
                        toInsert.Nodes.Next = new QuadNode(node, toInsert.Nodes);
                        toInsert.Nodes = toInsert.Nodes.Next;
                    }
                    return toInsert;
                }

                float w = toInsert.Rect.Width * 0.5f;
                if (w < 1)
                {
                    w = 1;
                }
                float h = toInsert.Rect.Height * 0.5f;
                if (h < 1)
                {
                    h = 1;
                }
                float x = (toInsert.Rect.Left + toInsert.Rect.Right) * 0.5f;
                float y = (toInsert.Rect.Top + toInsert.Rect.Bottom) * 0.5f;


                float l = Rect.Left;
                float t = Rect.Top;

                Box2 topLeft = Box2.FromDimensions(l, t, w, h);
                Box2 topRight = Box2.FromDimensions(x, t, w, h);
                Box2 bottomLeft = Box2.FromDimensions(l, y, w, h);
                Box2 bottomRight = Box2.FromDimensions(x, y, w, h);

                Quadrant child = null;

                if (topLeft.Contains(node.Rect))
                {
                    if (toInsert.TopLeft == null)
                    {
                        toInsert.TopLeft = new Quadrant(toInsert, topLeft, toInsert.Depth + 1);
                    }
                    child = toInsert.TopLeft;
                }
                else if(topRight.Contains(node.Rect))
                {
                    if (toInsert.TopRight == null)
                    {
                        toInsert.TopRight = new Quadrant(toInsert, topRight, toInsert.Depth + 1);
                    }
                    child = toInsert.TopRight;
                }
                else if(bottomLeft.Contains(node.Rect))
                {
                    if (toInsert.BottomLeft == null)
                    {
                        toInsert.BottomLeft = new Quadrant(toInsert, bottomLeft, toInsert.Depth + 1);
                    }
                    child = toInsert.BottomLeft;
                }
                else if(bottomRight.Contains(node.Rect))
                {
                    if (toInsert.BottomRight == null)
                    {
                        toInsert.BottomRight = new Quadrant(toInsert, bottomRight, toInsert.Depth + 1);
                    }
                    child = toInsert.BottomRight;
                }

                if (child != null)
                {
                    toInsert = child;
                }
                else
                {
                    if (toInsert.Nodes == null)
                    {
                        toInsert.Nodes = new QuadNode(node);
                    }
                    else
                    {
                        toInsert.Nodes.Next = new QuadNode(node, toInsert.Nodes);
                        toInsert.Nodes = toInsert.Nodes.Next;
                    }
                    return toInsert;
                }
            }
        }
    }

    public class QuadTree
    {
        public static uint MAX_DEPTH = 32;

        public Box2 Rect { get; set; }
        protected Dictionary<string, Quadrant> table;
        protected Quadrant root;

        public QuadTree(Box2 rect)
        {
            table = new Dictionary<string, Quadrant>();
            Rect = rect;
        }

        public void Insert(IQuadComparable n)
        {
            if (root == null)
            {
                root = new Quadrant(null, Rect);
            }

            var quadrant = root.Insert(n);
            table[n.ID] = quadrant;
        }

        public bool Remove(IQuadComparable n)
        {
            Quadrant q = null;
            if (table.TryGetValue(n.ID, out q))
            {
                return q.Remove(n);
            }

            return false;
        }

        public void Clear()
        {
            table.Clear();
            root = null;
        }

        public List<T> Get<T>(ref Box2 rect) where T : IQuadComparable
        {
            List<T> items = new List<T>();
            if (root == null) return items;
            root.GetIntersectingNodes<T>(items, ref rect);
            return items;
        }
    }
}
