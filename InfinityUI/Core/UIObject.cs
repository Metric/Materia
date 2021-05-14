using InfinityUI.Components;
using InfinityUI.Interfaces;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Spatial;
using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace InfinityUI.Core
{
    public class UIObject : IDisposable, IQuadComparable
    {
        public event Action<UIObject> Resize;
        public event Action<UIObject> ChildAdded;
        public event Action<UIObject> ChildRemoved;
        public event Action Reorder;

        public string ID { get; protected set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        public bool Visible { get; set; } = true;

        public bool RaycastTarget { get; set; } = false;

        /// <summary>
        /// determines if the children should always be raycast
        /// even if the point is outside the parent bounds
        /// </summary>
        /// <value>
        ///   <c>true</c> if [raycast always]; otherwise, <c>false</c>.
        /// </value>
        public bool RaycastAlways { get; set; } = false;

        public UIObject Parent { get; protected set; }

        public Box2 Padding { get; set; } = new Box2(0, 0, 0, 0);

        public Vector2 Position { get; set; } = Vector2.Zero;
        public Anchor RelativeTo { get; set; } = Anchor.BottomLeft;

        public Vector2 MinSize { get; set; } = Vector2.Zero;

        public SizeMode Sizing { get; set; } = SizeMode.Pixel;
        public SizeMode RelativeMode { get; set; } = SizeMode.Pixel;

        public Vector2 Origin { get; set; } = Vector2.Zero;

        protected int zOrder = 0;
        public int ZOrder
        {
            get
            {
                return zOrder;
            }
            set
            {
                if (zOrder != value)
                {
                    zOrder = value;
                    Reorder?.Invoke();
                }
            }
        }

        protected Vector2 size;
        public Vector2 Size
        {
            get
            {
                return size;
            }
            set
            {
                Vector2 prevSize = size;
                size = Vector2.MagnitudeMax(MinSize, value);
                if (prevSize != size)
                {
                    Resize?.Invoke(this);
                }
            }
        }

        protected Vector2 scale = new Vector2(1,1);
        public Vector2 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>
        /// Gets the rect
        /// </summary>
        /// <value>
        /// The rect.
        /// </value>
        public Box2 Rect
        {
            get
            {
                Vector2 size = WorldSize;
                return new Box2(WorldPosition, size.X, size.Y);
            }
        }

        /// <summary>
        /// Gets the local rect.
        /// This does not include scaling
        /// </summary>
        /// <value>
        /// The local rect.
        /// </value>
        public Box2 LocalRect
        {
            get
            {
                Vector2 size = AnchoredSize;
                return new Box2(AnchoredPosition, size.X, size.Y);
            }
        }

        public float Rotation { get; set; }

        public Matrix4 LocalMatrix
        {
            get
            {
                Vector2 size = AnchoredSize;
                Vector3 offset = new Vector3(size.X * Origin.X, size.Y * Origin.Y, 0);
                return Matrix4.CreateTranslation(-offset) * Matrix4.CreateRotationZ(Rotation * MathHelper.Deg2Rad) * Matrix4.CreateTranslation(offset);
            }
        }

        public Matrix4 WorldMatrix
        {
            get
            {
                if (Parent == null) return LocalMatrix;
                return LocalMatrix * Parent.WorldMatrix;
            }
        }

        public virtual Vector2 WorldScale
        {
            get
            {
                if (Parent == null) return Scale;
                return Scale * Parent.WorldScale;
            }
        }

        public virtual Vector2 WorldPosition
        {
            get
            {
                if (Parent == null) return AnchoredPosition;
                return AnchoredPosition + Parent.WorldPosition;
            }
        }

        public virtual Vector2 WorldSize
        {
            get
            {
                return AnchoredSize * WorldScale;
            }
        }

        public virtual Vector2 AnchoredPosition
        {
            get
            {
                Vector2 size = AnchoredSize;

                if (Parent == null) return Position + new Vector2(Padding.Left, Padding.Top);

                Vector2 pSize = Parent.AnchoredSize;

                //if RelativeMode is Percent we set local pos as a percent of parent size
                Vector2 pos = RelativeMode == SizeMode.Percent ? pSize * Position : Position;
                Vector2 offset = size * Origin;

                //calculate scale first
                pos -= offset;
                pos *= Scale;
                pos += offset;

                switch (RelativeTo)
                {
                    case Anchor.Top:
                        return new Vector2(pSize.X / 2 - size.X / 2 + pos.X + Padding.Left, pos.Y + Padding.Top);
                    case Anchor.Bottom:
                        return new Vector2(pSize.X / 2 - size.X / 2 + pos.X + Padding.Left, pSize.Y - pos.Y - Padding.Bottom - size.Y);
                    case Anchor.BottomLeft:
                    case Anchor.BottomHorizFill:
                        return new Vector2(pos.X + Padding.Left, pSize.Y - pos.Y - Padding.Bottom - size.Y);
                    case Anchor.TopLeft:
                    case Anchor.TopHorizFill:
                    case Anchor.Fill:
                        return pos + new Vector2(Padding.Left, Padding.Top);
                    case Anchor.BottomRight:
                        return new Vector2(pSize.X - pos.X - Padding.Right - size.X, pSize.Y - pos.Y - Padding.Bottom - size.Y);
                    case Anchor.TopRight:
                        return new Vector2(pSize.X - pos.X - Padding.Right - size.X, pos.Y + Padding.Top);
                    case Anchor.Center:
                        return new Vector2(pSize.X / 2 - size.X / 2 + pos.X + Padding.Left, pSize.Y / 2 - size.Y / 2 + pos.Y + Padding.Top);
                    case Anchor.CenterHorizFill:
                    case Anchor.CenterLeft:
                        return new Vector2(pos.X + Padding.Left, pSize.Y / 2 - size.Y / 2 + pos.Y + Padding.Top);
                    case Anchor.CenterRight:
                        return new Vector2(pSize.X - pos.X - Padding.Right - size.X, pSize.Y / 2 - size.Y / 2 + pos.Y + Padding.Top);
                }

                return pos + new Vector2(Padding.Left, Padding.Top);
            }
        }

        public virtual Vector2 AnchoredSize
        {
            get
            {
                Vector2 size = Size;
                Vector2 pSize = Parent == null ? Vector2.One : Parent.AnchoredSize;

                //if we are percent mode
                //then size is 0-1 only for percent
                //thus we need to calculate size based
                //on parent if there is one
                size = Parent == null || Sizing == SizeMode.Pixel ? size : new Vector2(size.X * pSize.X, size.Y * pSize.Y);

                if (Parent == null) return size + new Vector2(-(Padding.Right + Padding.Left), -(Padding.Bottom + Padding.Top));
                switch (RelativeTo)
                {
                    case Anchor.BottomHorizFill:
                    case Anchor.CenterHorizFill:
                    case Anchor.TopHorizFill:
                        return new Vector2(pSize.X, size.Y) + new Vector2(-(Padding.Right + Padding.Left), -(Padding.Bottom + Padding.Top));
                    case Anchor.Fill:
                        return pSize + new Vector2(-(Padding.Right + Padding.Left), -(Padding.Bottom + Padding.Top));
                }

                return size + new Vector2(-(Padding.Right + Padding.Left), -(Padding.Bottom + Padding.Top));
            }
        }

        public UICanvas Canvas { get; set; }

        public List<UIObject> Children { get; protected set; } = new List<UIObject>();

        protected Dictionary<Type, IComponent> components = new Dictionary<Type, IComponent>();

        public UIObject()
        {
            UI.Add(this); //add to object cache to ensure cleanup for UI.Dispose()
        }

        public void AddComponent<T>(T c) where T : IComponent
        {
            if (c == null)
            {
                return;
            }

            c.Parent = this;
            components[c.GetType()] = c;
            c.Awake();
        }

        public T AddComponent<T>() where T : IComponent
        {
            Type type = typeof(T);
            try
            {
                IComponent c = (IComponent)Activator.CreateInstance(type);

                if (c == null)
                {
                    return default(T);
                }

                c.Parent = this;
                components[type] = c;
                c.Awake();
                return (T)c;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace.ToString());
            }

            return default(T);
        }

        public void RemoveComponent<T>() where T : IComponent
        {
            IComponent c;
            components.TryGetValue(typeof(T), out c);

            if (c != null)
            {
                c.Dispose();
                c.Parent = null;
            }

            components.Remove(typeof(T));
        }

        public bool HasComponent<T>() where T : IComponent
        {
            return components.ContainsKey(typeof(T));
        }

        public T GetComponent<T>() where T : IComponent
        {
            IComponent c;
            components.TryGetValue(typeof(T), out c);
            return (T)c;
        }

        public virtual void Update() { }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing = true)
        {
            for (int i = 0; i < Children.Count; ++i)
            {
                var ele = Children[i];

                if (ele == null) continue;

                ele.Dispose(disposing);
                ele.Parent = null;
            }

            Children.Clear();

            var comps = components.Values.ToList();

            for (int i = 0; i < comps.Count; ++i)
            {
                comps[i]?.Dispose();
            }

            components.Clear();
        }

        protected bool IsPointInPolygon(ref Vector2 p)
        {
            Vector2 pos = WorldPosition;
            Vector2 size = WorldSize;

            Vector2 topLeft = pos;
            Vector2 topRight = pos + new Vector2(size.X, 0);
            Vector2 bottomLeft = pos + new Vector2(0, size.Y);
            Vector2 bottomRight = pos + size;

            topLeft = ToWorld(ref topLeft); // ToWorld(ref topLeft, e);
            topRight = ToWorld(ref topRight); // ToWorld(ref topRight, e);
            bottomLeft = ToWorld(ref bottomLeft); // ToWorld(ref bottomLeft, e);
            bottomRight = ToWorld(ref bottomRight); // ToWorld(ref bottomRight, e);

            List<Vector2> pts = new List<Vector2>
            {
                bottomLeft,
                topLeft,
                topRight,
                bottomRight
            };

            return UI.PointInPolygon(pts, ref p);
        }

        public virtual Vector2 ToWorld(ref Vector2 p)
        {
            Vector4 rot = new Vector4(p.X, p.Y, 0, 1) * WorldMatrix;
            return rot.Xy;
        }

        public virtual bool Contains(ref Vector2 p)
        {
            if (!Visible) return false;

            float s = Scale.LengthSquared;

            if (Rotation > 0 || Rotation < 0
                || s < 1 || s > 1)
            {
                return IsPointInPolygon(ref p);
            }

            return Rect.Contains(p);
        }

        public virtual bool Contains(UIObject e)
        {
            return Rect.Contains(e.Rect);
        }

        public UIObject Pick(ref Vector2 p)
        {
            if (!Visible || (!RaycastAlways && !Contains(ref p))) return null;

            for (int i = Children.Count - 1; i >= 0; --i)
            {
                if (!Children[i].RaycastTarget || !Children[i].Visible) continue;
                if (Children[i].RaycastAlways || Children[i].Contains(ref p))
                {
                    var c = Children[i].Pick(ref p);
                    if (c == null)
                    {
                        return Children[i].RaycastTarget ? Children[i] : null;
                    }

                    return c.RaycastTarget ? c : null;
                }
            }

            return RaycastTarget ? this : null;
        }

        public virtual void InsertChild(int i, UIObject e)
        {
            if (e == null) return;

            e.Parent?.RemoveChild(e);

            if (i >= Children.Count)
            {
                AddChild(e);
                return;
            }

            e.Canvas = Canvas;

            e.Parent = this;
            Children.Insert(i, e);
            Children.Sort(Compare);
            e.Reorder += E_Reorder;
            ChildAdded?.Invoke(e);
        }

        public virtual void AddChild(UIObject e)
        {
            if (e == null) return;

            e.Parent?.RemoveChild(e);

            e.Canvas = Canvas;

            e.Parent = this;

            Children.Add(e);
            Children.Sort(Compare);

            e.Reorder += E_Reorder;

            ChildAdded?.Invoke(e);
        }

        private void E_Reorder()
        {
            Children.Sort(Compare);
        }

        public virtual void RemoveChild(UIObject e)
        {
            if (e == null) return;
            if (e.Parent != this) return;
            e.Reorder -= E_Reorder;
            e.Parent = null;
            Children.Remove(e);
            ChildRemoved?.Invoke(e);
        }

        protected virtual int Compare(UIObject a, UIObject b)
        {
            return b.zOrder - a.zOrder;
        }

        protected static void TryAndSendMessage(IComponent c, string fn, params object[] args)
        {
            if (string.IsNullOrEmpty(fn)) return;
            if (c == null) return;

            try
            {
                Type t = c.GetType();
                MethodInfo method = null;

                Type[] types = new Type[args.Length];

                for (int i = 0; i < args.Length; ++i)
                {
                    types[i] = args[i].GetType();
                }

                method = t.GetMethod(fn, types);
                method?.Invoke(c, args);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace.ToString());
            }
        }

        public virtual void SendMessageUpwards(string fn, params object[] args)
        {
            try
            {
                foreach (var c in components.Values)
                {
                    TryAndSendMessage(c, fn, args);
                }
            }
            catch { }

            UIObject parent = Parent;

            while (parent != null)
            {
                if (args != null & args.Length > 0)
                {
                    var arg = args[0];
                    if (arg is UIEventArgs)
                    {
                        if ((arg as UIEventArgs).IsHandled)
                        {
                            return;
                        }
                    }
                }

                parent.SendMessage(fn, false, args);
                parent = parent.Parent;
            }
        }

        public virtual void SendMessage(string fn, bool toChildren, params object[] args)
        {
            try
            {
                foreach (var c in components.Values)
                {
                    TryAndSendMessage(c, fn, args);
                }
            }
            catch { }

            if (!toChildren) return;

            Stack<UIObject> queue = new Stack<UIObject>();

            for (int i = Children.Count - 1; i >= 0; --i)
            {
                var c = Children[i];
                if (c == null || !c.Visible) continue;
                queue.Push(c);
            }

            while (queue.Count > 0)
            {
                var c = queue.Pop();
                if (c != null)
                {
                    c.SendMessage(fn, false, args);

                    var childs = c.Children;
                    for (int i = childs.Count - 1; i >= 0; --i)
                    {
                        var subchild = childs[i];
                        if (subchild == null || !subchild.Visible) continue;
                        queue.Push(subchild);
                    }
                }
            }
        }
    }
}
