using InfinityUI.Components;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace InfinityUI.Controls
{
    public class TextRange
    {
        public int start;
        public int end;

        public int Length
        {
            get
            {
                return Math.Abs(end - start);
            }
        }

        public TextRange(int s, int e)
        {
            start = s;
            end = e;
        }

        public void Clamp(int len)
        {
            start = Math.Max(0, Math.Min(start, len));
            end = Math.Max(0, Math.Min(end, len));

            if (start > end)
            {
                int t = end;
                end = start;
                start = t;
            }
        }
    }

    public class TextInput : UIObject, ILayout
    {
        public event Action<TextInput> LostFocus;
        public event Action<TextInput> OnSubmit;
        public event Action<TextInput> OnClear;

        public bool NeedsUpdate { get; set; }

        TextRange selectedText;
        public TextRange SelectText
        {
            get
            {
                return selectedText;
            }
            set
            {
                selectedText = value;
                NeedsUpdate = true;
            }
        }

        public Regex AllowedFormat { get; set; }
        public UIText View { get; protected set; }
        public UIText PlaceholderView { get; protected set; }
        public string Text
        {
            get
            {
                if (View == null || string.IsNullOrEmpty(View.Text)) return "";
                return View.Text;
            }
            set
            {
                if (View == null) return;
                View.Text = value;
                carretPosition = value.Length;
                NeedsUpdate = true;
            }
        }

        public string Placeholder
        {
            get
            {
                if (PlaceholderView == null || string.IsNullOrEmpty(PlaceholderView.Text)) return "";
                return PlaceholderView.Text;
            }
            set
            {
                if (PlaceholderView == null) return;
                PlaceholderView.Text = value;
            }
        }

        public int TextLength
        {
            get
            {
                if (View == null || string.IsNullOrEmpty(View.Text))
                {
                    return 0;
                }

                return View.Text.Length;
            }
        }

        public UIImage Background { get => background; }

        public UIObject Carret { get; set; }
        public UIObject CarretSelection { get; set; }

        protected UIImage background;
        protected UISelectable selectable;
        protected UIObject textContainer;
        protected UIObject placeholderContainer;
        protected Button clearButton;

        protected bool isFocused = false;
        protected float horizontalOffset = 0;
        protected float maxOverflow = 0;
        protected bool mouseDown = false;

        protected int clickCount = 0;
        protected long clickTick = 0;
        protected int carretPosition;

        protected Stack<string> redos = new Stack<string>();
        protected Stack<string> undos = new Stack<string>();

        public int CarretPosition
        {
            get
            {
                return carretPosition;
            }
            set
            {
                carretPosition = Math.Min(Text.Length, Math.Max(0, value));
                NeedsUpdate = true;
            }
        }
        public int MaxLength { get; set; }

        public TextInput(float fontSize, Vector2 size, int maxLength = 0, GLTexture2D clearButtonIcon = null) : base()
        {
            RaycastTarget = true;

            Size = size;

            background = AddComponent<UIImage>();
            background.Clip = true;

            selectable = AddComponent<UISelectable>();

            textContainer = new UIObject
            {
                RelativeTo = Anchor.Left,
                Margin = new Box2(4,0,0,0)
            };

            View = textContainer.AddComponent<UIText>();
            View.Text = "";
            View.FontSize = fontSize;

            placeholderContainer = new UIObject
            {
                RelativeTo = Anchor.Left,
                Margin = new Box2(4,0,0,0)
            };

            PlaceholderView = placeholderContainer.AddComponent<UIText>();
            PlaceholderView.Text = "";
            PlaceholderView.FontSize = fontSize;
            PlaceholderView.Color = new Vector4(View.Color.Xyz, 0.75f);

            MaxLength = maxLength;

            Carret = new UIObject();
            Carret.Size = new Vector2(4, Size.Y);
            var carretImage = Carret.AddComponent<UIImage>();
            Carret.Visible = false;

            CarretSelection = new UIObject();
            CarretSelection.Size = new Vector2(0, Size.Y);
            var selectionIamge = CarretSelection.AddComponent<UIImage>();
            selectionIamge.Color = new Vector4(0, 1, 1, 0.5f);

            clearButton = new Button("", new Vector2(32, 32))
            {
                RelativeTo = Anchor.TopRight,
                Margin = new Box2(2,2,2,2),
                Visible = false,
            };
            clearButton.Submit += ClearButton_Submit;
            clearButton.Background.Texture = clearButtonIcon ?? UI.DefaultWhite;

            AddChild(textContainer);
            AddChild(Carret);
            AddChild(CarretSelection);
            AddChild(placeholderContainer);
            AddChild(clearButton);

            InitEvents();

            NeedsUpdate = true;
        }

        private void ClearButton_Submit(Button obj)
        {
            Text = "";
            OnClear?.Invoke(this);
        }

        protected virtual void InitEvents()
        {
            if (selectable == null) return;
            selectable.KeyDown += OnKeyDown;
            selectable.KeyUp += OnKeyUp;
            selectable.TextInput += OnTextInput;
            selectable.PointerDown += OnMouseDown;
            selectable.PointerUp += OnMouseUp;
            selectable.PointerMove += OnMouseMove;
            selectable.FocusChanged += OnFocusChanged;
        }

        public virtual void Invalidate()
        {
            if (View == null || Carret == null || !NeedsUpdate) return;

            var carretImage = Carret.GetComponent<UIImage>();
            if (carretImage != null && background != null)
            {
                carretImage.Color = new Vector4(1.0f - background.Color.X, 1.0f - background.Color.Y, 1.0f - background.Color.Z, carretImage.Color.W);
            }

            float textSize = GetTextSizeX();
            float carretX = GetCarretX() - Carret.Size.X * 0.5f;

            float carretSelectX = GetSelectionStartX();
            float carretSelectEnd = GetSelectionEndX();

            float carretSelectWidth = MathF.Abs(carretSelectEnd - carretSelectX);

            Vector2 wSize = AnchorSize;
            if (textSize > wSize.X)
            {
                maxOverflow = textSize - wSize.X;

                if (carretX >= wSize.X)
                {
                    horizontalOffset = (wSize.X - carretX - Carret.Size.X) - (wSize.X - (carretX % wSize.X));
                }
                else
                {
                    horizontalOffset = 0;
                }
            }
            else
            {
                maxOverflow = 0;
                horizontalOffset = 0;
            }

            textContainer.Position = new Vector2(horizontalOffset, 0);
            Carret.Position = new Vector2(carretX + horizontalOffset, 0);

            if (selectedText != null && selectedText.Length > 0)
            {
                CarretSelection.Visible = true;
            }
            else
            {
                CarretSelection.Visible = false;
            }

            //update carret selection area
            CarretSelection.Size = new Vector2(carretSelectWidth, CarretSelection.Size.Y);
            CarretSelection.Position = new Vector2(carretSelectX + horizontalOffset, 0);

            placeholderContainer.Visible = !isFocused && string.IsNullOrEmpty(View.Text);

            clearButton.Visible = !string.IsNullOrEmpty(Text) && clearButton.Background.Texture != UI.DefaultWhite;

            NeedsUpdate = false;
        }

        protected float GetTextSizeX()
        {
            if (View == null || string.IsNullOrEmpty(Text)) return 0;
            Vector2 measure = View.Measure(Text);
            return measure.X;
        }

        protected float GetSelectionStartX()
        {
            if (View == null || string.IsNullOrEmpty(Text) || selectedText == null || selectedText.Length == 0) return 0;
            Vector2 measure = View.Measure(Text.Substring(0, selectedText.start));
            return measure.X;
        }

        protected float GetSelectionEndX()
        {
            if (View == null || string.IsNullOrEmpty(Text) || selectedText == null || selectedText.Length == 0) return 0;
            Vector2 measure = View.Measure(Text.Substring(0, selectedText.end));
            return measure.X;
        }

        protected float GetCarretX()
        {
            if (View == null || string.IsNullOrEmpty(Text)) return 0;
            Vector2 measure = View.Measure(Text.Substring(0, carretPosition));
            return measure.X;
        }

        protected virtual void OnKeyDown(UISelectable selectable, KeyboardEventArgs e)
        {
            if (!isFocused) return;
            if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter 
                || e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEnter)
            {
                UI.Focus = null;
                OnSubmit?.Invoke(this);
                return;
            }

            string current = Text;

            switch(e.Key)
            {
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace:
                    if (carretPosition > 0)
                    {
                        RegisterUndo();

                        redos.Clear();

                        if (selectedText != null && selectedText.Length > 0)
                        {
                            View.Text = current.Substring(0, selectedText.start) + current.Substring(selectedText.end);
                            int start = selectedText.start;
                            selectedText = null;
                            CarretPosition = start;
                        }
                        else
                        {
                            View.Text = current.Substring(0, carretPosition - 1) + current.Substring(carretPosition);
                            CarretPosition--;
                        }
                    }
                    return;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete:
                    if (carretPosition < current.Length)
                    {
                        RegisterUndo();

                        redos.Clear();

                        if (selectedText != null && selectedText.Length > 0)
                        {
                            View.Text = current.Substring(0, selectedText.start) + current.Substring(selectedText.end);
                            int start = selectedText.start;
                            selectedText = null;
                            CarretPosition = start;
                        }
                        else
                        {
                            View.Text = current.Substring(0, carretPosition) + current.Substring(carretPosition + 1);
                            //just update
                            CarretPosition = carretPosition;
                        }
                    }
                    return;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape:
                    UI.Focus = null;
                    return;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.End:
                    CarretPosition = View.Text.Length;
                    return;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left:
                    if (e.IsShift)
                    {
                        if (selectedText == null)
                        {
                            selectedText = new TextRange(CarretPosition - 1, CarretPosition);
                            selectedText.Clamp(TextLength);
                        }
                        else
                        {
                            if (selectedText.start < CarretPosition)
                            {
                                selectedText.end = CarretPosition - 1;
                            }
                            else
                            {
                                selectedText.start = CarretPosition - 1;
                            }
                            selectedText.Clamp(TextLength);
                        }
                    }
                    else
                    {
                        selectedText = null;
                    }
                    CarretPosition--;
                    return;
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right:
                    if (e.IsShift)
                    {
                        if (selectedText == null)
                        {
                            selectedText = new TextRange(CarretPosition, CarretPosition + 1);
                            selectedText.Clamp(TextLength);
                        }
                        else
                        {
                            if (selectedText.start < CarretPosition)
                            {
                                selectedText.end = CarretPosition + 1;
                            }
                            else
                            {
                                selectedText.start = CarretPosition + 1;
                            }
                            selectedText.Clamp(TextLength);
                        }
                    }
                    else
                    {
                        selectedText = null;
                    }
                    CarretPosition++;
                    return;
            }

            if (e.IsCtrl && e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.V)
            {
                object o = e.GetClipboardContent?.Invoke();

                if (o == null)
                {
                    return;
                }

                string s = o.ToString();

                if (AllowedFormat != null)
                {
                    if (!AllowedFormat.IsMatch(s))
                    {
                        return;
                    }
                }

                if (selectedText != null && selectedText.Length > 0)
                {
                    current = current.Substring(0, selectedText.start) + current.Substring(selectedText.end);
                    carretPosition = selectedText.start;
                    selectedText = null;
                }

                RegisterUndo();

                redos.Clear();

                View.Text = current.Insert(carretPosition, s);
                CarretPosition += s.Length;
            }
            else if (e.IsCtrl && e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.X && selectedText != null)
            {
                if (selectedText.Length > Text.Length || selectedText.Length == 0)
                {
                    return;
                }
             
                //copy to clipboard
                e.SetClipboardContent?.Invoke(Text.Substring(selectedText.start, selectedText.Length));

                //then cut
                current = current.Substring(0, selectedText.start) + current.Substring(selectedText.end);
                carretPosition = selectedText.start;
                selectedText = null;

                RegisterUndo();

                redos.Clear();

                View.Text = current;
                CarretPosition = carretPosition;
            }
            else if (e.IsCtrl && e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.C && selectedText != null)
            {
                if (selectedText.Length > Text.Length || selectedText.Length == 0)
                {
                    return;
                }

                e.SetClipboardContent?.Invoke(Text.Substring(selectedText.start, selectedText.Length));
            }
            else if (e.IsCtrl && e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Z)
            {
                if (undos.Count > 0)
                {
                    RegisterRedo();

                    View.Text = undos.Pop();
                }
            }
            else if(e.IsCtrl && e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Y)
            {
                if (redos.Count > 0)
                {
                    RegisterUndo();

                    View.Text = redos.Pop();
                }
            }
        }

        protected void RegisterUndo()
        {
            if (undos.Count + 1 >= 100)
            {
                undos = new Stack<string>(undos.ToList().Take(75));
            }

            if (TextLength > 0)
            {
                undos.Push(Text);
            }
        }

        protected void RegisterRedo()
        {
            if (redos.Count + 1 >= 100)
            {
                redos = new Stack<string>(redos.ToList().Take(75));
            }

            if (TextLength > 0)
            {
                redos.Push(Text);
            }
        }

        protected virtual void OnTextInput(UISelectable selectable, KeyboardEventArgs e)
        {
            if (!isFocused) return;

            string current = Text;

            if (current.Length + e.Char.Length >= MaxLength && MaxLength > 0)
            {
                return;
            }

            string s = e.Char;

            if (e.IsShift || e.IsCapsLock)
            {
                s = s.ToUpper();
            }

            if (AllowedFormat != null)
            {
                if (!AllowedFormat.IsMatch(s))
                {
                    return;
                }
            }

            RegisterUndo();

            redos.Clear();

            View.Text = current.Insert(carretPosition, s);
            CarretPosition += s.Length;
        }

        protected int ToCarretPosition(float x)
        {
            if (View == null) return 0;

            //average font size width
            x -= horizontalOffset;
            Vector2 pixelSize = View.Measure(Text);
            float t = x / pixelSize.X;
            return (int)(TextLength * t);
        }

        protected virtual void OnKeyUp(UISelectable selectable, KeyboardEventArgs e)
        {

        }

        protected virtual void OnMouseDown(UISelectable selectable, MouseEventArgs e)
        {
            if (View == null || Carret == null || !e.Button.HasFlag(MouseButton.Left)) return;

            if (new TimeSpan(DateTime.Now.Ticks - clickTick).TotalMilliseconds > 250)
            {
                clickCount = 0;
            }

            ++clickCount;

            if (clickCount >= 2)
            {
                selectedText = new TextRange(0, TextLength);
                clickCount = 0;
            }
            else
            {
                selectedText = null;
            }

            clickTick = DateTime.Now.Ticks;

            mouseDown = true;
            CarretPosition = ToCarretPosition(e.Position.X - Rect.Left);
        }
        protected virtual void OnMouseMove(UISelectable selectable, MouseEventArgs e)
        {
            if (!isFocused || !mouseDown) return;
            if (View == null || Carret == null) return;

            int prev = CarretPosition;
            CarretPosition = ToCarretPosition(e.Position.X - Rect.Left);
            int dir = CarretPosition - prev;
            if (selectedText == null)
            {
                if (dir < 0)
                {
                    selectedText = new TextRange(CarretPosition, prev);
                    selectedText.Clamp(TextLength);
                }
                else if(dir > 0)
                {
                    selectedText = new TextRange(CarretPosition, prev);
                    selectedText.Clamp(TextLength);
                }
            }
            else
            {
                if (dir < 0)
                {
                    selectedText.start = CarretPosition;
                }
                else if(dir > 0)
                {
                    selectedText.end = CarretPosition;
                }

                selectedText.Clamp(TextLength);
            }
        }

        protected virtual void OnMouseUp(UISelectable selectable, MouseEventArgs e)
        {
            mouseDown = false;
        }

        protected virtual void OnFocusChanged(UISelectable selectable, FocusEvent fv, bool focused)
        {
            isFocused = focused;
            Carret.Visible = focused;
            if (!focused)
            {
                LostFocus?.Invoke(this);
            }
            placeholderContainer.Visible = !isFocused && string.IsNullOrEmpty(View.Text);
        }

        public void Focus()
        {
            if (selectable == null) return;
            selectable.OnFocus(new FocusEvent());
        }

        public override void Dispose(bool disposing = true)
        {
            base.Dispose(disposing);
            if (selectable == null) return;
            selectable.KeyDown -= OnKeyDown;
            selectable.KeyUp -= OnKeyUp;
            selectable.TextInput -= OnTextInput;
            selectable.PointerDown -= OnMouseDown;
            selectable.PointerUp -= OnMouseUp;
            selectable.PointerMove -= OnMouseMove;
            selectable.FocusChanged -= OnFocusChanged;
        }
    }
}
