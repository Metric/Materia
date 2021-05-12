using Assimp.Configs;
using InfinityUI.Components;
using InfinityUI.Interfaces;
using Materia.Rendering;
using Materia.Rendering.Imaging;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace InfinityUI.Core
{
    public class UI
    {
        protected static Dictionary<string, GLTexture2D> ImageCache { get; set; } = new Dictionary<string, GLTexture2D>();
        public static GLTexture2D DefaultWhite { get; protected set; }

        public static bool Initied { get; protected set; }
        public static UIObject Selection { get; protected set; }

        public static Dictionary<string, UIObject> Elements { get; protected set; } = new Dictionary<string, UIObject>();

        private static IFocusable focus;
        public static IFocusable Focus
        {
            get => focus;
            set
            {
                if (focus != value)
                {
                    focus?.OnLostFocus(new FocusEvent());
                }
                focus = value;
            }
        }

        private static List<UIObject> canvases = new List<UIObject>();

        private static UIObject currentSelection = null, activeSelection = null;
        private static Vector2 mousePosition, prevMousePosition;
        private static MouseButton mouseButton = MouseButton.None;

        // helpers to access last known keyboard state for modifier keys
        public static bool IsCtrlPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsCtrl : false;
        }
        public static bool IsAltPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsAlt : false;
        }
        public static bool IsShiftPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsShift : false;
        }
        public static bool IsLeftCtrlPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsLeftCtrl : false;
        }
        public static bool IsRightCtrlPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsRightCtrl : false;
        }
        public static bool IsLeftAltPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsLeftAlt : false;
        }
        public static bool IsRightAltPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsRightAlt : false;
        }
        public static bool IsLeftShiftPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsLeftShift : false;
        }
        public static bool IsRightShiftPressed
        {
            get => LastKeyboardEvent != null ? LastKeyboardEvent.IsRightShift : false;
        }
        public static KeyboardEventArgs LastKeyboardEvent { get; private set; }

        public static event Action<KeyboardEventArgs> KeyDown;
        public static event Action<KeyboardEventArgs> KeyUp;
        public static event Action<KeyboardEventArgs> TextInput;

        public static event Action<MouseEventArgs> MouseMove;
        public static event Action<MouseEventArgs> MouseUp;
        public static event Action<MouseEventArgs> MouseDown;
        public static event Action<MouseEventArgs> MouseClick;
        public static event Action<MouseWheelArgs> MouseWheel;

        /// <summary>
        /// Gets or sets the mouse position.
        /// This should be an unscaled mouse position
        /// </summary>
        /// <value>
        /// The mouse position.
        /// </value>
        public static Vector2 MousePosition
        {
            get
            {
                return mousePosition;
            }
            set
            {
                prevMousePosition = mousePosition;
                mousePosition = value;
            }
        }

        /// <summary>
        /// Gets the mouse delta.
        /// This should be an unscaled mouse delta
        /// </summary>
        /// <value>
        /// The mouse delta.
        /// </value>
        public static Vector2 MouseDelta
        {
            get
            {
                return mousePosition - prevMousePosition;
            }
        }

        /// <summary>
        /// Gets the previous mouse position.
        /// This is unscaled
        /// </summary>
        /// <value>
        /// The previous mouse position.
        /// </value>
        public static Vector2 PrevMousePosition
        {
            get
            {
                return prevMousePosition;
            }
        }

        public static void OnMouseWheel(Vector2 delta)
        {
            delta.Y *= -1;

            if(currentSelection != null)
            {
                MouseWheelArgs e = new MouseWheelArgs()
                {
                    Delta = delta
                };
                currentSelection.SendMessageUpwards("OnMouseWheel", e);
            }

            {
                MouseWheelArgs e = new MouseWheelArgs()
                {
                    Delta = delta
                };
                MouseWheel?.Invoke(e);
            }
        }

        public static void OnMouseMove(float x, float y)
        {
            UIObject last = currentSelection;
            Vector2 p = MousePosition = new Vector2(x, y);

            if (currentSelection != null)
            {
                MouseEventArgs e = new MouseEventArgs()
                {
                    Delta = MouseDelta,
                    Button = mouseButton,
                    Position = MousePosition
                };

                currentSelection.SendMessage("OnMouseMove", false, e);
            }

            {
                MouseEventArgs e = new MouseEventArgs()
                {
                    Delta = MouseDelta,
                    Button = mouseButton,
                    Position = MousePosition
                };
                MouseMove?.Invoke(e);
            }

            if (Pick(ref p))
            {
                currentSelection = Selection;
            }
            else
            {
                currentSelection = null;
            }

            if (currentSelection != last)
            {
                if (last != null)
                {
                    MouseEventArgs e = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = mouseButton,
                        Position = MousePosition
                    };

                    last.SendMessage("OnMouseLeave", false, e);
                }

                if (currentSelection != null)
                {
                    MouseEventArgs e = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = mouseButton,
                        Position = MousePosition
                    };

                    currentSelection.SendMessage("OnMouseEnter", false, e);
                }
            }
        }

        public static void OnMouseClick(MouseButton btn)
        {
            if (btn.HasFlag(MouseButton.Down))
            {
                if (currentSelection != null)
                {
                    FocusEvent fev = new FocusEvent();
                    currentSelection.SendMessageUpwards("OnFocus", fev);

                    MouseEventArgs e = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = btn,
                        Position = MousePosition
                    };

                    currentSelection.SendMessageUpwards("OnMouseDown", e);
                    activeSelection = currentSelection;
                }

                {
                    MouseEventArgs e = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = btn,
                        Position = MousePosition
                    };
                    MouseDown?.Invoke(e);
                }
            }
            else if(btn.HasFlag(MouseButton.Up))
            {
                if (activeSelection != null && activeSelection != currentSelection)
                { 
                    MouseEventArgs e = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = btn,
                        Position = MousePosition
                    };

                    activeSelection.SendMessageUpwards("OnMouseUp", e);
                    activeSelection = currentSelection;
                }
                else if (currentSelection != null)
                {
                    MouseEventArgs ex = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = btn,
                        Position = MousePosition
                    };

                    currentSelection.SendMessageUpwards("OnMouseClick", ex);
  
                    MouseEventArgs e = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = btn,
                        Position = MousePosition
                    };

                    currentSelection.SendMessageUpwards("OnMouseUp", e);
                }

                {
                    MouseEventArgs e = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = btn,
                        Position = MousePosition
                    };
                    MouseClick?.Invoke(e);
                }

                {
                    MouseEventArgs e = new MouseEventArgs()
                    {
                        Delta = MouseDelta,
                        Button = btn,
                        Position = MousePosition
                    };
                    MouseUp?.Invoke(e);
                }
            }

            mouseButton = btn;
        }

        public static void OnKeyDown(KeyboardEventArgs e)
        {
            LastKeyboardEvent = e;
            if (Focus != null && Focus is IKeyboardInput)
            {
                ((IKeyboardInput)Focus).OnKeyDown(e);
            }

            //reset handled
            e.IsHandled = false;
            KeyDown?.Invoke(e);
        }

        public static void OnTextInput(KeyboardEventArgs e)
        {
            LastKeyboardEvent = e;
            if (Focus != null && Focus is IKeyboardInput)
            {
                ((IKeyboardInput)Focus).OnTextInput(e);
            }

            //reset handled
            e.IsHandled = false;
            TextInput?.Invoke(e);
        }
        public static void OnKeyUp(KeyboardEventArgs e)
        {
            LastKeyboardEvent = e;
            if (Focus != null && Focus is IKeyboardInput)
            {
                ((IKeyboardInput)Focus).OnKeyUp(e);
            }

            //reset handled
            e.IsHandled = false;
            KeyUp?.Invoke(e);
        }

        public static void Init()
        {
            if (Initied) return;

            Initied = true;

            UIRenderer.Init();

            if (DefaultWhite == null)
            {
                RawBitmap bmp = new RawBitmap(16, 16);
                GLPixel px = GLPixel.FromRGBA(1.0f, 1.0f, 1.0f, 1.0f);
                bmp.Clear(px);
                DefaultWhite = new GLTexture2D(PixelInternalFormat.Rgba8);
                DefaultWhite.Bind();
                DefaultWhite.SetData(bmp.Image, PixelFormat.Bgra, 16, 16, 0);
                DefaultWhite.Linear();
                DefaultWhite.Repeat();
                DefaultWhite.GenerateMipMaps();
                GLTexture2D.Unbind();
            }
        }

        public static bool PointInPolygon(List<Vector2> pts, ref Vector2 p)
        {
            if (pts.Count <= 2)
            {
                return false;
            }

            float y = p.Y;
            float x = p.X;

            int i = 0, j = pts.Count - 1;
            bool odd = false;

            for (i = 0; i < pts.Count; ++i)
            {
                Vector2 p2 = pts[i];
                Vector2 p3 = pts[j];

                if (((p2.Y < y && p3.Y >= y) || (p3.Y < y && p2.Y >= y))
                    && (p2.X <= x || p3.X <= x))
                {
                    odd ^= (p2.X + (y - p2.Y) / (p3.Y - p2.Y) * (p3.X - p2.X) < x);
                }
                j = i;
            }

            return odd;
        }

        public static bool IsPointIn(ref Vector2 p, UIObject e)
        {
            Vector2 pos = e.AnchoredPosition;
            Vector2 size = e.AnchoredSize;

            Vector2 topLeft = pos;
            Vector2 topRight = pos + new Vector2(size.X, 0);
            Vector2 bottomLeft = pos + new Vector2(0, size.Y);
            Vector2 bottomRight = pos + size;

            topLeft = ToWorld(ref topLeft, e);
            topRight = ToWorld(ref topRight, e);
            bottomLeft = ToWorld(ref bottomLeft, e);
            bottomRight = ToWorld(ref bottomRight, e);

            List<Vector2> pts = new List<Vector2>
            {
                bottomLeft,
                topLeft,
                topRight,
                bottomRight
            };

            return PointInPolygon(pts, ref p);
        }

        public static Vector2 ToWorld(ref Vector2 p, UIObject e)
        {
            Vector4 rot = new Vector4(p.X, p.Y, 0, 1) * e.LocalMatrix; //todo: test with e.ModelMatrix instead
            return rot.Xy;
        }

        public static void Add(UIObject e)
        {
            if (e == null) return;
            Elements.Add(e.ID, e);
        }

        public static void Remove(UIObject e)
        {
            if (e == null) return;
            Elements.Remove(e.ID);
        }

        public static void RegisterCanvas(UICanvas canvas)
        {
            if (canvas == null || canvas.Parent == null) return;
            canvases.Add(canvas.Parent);
        }

        public static void UnregisterCanvas(UICanvas canvas)
        {
            if (canvas == null || canvas.Parent == null) return;
            canvases.Remove(canvas.Parent);
        }

        public static UIObject Get(string id)
        {
            UIObject ele = null;
            Elements.TryGetValue(id, out ele);
            return ele;
        }

        public static void Draw()
        {
            IGL.Primary.Disable((int)EnableCap.CullFace);
            IGL.Primary.Enable((int)EnableCap.StencilTest);
            IGL.Primary.Clear((int)ClearBufferMask.StencilBufferBit);
            IGL.Primary.StencilMask(0xFF);

            UIRenderer.Bind();

            for (int i = 0; i < canvases.Count; ++i)
            {
                if (!canvases[i].Visible) continue;
                var cv = canvases[i]?.GetComponent<UICanvas>();
                cv?.Render();
            }

            UIRenderer.Unbind();
            IGL.Primary.Disable((int)EnableCap.StencilTest);
        }

        public static bool Pick(ref Vector2 p)
        {
            Selection = null;
            for (int i = canvases.Count - 1; i >= 0; --i)
            {
                UIObject obj = canvases[i];
                UICanvas canvas = obj.GetComponent<UICanvas>();
                Vector2 wp = canvas.ToCanvasSpace(p);
                Selection = obj.Pick(ref wp);
                if (Selection != null)
                {
                    break;
                }
            }

            return Selection != null;
        }

        public static void Dispose()
        {
            UIRenderer.Dispose();

            DefaultWhite?.Dispose();
            DefaultWhite = null;

            var images = ImageCache.Values.ToList();
            for (int i = 0; i < images.Count; ++i)
            {
                images[i]?.Dispose();
            }

            var elements = Elements.Values.ToList();
            for (int i = 0; i < elements.Count; ++i)
            {
                elements[i]?.Dispose();
            }

            ImageCache.Clear();
            Elements.Clear();
        }

        public static GLTexture2D GetEmbeddedImage(string path, Type t)
        {
            GLTexture2D img = null;
            if (ImageCache.TryGetValue(path, out img))
            {
                return img;
            }

            try
            {
                EmbeddedFileProvider provider = new EmbeddedFileProvider(t.Assembly);
                using (Bitmap rbmp = (Bitmap)Bitmap.FromStream(provider.GetFileInfo(path).CreateReadStream()))
                {
                    RawBitmap bmp = RawBitmap.FromBitmap(rbmp);
                    img = new GLTexture2D(PixelInternalFormat.Rgba8);
                    img.Bind();
                    img.SetData(bmp.Image, PixelFormat.Bgra, bmp.Width, bmp.Height);
                    img.Linear();
                    img.Repeat();
                    img.GenerateMipMaps();
                    GLTexture2D.Unbind();

                    ImageCache[path] = img;
                }

                return img;
            }
            catch
            {
                Debug.WriteLine("Failed to get embedded image for: " + path);
                return null;
            }
        }

        public static GLTexture2D GetImage(string path)
        {
            GLTexture2D img = null;
            if (ImageCache.TryGetValue(path, out img))
            {
                return img;
            }

            try
            {
                using (Bitmap rbmp = (Bitmap)Bitmap.FromFile(path))
                {
                    RawBitmap bmp = RawBitmap.FromBitmap(rbmp);
                    img = new GLTexture2D(PixelInternalFormat.Rgba8);
                    img.Bind();
                    img.SetData(bmp.Image, PixelFormat.Bgra, bmp.Width, bmp.Height);
                    img.Linear();
                    img.Repeat();
                    img.GenerateMipMaps();
                    GLTexture2D.Unbind();

                    ImageCache[path] = img;
                }

                return img;
            }
            catch
            {
                Debug.WriteLine("Failed to get image for: " + path);
                return null;
            }
        }

        public static void SnapToGrid(UIObject ele, int gridSize)
        {
            ele.Position = new Vector2(
                            MathF.Round(ele.Position.X / gridSize), 
                            MathF.Round(ele.Position.Y / gridSize)
                           );

            ele.Position *= gridSize;
        }

        public static void SnapToElement(UIObject a, UIObject b, 
            float tolerance = 4, float xSign = 1, float ySign = 1)
        {
            if (xSign > 0)
            {
                if (MathF.Abs(a.Rect.Left - b.Rect.Right) <= tolerance)
                {
                    a.Position = new Vector2(b.Position.X + b.Size.X, a.Position.Y);
                }
                else if (MathF.Abs(a.Rect.Right - b.Rect.Left) <= tolerance)
                {
                    a.Position = new Vector2(b.Position.X - a.Size.X, a.Position.Y);
                }
            }
            else
            {
                if (MathF.Abs(a.Rect.Left - b.Rect.Right) <= tolerance)
                {
                    a.Position = new Vector2(b.Position.X, a.Position.Y);
                }
                else if (MathF.Abs(a.Rect.Right - b.Rect.Left) <= tolerance)
                {
                    a.Position = new Vector2(b.Position.X + b.Size.X + a.Size.X, a.Position.Y);
                }
            }

            if (ySign > 0)
            {
                if (MathF.Abs(a.Rect.Top - b.Rect.Bottom) <= tolerance)
                {
                    a.Position = new Vector2(a.Position.X, b.Position.Y + b.Size.Y);
                }
                else if (MathF.Abs(a.Rect.Bottom - b.Rect.Top) <= tolerance)
                {
                    a.Position = new Vector2(a.Position.X, b.Position.Y - a.Size.Y);
                }
            }
            else
            {
                if (MathF.Abs(a.Rect.Top - b.Rect.Bottom) <= tolerance)
                {
                    a.Position = new Vector2(a.Position.X, b.Position.Y);
                }
                else if (MathF.Abs(a.Rect.Bottom - b.Rect.Top) <= tolerance)
                {
                    a.Position = new Vector2(a.Position.X, b.Position.Y + b.Size.Y + a.Size.Y);
                }
            }
        }
    }
}
