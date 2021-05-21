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

        public bool IsDirty { get; private set; } = false;

        public UIObject ClippingParent { get; private set; }

        private bool visible = true;
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;

                    //call resize because
                    //technically we have visually
                    //but not physically
                    //and thus allow components
                    //and etc to update properly
                    Resize?.Invoke(this);
                }
            }
        }

        public bool RaycastTarget { get; set; } = false;

        /// <summary>
        /// determines if the children should always be raycast
        /// even if the point is outside the parent bounds
        /// </summary>
        /// <value>
        ///   <c>true</c> if [raycast always]; otherwise, <c>false</c>.
        /// </value>
        public bool RaycastAlways { get; set; } = false;

        private UIObject parent = null;
        public UIObject Parent
        {
            get => parent;
        }

        private Anchor relativeTo = Anchor.TopLeft;
        public Anchor RelativeTo
        {
            get => relativeTo;
            set
            {
                if (relativeTo != value)
                {
                    relativeTo = value;
                    IsDirty = true;
                    UpdateMatrix();
                }
            }
        }

        private Box2 margin = new Box2(0, 0, 0, 0);
        public Box2 Margin
        {
            get => margin;
            set
            {
                if (margin != value)
                {
                    margin = value;
                    IsDirty = true;
                    UpdateMatrix();
                }
            }
        }

        private Box2 padding = new Box2(0, 0, 0, 0);
        public Box2 Padding
        {
            get => padding;
            set
            {
                if (padding != value)
                {
                    padding = value;
                    IsDirty = true;
                    UpdateMatrix();
                }
            }
        }

        private Vector2 position = Vector2.Zero;
        public Vector2 Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    IsDirty = true;
                    UpdateMatrix();
                }
            }
        }

        public Vector2 MinSize { get; set; } = Vector2.Zero;

        private Vector2 origin = Vector2.Zero;
        public Vector2 Origin
        {
            get => origin;
            set
            {
                if (origin != value)
                {
                    origin = value;
                    IsDirty = true;
                    UpdateMatrix();
                }
            }
        }

        private int zOrder = 0;
        public int ZOrder
        {
            get => zOrder;
            set
            {
                if (zOrder != value)
                {
                    zOrder = value;
                    Reorder?.Invoke();
                }
            }
        }

        private Vector2 size;
        public Vector2 Size
        {
            get => size;
            set
            {
                Vector2 prevSize = size;
                size = Vector2.MagnitudeMax(MinSize, value);
                if (prevSize != size)
                {
                    IsDirty = true;
                    UpdateMatrix();
                    Resize?.Invoke(this);
                }
            }
        }

        private Vector2 scale = new Vector2(1,1);
        public Vector2 Scale
        {
            get => scale;
            set
            {
                if (scale != value)
                {
                    scale = value;
                    IsDirty = true;
                    UpdateMatrix();
                }
            }
        }

        private Vector2 worldSize = Vector2.Zero;
        public Vector2 WorldSize
        {
            get => worldSize;
        }

        private Vector2 worldScale = Vector2.One;
        public Vector2 WorldScale
        {
            get => worldScale;
        }

        private Vector2 worldPosition = Vector2.Zero;
        public Vector2 WorldPosition
        {
            get => worldPosition;
        }

        private Matrix4 worldMatrix = Matrix4.Identity;
        public Matrix4 WorldMatrix
        {
            get => worldMatrix;
        }

        private Vector2 anchorSize = Vector2.Zero;
        public Vector2 AnchorSize
        {
            get => anchorSize;
        }

        private Vector2 anchorPosition = Vector2.Zero;
        public Vector2 AnchorPosition
        {
            get => anchorPosition;
        }

        private Box2 rect = new Box2(0, 0, 0, 0);
        /// <summary>
        /// Gets the local rect.
        /// This does not include scaling
        /// </summary>
        /// <value>
        /// The local rect.
        /// </value>
        public Box2 Rect
        {
            get => rect;
        }

        private Box2 visibleRect;
        public Box2 VisibleRect
        {
            get => visibleRect;
        }

        private Box2 extendedRect;
        public Box2 ExtendedRect
        {
            get => extendedRect;
        }

        private Box2 anchoredRect = new Box2(0, 0, 0, 0);
        public Box2 AnchoredRect
        {
            get => anchoredRect;
        }

        private float rotation = 0;
        public float Rotation
        {
            get => rotation;
            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    IsDirty = true;
                    UpdateMatrix();
                }
            }
        }

        private Matrix4 localMatrix = Matrix4.Identity;
        public Matrix4 LocalMatrix
        {
            get => localMatrix;
        }

        public bool IsClipped
        {
            get
            {
                var drawable = FindComponent<UIDrawable>();
                if (drawable == null) return false;
                return drawable.Clip;
            }
        }

        public UICanvas Canvas { get; set; }

        public List<UIObject> Children { get; protected set; } = new List<UIObject>();

        protected Dictionary<Type, IComponent> components = new Dictionary<Type, IComponent>();
        protected List<IComponent> componentList = new List<IComponent>();

        public UIObject()
        {
            UI.Add(this); //add to object cache to ensure cleanup for UI.Dispose()
        }

        public void AddComponent<T>(T c) where T : IComponent
        {
            if (c == null) return;

            c.Parent = this;
            components[c.GetType()] = c;
            componentList.Add(c);
            c.Awake();
        }

        public T FindComponent<T>() where T : IComponent
        {
            return (T)componentList.Find(m => m is T);
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
                componentList.Add(c);
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
                componentList.Remove(c);
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

        public virtual void Update() 
        {
            bool wasDirty = IsDirty;
            IsDirty = false;

            if (wasDirty)
            {
                UpdateMatrix(false);
            }

            Box2 b = rect;

            for (int i = 0; i < Children.Count; ++i)
            {
                Children[i].ClippingParent = IsClipped ? this : ClippingParent;
                Children[i].IsDirty = wasDirty;
                //make sure canvas is passed down properly
                Children[i].Canvas = Canvas;
                Children[i].Update();
                if (Children[i].Visible)
                {
                    b.Encapsulate(Children[i].rect);
                    b.Encapsulate(Children[i].visibleRect);
                }
            }

            visibleRect = b;

            for (int i = 0; i < componentList.Count; ++i)
            {
                var c = componentList[i];
                c.Update();
                if (c is ILayout)
                {
                    var layout = c as ILayout;
                    layout?.Invalidate();
                }
            }

            if (this is ILayout)
            {
                var layout = (this as ILayout);
                layout?.Invalidate();
            }
        }

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
                ele.parent = null;
            }

            Children.Clear();

            var comps = componentList;

            for (int i = 0; i < comps.Count; ++i)
            {
                comps[i]?.Dispose();
            }

            components.Clear();
            componentList.Clear();
        }

        protected bool IsPointInPolygon(ref Vector2 p)
        {
            Vector2 pos = worldPosition;
            Vector2 size = worldSize;

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
            Vector4 rot = new Vector4(p.X, p.Y, 0, 1) * LocalMatrix;
            return rot.Xy;
        }

        public virtual bool Contains(ref Vector2 p)
        {
            if (!Visible) return false;

            float s = worldScale.LengthSquared;

            if (rotation > 0 || rotation < 0
                || s < 2 || s > 2)
            {
                return IsPointInPolygon(ref p);
            }

            return Rect.Contains(p);
        }

        public virtual bool Contains(UIObject e)
        {
            return Rect.Contains(e.Rect);
        }

        public virtual bool InVisibleArea(ref Vector2 p)
        {
            return visibleRect.Contains(p);
        }

        public UIObject Pick(ref Vector2 p)
        {
            if (!Visible) return null;

            UIObject c;
            for (int i = Children.Count - 1; i >= 0; --i)
            {
                if (!Children[i].RaycastTarget || !Children[i].Visible) continue;
                if (Children[i].RaycastAlways 
                    || (!Children[i].IsClipped && Children[i].InVisibleArea(ref p)) 
                    || (Children[i].IsClipped && Children[i].Contains(ref p)))
                {
                    c = Children[i].Pick(ref p);
                    if (c == null)
                    {
                        if ((Children[i].RaycastTarget && Children[i].Contains(ref p)) || (Children[i].RaycastTarget && Children[i].RaycastAlways))
                        {
                            return Children[i];
                        }
                    }

                    if (c != null && ((c.RaycastTarget && c.Contains(ref p)) || (c.RaycastTarget && c.RaycastAlways)))
                    {
                        return c;
                    }
                }
            }

            return (RaycastTarget && Contains(ref p)) || (RaycastTarget && RaycastAlways) ? this : null;
        }

        public virtual void InsertChild(int i, UIObject e)
        {
            if (e == null) return;

            e.parent?.RemoveChild(e, false);

            if (i >= Children.Count)
            {
                AddChild(e);
                return;
            }

            e.Canvas = Canvas;
            e.parent = this;

            Children.Insert(i, e);
            Children.Sort(Compare);
            e.Reorder += E_Reorder;
            ChildAdded?.Invoke(e);
            IsDirty = true;
        }

        public virtual void AddChild(UIObject e)
        {
            if (e == null) return;

            e.parent?.RemoveChild(e, false);
            e.Canvas = Canvas;

            e.parent = this;

            Children.Add(e);
            Children.Sort(Compare);
            e.Reorder += E_Reorder;
            ChildAdded?.Invoke(e);
            IsDirty = true;
        }

        public virtual void ClearChildren()
        {
            for (int i = 0; i < Children.Count; ++i)
            {
                RemoveChild(Children[i]);
                Children[i]?.Dispose();
                --i;
            }
        }

        public virtual int IndexOf(UIObject e)
        {
            if (e == null) return -1;
            if (e.parent != this) return -1;
            return Children.IndexOf(e);
        }

        private void E_Reorder()
        {
            Children.Sort(Compare);
        }

        public virtual bool RemoveChild(UIObject e, bool markDirty = true)
        {
            if (e == null) return false;
            if (e.parent != this) return false;
            if (Children.Remove(e))
            {
                e.Reorder -= E_Reorder;
                e.parent = null;
                ChildRemoved?.Invoke(e);
                return true;
            }
            return false;
        }

        protected virtual int Compare(UIObject a, UIObject b)
        {
            return b.zOrder - a.zOrder;
        }

        protected static void TryAndSendMessage(object c, string fn, params object[] args)
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
                for (int i = 0; i < componentList.Count; ++i)
                {
                    var c = componentList[i];
                    TryAndSendMessage(c, fn, args);
                }
            }
            catch { }

            if (args != null && args.Length > 0)
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

            TryAndSendMessage(this, fn, args);

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
                parent = parent.parent;
            }
        }

        public virtual void SendMessage(string fn, bool toChildren, params object[] args)
        {
            try
            {
                for (int i = 0; i < componentList.Count; ++i)
                {
                    var c = componentList[i];
                    TryAndSendMessage(c, fn, args);
                }
            }
            catch { }

            if (args != null && args.Length > 0)
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

            TryAndSendMessage(this, fn, args);

            if (!toChildren) return;

            //Stack<UIObject> stack = new Stack<UIObject>();

            UIObject previous = null;
            for (int i = 0; i < Children.Count; ++i)
            {
                var c = Children[i];
                if (c == null || !c.visible) continue;

                //special handling of draw
                //to help reset stencil level
                //if needed
                if (args != null && args.Length > 0)
                {
                    if (args[0] is DrawEvent)
                    {
                        var d = args[0] as DrawEvent;
                        var copy = d.Copy();
                        copy.previous = previous;
                        args[0] = copy;
                    }
                }

                c.SendMessage(fn, toChildren, args);

                previous = c;
            }
        }

        private void UpdateMatrix(bool updateChildren = true)
        {
            CalculateWorldScale();
            CalculateWorldSize();
            CalculateWorldPosition();

            Vector3 offset = new Vector3(origin * anchorSize);
            localMatrix = Matrix4.CreateTranslation(-offset) * Matrix4.CreateRotationZ(rotation * MathHelper.Deg2Rad) * Matrix4.CreateTranslation(offset);

            CalculateWorldMatrix();
            CalculateExtendedRect();

            anchoredRect = new Box2(anchorPosition, anchorSize.X, anchorSize.Y);
            rect = new Box2(worldPosition, worldSize.X, worldSize.Y);

            if (!updateChildren) return;

            Box2 b = rect;
            var children = Children;
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].UpdateMatrix();
                if (children[i].Visible)
                {
                    b.Encapsulate(children[i].rect);
                    b.Encapsulate(children[i].visibleRect);
                }
            }
            visibleRect = b;
        }

        private void CalculateExtendedRect()
        {
            Vector2 size = anchorSize + new Vector2(margin.Left + margin.Right, margin.Top + margin.Bottom);
            Vector2 offset = anchorPosition;

            if (parent == null)
            {
                offset -= new Vector2(margin.Left, margin.Top);
                extendedRect = new Box2(offset, anchorPosition + size);
                return;
            }

            Box2 pPadding = parent.padding;

            Vector2 bottomLeftOffset = new Vector2(margin.Left + pPadding.Left, -(margin.Bottom + pPadding.Bottom));
            Vector2 topLeftOffset = new Vector2(margin.Left + pPadding.Left, margin.Top + pPadding.Top);
            Vector2 bottomRightOffset = new Vector2(-(margin.Right + pPadding.Right), -(margin.Bottom + pPadding.Bottom));
            Vector2 topRightOffset = new Vector2(-(margin.Right + pPadding.Right), margin.Top + pPadding.Top);

            Vector2 leftOffset = new Vector2(margin.Left + pPadding.Left, margin.Top - pPadding.Top * 0.5f);
            Vector2 rightOffset = new Vector2(-(margin.Right + pPadding.Right), margin.Top - pPadding.Top * 0.5f);

            switch (relativeTo)
            {
                case Anchor.TopLeft:
                case Anchor.TopHorizFill:
                    offset -= topLeftOffset;
                    break;
                case Anchor.TopRight:
                    offset -= topRightOffset;
                    break;
                case Anchor.Top:
                    offset -= new Vector2(margin.Left - pPadding.Left * 0.5f, margin.Top + pPadding.Top);
                    break;
                case Anchor.BottomLeft:
                case Anchor.BottomHorizFill:
                    offset -= bottomLeftOffset;
                    break;
                case Anchor.BottomRight:
                    offset -= bottomRightOffset;
                    break;
                case Anchor.Bottom:
                    offset -= new Vector2(margin.Left - pPadding.Left * 0.5f, -(margin.Bottom + pPadding.Bottom));
                    break;
                case Anchor.Center:
                    offset -= new Vector2(margin.Left - pPadding.Left * 0.5f, margin.Top - pPadding.Top * 0.5f);
                    break;
                case Anchor.CenterHorizFill:
                    offset -= leftOffset;
                    break;
                case Anchor.Left:
                    offset -= leftOffset;
                    break;
                case Anchor.Right:
                    offset -= rightOffset;
                    break;
                case Anchor.LeftVerticalFill:
                    offset -= topLeftOffset;
                    break;
                case Anchor.RightVerticalFill:
                    offset -= topRightOffset;
                    break;
                default:
                    offset -= topLeftOffset;
                    break;
            }

            extendedRect = new Box2(offset, anchorPosition + size);
        }

        private void CalculateAnchorSize()
        {
            Vector2 offset = new Vector2(padding.Left + padding.Right, padding.Top + padding.Bottom)
                           - new Vector2(margin.Left + margin.Right, margin.Top + margin.Bottom);
            if (parent == null)
            {
                anchorSize = size + offset;
                return;
            }
            Vector2 pSize = parent.anchorSize;

            switch (relativeTo)
            {
                case Anchor.TopHorizFill:
                case Anchor.CenterHorizFill:
                case Anchor.BottomHorizFill:
                    anchorSize = new Vector2(pSize.X, size.Y) + offset;
                    break;
                case Anchor.Fill:
                    anchorSize = pSize + offset;
                    break;
                case Anchor.LeftVerticalFill:
                case Anchor.RightVerticalFill:
                    anchorSize = new Vector2(size.X, pSize.Y) + offset;
                    break;
                default:
                    anchorSize = size + offset;
                    break;
            }
        }

        private void CalculateWorldSize()
        {
            CalculateAnchorSize();
            worldSize = anchorSize * worldScale;
        }

        private void CalculateWorldMatrix()
        {
            if (parent == null)
            {
                worldMatrix = localMatrix;
                return;
            }
            worldMatrix = localMatrix * parent.worldMatrix;
        }

        private void CalculateAnchorPositionOffset()
        {
            if (parent == null)
            {
                anchorPosition += new Vector2(margin.Left, margin.Top);
                return;
            }

            Box2 pPadding = parent.padding;

            Vector2 bottomLeftOffset = new Vector2(margin.Left + pPadding.Left, -(margin.Bottom + pPadding.Bottom));
            Vector2 topLeftOffset = new Vector2(margin.Left + pPadding.Left, margin.Top + pPadding.Top);
            Vector2 bottomRightOffset = new Vector2(-(margin.Right + pPadding.Right), -(margin.Bottom + pPadding.Bottom));
            Vector2 topRightOffset = new Vector2(-(margin.Right + pPadding.Right), margin.Top + pPadding.Top);

            Vector2 leftOffset = new Vector2(margin.Left + pPadding.Left, margin.Top - pPadding.Top * 0.5f);
            Vector2 rightOffset = new Vector2(-(margin.Right + pPadding.Right), margin.Top - pPadding.Top * 0.5f);

            switch (relativeTo)
            {
                case Anchor.TopLeft:
                case Anchor.TopHorizFill:
                    anchorPosition += topLeftOffset;
                    break;
                case Anchor.TopRight:
                    anchorPosition += topRightOffset;
                    break;
                case Anchor.Top:
                    anchorPosition += new Vector2(margin.Left - pPadding.Left * 0.5f, margin.Top + pPadding.Top);
                    break;
                case Anchor.BottomLeft:
                case Anchor.BottomHorizFill:
                    anchorPosition += bottomLeftOffset;
                    break;
                case Anchor.BottomRight:
                    anchorPosition += bottomRightOffset;
                    break;
                case Anchor.Bottom:
                    anchorPosition += new Vector2(margin.Left - pPadding.Left * 0.5f, -(margin.Bottom + pPadding.Bottom));
                    break;
                case Anchor.Center:
                    anchorPosition += new Vector2(margin.Left - pPadding.Left * 0.5f, margin.Top - pPadding.Top * 0.5f);
                    break;
                case Anchor.CenterHorizFill:
                    anchorPosition += leftOffset;
                    break;
                case Anchor.Left:
                    anchorPosition += leftOffset;
                    break;
                case Anchor.Right:
                    anchorPosition += rightOffset;
                    break;
                case Anchor.LeftVerticalFill:
                    anchorPosition += topLeftOffset;
                    break;
                case Anchor.RightVerticalFill:
                    anchorPosition += topRightOffset;
                    break;
                default:
                    anchorPosition += topLeftOffset;
                    break;
            }
        }

        private void CalculateAnchorPosition()
        {
            Vector2 size = anchorSize;
            Vector2 pos = position;

            //calculate scaled position
            Vector2 offset = origin * size;
            pos -= offset;
            pos *= scale;
            pos += offset;

            if (parent == null)
            {
                anchorPosition = pos;
                return;
            }

            Vector2 pSize = parent.anchorSize;

            switch (relativeTo)
            {
                case Anchor.Bottom:
                    anchorPosition = new Vector2(pSize.X * 0.5f - size.X * 0.5f + pos.X, pSize.Y - size.Y - pos.Y);
                    break;
                case Anchor.Top:
                    anchorPosition = new Vector2(pSize.X * 0.5f - size.X * 0.5f + pos.X, pos.Y);
                    break;
                case Anchor.LeftVerticalFill:
                case Anchor.TopHorizFill:
                case Anchor.TopLeft:
                case Anchor.Fill:
                    anchorPosition = pos;
                    break;
                case Anchor.RightVerticalFill:
                case Anchor.TopRight:
                    anchorPosition = new Vector2(pSize.X - size.X - pos.X, pos.Y);
                    break;
                case Anchor.BottomHorizFill:
                case Anchor.BottomLeft:
                    anchorPosition = new Vector2(pos.X, pSize.Y - size.Y - pos.Y);
                    break;
                case Anchor.BottomRight:
                    anchorPosition = pSize - size - pos;
                    break;
                case Anchor.CenterHorizFill:
                case Anchor.Center:
                    anchorPosition = pSize * 0.5f - size * 0.5f + pos;
                    break;
                case Anchor.Right:
                    anchorPosition = new Vector2(pSize.X - size.X - pos.X, pSize.Y * 0.5f - size.Y * 0.5f + pos.Y);
                    break;
                case Anchor.Left:
                    anchorPosition = new Vector2(pos.X, pSize.Y * 0.5f - size.Y * 0.5f + pos.Y);
                    break;
                default:
                    anchorPosition = pos;
                    break;
            }
        }

        private void CalculateWorldPosition()
        {
            CalculateAnchorPosition();
            CalculateAnchorPositionOffset();
            if (parent == null)
            {
                worldPosition = anchorPosition;
                return;
            }
            worldPosition = anchorPosition + parent.worldPosition;
        }

        private void CalculateWorldScale()
        {
            if (parent == null)
            {
                worldScale = scale;
                return;
            }
            worldScale = scale * parent.worldScale;
        }
    }
}
