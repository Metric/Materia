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

        public UIObject Parent { get; protected set; }

        public Box2 Padding { get; set; } = new Box2(0, 0, 0, 0);

        public Vector2 Position { get; set; } = Vector2.Zero;
        public Anchor RelativeTo { get; set; } = Anchor.BottomLeft;

        public Vector2 MinSize { get; set; } = Vector2.Zero;

        public SizeMode Sizing { get; set; } = SizeMode.Pixel;

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
        /// Gets the rect.
        /// This does not include scaling
        /// </summary>
        /// <value>
        /// The rect.
        /// </value>
        public Box2 Rect
        {
            get
            {
                return new Box2(AnchoredPosition, AnchoredSize.X, AnchoredSize.Y);
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
                return new Box2(Position, AnchoredSize.X, AnchoredSize.Y);
            }
        }

        public float Rotation { get; set; }

        public Vector2 Origin { get; set; } = new Vector2(0f, 0f);

        public Matrix4 LocalScaleMatrix
        {
            get
            {
                return Matrix4.CreateTranslation(-Origin.X * AnchoredSize.X, -Origin.Y * AnchoredSize.Y, 0) * Matrix4.CreateScale(scale.X, scale.Y, 1) * Matrix4.CreateTranslation(Origin.X * AnchoredSize.X, Origin.Y * AnchoredSize.Y, 0);
            }
        }

        public Matrix4 LocalRotationMatrix
        {
            get
            {
                return Matrix4.CreateTranslation(-Origin.X * AnchoredSize.X, -Origin.Y * AnchoredSize.Y, 0) * Matrix4.CreateRotationZ(Rotation * MathHelper.Deg2Rad) * Matrix4.CreateTranslation(Origin.X * AnchoredSize.X, Origin.Y * AnchoredSize.Y, 0);
            }
        }

        public Matrix4 LocalMatrix
        {
            get
            {
                return LocalScaleMatrix * LocalRotationMatrix;
            }
        }

        public Matrix4 ModelMatrix
        {
            get
            {
                if (Parent == null) return LocalMatrix;
                return LocalMatrix * Parent.ModelMatrix;
            }
        }

        public virtual Vector2 AnchoredPosition
        {
            get
            {
                Vector2 size = Size;

                switch (Sizing)
                {
                    //if we are percent mode
                    //then size is 0-1 only for percent
                    //thus we need to calculate size based
                    //on parent if there is one
                    case SizeMode.Percent:
                        size = Parent == null ? size : new Vector2(size.X * Parent.AnchoredSize.X, size.Y * Parent.AnchoredSize.Y);
                        break;
                }

                if (Parent == null) return Position + new Vector2(Padding.Left, Padding.Top);

                switch (RelativeTo)
                {
                    case Anchor.Top:
                        return new Vector2(Parent.AnchoredSize.X / 2 - size.X / 2 + Position.X + Padding.Left, Position.Y + Padding.Top) + Parent.AnchoredPosition;
                    case Anchor.Bottom:
                        return new Vector2(Parent.AnchoredSize.X / 2 - size.X / 2 + Position.X + Padding.Left, Parent.AnchoredSize.Y - Position.Y - Padding.Bottom - size.Y) + Parent.AnchoredPosition;
                    case Anchor.BottomLeft:
                    case Anchor.BottomHorizFill:
                        return new Vector2(Position.X + Padding.Left, Parent.AnchoredSize.Y - Position.Y - Padding.Bottom - size.Y) + Parent.AnchoredPosition;
                    case Anchor.TopLeft:
                    case Anchor.TopHorizFill:
                        return Position + new Vector2(Padding.Left, Padding.Top) + Parent.AnchoredPosition;
                    case Anchor.BottomRight:
                        return new Vector2(Parent.AnchoredSize.X - Position.X - Padding.Right - size.X, Parent.Size.Y - Position.Y - Padding.Bottom - Size.Y) + Parent.AnchoredPosition;
                    case Anchor.TopRight:
                        return new Vector2(Parent.AnchoredSize.X - Position.X - Padding.Right - size.X, Position.Y + Padding.Top) + Parent.AnchoredPosition;
                    case Anchor.Fill:
                        return new Vector2(Padding.Left, Padding.Top) + Parent.AnchoredPosition;
                    case Anchor.Center:
                        return new Vector2(Parent.AnchoredSize.X / 2 - size.X / 2 + Position.X + Padding.Left, Parent.AnchoredSize.Y / 2 - size.Y / 2 + Position.Y + Padding.Top) + Parent.AnchoredPosition;
                    case Anchor.CenterHorizFill:
                    case Anchor.CenterLeft:
                        return new Vector2(Position.X + Padding.Left, Parent.AnchoredSize.Y / 2 - size.Y / 2 + Position.Y + Padding.Top) + Parent.AnchoredPosition;
                    case Anchor.CenterRight:
                        return new Vector2(Parent.AnchoredSize.X - Position.X - Padding.Right - size.X, Parent.AnchoredSize.Y / 2 - size.Y / 2 + Position.Y + Padding.Top) + Parent.AnchoredPosition;
                }

                return Position + new Vector2(Padding.Left, Padding.Top) + Parent.AnchoredPosition;
            }
        }

        public virtual Vector2 AnchoredSize
        {
            get
            {
                Vector2 size = Size;

                switch (Sizing)
                {
                    //if we are percent mode
                    //then size is 0-1 only for percent
                    //thus we need to calculate size based
                    //on parent if there is one
                    case SizeMode.Percent:
                        size = Parent == null ? size : new Vector2(size.X * Parent.AnchoredSize.X, size.Y * Parent.AnchoredSize.Y);
                        break;
                }

                if (Parent == null) return size + new Vector2(-(Padding.Right + Padding.Left), -(Padding.Bottom + Padding.Top));
                switch (RelativeTo)
                {
                    case Anchor.BottomHorizFill:
                    case Anchor.CenterHorizFill:
                    case Anchor.TopHorizFill:
                        return new Vector2(Parent.AnchoredSize.X, size.Y) + new Vector2(-(Padding.Right + Padding.Left), -(Padding.Bottom + Padding.Top));
                    case Anchor.Fill:
                        return Parent.AnchoredSize + new Vector2(-(Padding.Right + Padding.Left), -(Padding.Bottom + Padding.Top));
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
            Vector2 topLeft = AnchoredPosition;
            Vector2 topRight = AnchoredPosition + new Vector2(AnchoredSize.X, 0);
            Vector2 bottomLeft = AnchoredPosition + new Vector2(0, AnchoredSize.Y);
            Vector2 bottomRight = AnchoredPosition + AnchoredSize;

            topLeft = ToWorld(ref topLeft); // ToWorld(ref topLeft, e);
            topRight = ToWorld(ref topRight); // ToWorld(ref topRight, e);
            bottomLeft = ToWorld(ref bottomLeft); // ToWorld(ref bottomLeft, e);
            bottomRight = ToWorld(ref bottomRight); // ToWorld(ref bottomRight, e);

            List<Vector2> pts = new List<Vector2>();
            pts.Add(bottomLeft);
            pts.Add(topLeft);
            pts.Add(topRight);
            pts.Add(bottomRight);

            return UI.PointInPolygon(pts, ref p);
        }

        public virtual Vector2 ToWorld(ref Vector2 p)
        {
            Vector4 rot = new Vector4(p.X, p.Y, 0, 1) * LocalMatrix; //todo: test with ModelMatrix
            return rot.Xy;
        }

        public virtual bool Contains(ref Vector2 p)
        {
            if (!Visible) return false;

            if (Rotation > 0 || Rotation < 0 
                || MathF.Abs(scale.X - 1) > float.Epsilon  
                || MathF.Abs(scale.Y - 1) > float.Epsilon)
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
            if (!Contains(ref p)) return null;

            for (int i = Children.Count - 1; i >= 0; --i)
            {
                if (Children[i].Contains(ref p))
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
                if (c == null) continue;
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
                        if (subchild == null) continue;
                        queue.Push(subchild);
                    }
                }
            }
        }
    }
}
