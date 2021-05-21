using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using InfinityUI.Components;
using InfinityUI.Core;
using Materia.Graph;
using Materia.Rendering.Fonts;
using Materia.Rendering.Geometry;
using Materia.Rendering.Hdr;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Material;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;
using Materia.Rendering.Textures;
using MateriaCore.Components.GL;
using MateriaCore.Components.Panes;
using MateriaCore.Utils;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MateriaCore
{
    public class MainGLWindow : NativeWindow
    {
        public event Action Restored;

        #region DeltaTime
        public static int TargetFPS { get; set; } = 60;
        protected float deltaTime = 0;
        protected long lastTick = 0;
        protected float fpsTick = 0;
        #endregion

        protected UIObject graphArea;
        protected UIObject rootArea;

        protected UICanvas rootCanvas;
        protected UICanvas graphCanvas;

        protected UIGraph activeDocument;
        protected Graph activeGraph;

        protected UI2DPreview preview2D;
        protected UI3DPreview preview3D;
        protected UIShelf shelf;

        protected IHdrFile hdrToLoad;
        protected HdrMap hdrMap;

        private Keys currentKey;
        private string currentTextInput;

        private bool pbrInitialized = false;

        private Parameters parameterPane;

        private bool isMinimized = false;

        public MainGLWindow(NativeWindowSettings settings) : base(settings)
        {
            InitializeEvents();
            parameterPane = new Parameters(this);
            parameterPane.Show();
        }

        #region Window Events
        private void MainGLWindow_Closing(System.ComponentModel.CancelEventArgs obj)
        {
            IsVisible = false;
            InternalDispose();
            (Application.Current.ApplicationLifetime as IControlledApplicationLifetime)?.Shutdown();
        }

        private void MainGLWindow_KeyUp(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            currentKey = obj.Key;

            InfinityUI.Interfaces.KeyboardEventArgs kevent = new InfinityUI.Interfaces.KeyboardEventArgs()
            {
                Key = currentKey,
                Char = currentTextInput,
                IsAlt = IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt),
                IsLeftAlt = IsKeyDown(Keys.LeftAlt),
                IsRightAlt = IsKeyDown(Keys.RightAlt),
                IsCtrl = IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl),
                IsLeftCtrl = IsKeyDown(Keys.LeftControl),
                IsRightCtrl = IsKeyDown(Keys.RightControl),
                IsShift = IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift),
                IsLeftShift = IsKeyDown(Keys.LeftShift),
                IsRightShift = IsKeyDown(Keys.RightShift),
                IsCapsLock = IsKeyDown(Keys.CapsLock),
                GetClipboardContent = () =>
                {
                    return ClipboardString;
                },
                SetClipboardContent = (o) => {
                    if (o == null)
                    {
                        ClipboardString = "";
                    }
                    else
                    {
                        ClipboardString = o.ToString();
                    }
                }
            };

            UI.OnKeyUp(kevent);
        }

        private void MainGLWindow_KeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            currentKey = obj.Key;

            InfinityUI.Interfaces.KeyboardEventArgs kevent = new InfinityUI.Interfaces.KeyboardEventArgs()
            {
                Key = currentKey,
                Char = currentTextInput,
                IsAlt = IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt),
                IsLeftAlt = IsKeyDown(Keys.LeftAlt),
                IsRightAlt = IsKeyDown(Keys.RightAlt),
                IsCtrl = IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl),
                IsLeftCtrl = IsKeyDown(Keys.LeftControl),
                IsRightCtrl = IsKeyDown(Keys.RightControl),
                IsShift = IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift),
                IsLeftShift = IsKeyDown(Keys.LeftShift),
                IsRightShift = IsKeyDown(Keys.RightShift),
                IsCapsLock = IsKeyDown(Keys.CapsLock),
                GetClipboardContent = () =>
                {
                    return ClipboardString;
                },
                SetClipboardContent = (o) => {
                    if (o == null)
                    {
                        ClipboardString = "";
                    }
                    else
                    {
                        ClipboardString = o.ToString();
                    }
                }
            };

            UI.OnKeyDown(kevent);
        }

        private void MainGLWindow_MouseUp(OpenTK.Windowing.Common.MouseButtonEventArgs obj)
        {
            InfinityUI.Interfaces.MouseButton btn = InfinityUI.Interfaces.MouseButton.None;

            switch (obj.Button)
            {
                case MouseButton.Left:
                    btn = InfinityUI.Interfaces.MouseButton.Left | InfinityUI.Interfaces.MouseButton.Up;
                    break;
                case MouseButton.Right:
                    btn = InfinityUI.Interfaces.MouseButton.Right | InfinityUI.Interfaces.MouseButton.Up;
                    break;
                case MouseButton.Middle:
                    btn = InfinityUI.Interfaces.MouseButton.Middle | InfinityUI.Interfaces.MouseButton.Up;
                    break;
            }

            if (btn != InfinityUI.Interfaces.MouseButton.None)
            {
                UI.OnMouseClick(btn);
            }
        }

        private void MainGLWindow_MouseWheel(OpenTK.Windowing.Common.MouseWheelEventArgs obj)
        {
            UI.OnMouseWheel(new Vector2(obj.OffsetX, obj.OffsetY));
        }

        private void MainGLWindow_MouseMove(OpenTK.Windowing.Common.MouseMoveEventArgs obj)
        {
            UI.OnMouseMove(MousePosition.X, MousePosition.Y);
        }

        private void MainGLWindow_MouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs obj)
        {
            InfinityUI.Interfaces.MouseButton btn = InfinityUI.Interfaces.MouseButton.None;

            switch (obj.Button)
            {
                case MouseButton.Left:
                    btn = InfinityUI.Interfaces.MouseButton.Left | InfinityUI.Interfaces.MouseButton.Down;
                    break;
                case MouseButton.Right:
                    btn = InfinityUI.Interfaces.MouseButton.Right | InfinityUI.Interfaces.MouseButton.Down;
                    break;
                case MouseButton.Middle:
                    btn = InfinityUI.Interfaces.MouseButton.Middle | InfinityUI.Interfaces.MouseButton.Down;
                    break;
            }

            if (btn != InfinityUI.Interfaces.MouseButton.None)
            {
                UI.OnMouseClick(btn);
            }
        }

        private void MainGLWindow_TextInput(OpenTK.Windowing.Common.TextInputEventArgs obj)
        {
            currentTextInput = obj.AsString;

            InfinityUI.Interfaces.KeyboardEventArgs kevent = new InfinityUI.Interfaces.KeyboardEventArgs()
            {
                Key = currentKey,
                Char = currentTextInput,
                IsAlt = IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt),
                IsLeftAlt = IsKeyDown(Keys.LeftAlt),
                IsRightAlt = IsKeyDown(Keys.RightAlt),
                IsCtrl = IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl),
                IsLeftCtrl = IsKeyDown(Keys.LeftControl),
                IsRightCtrl = IsKeyDown(Keys.RightControl),
                IsShift = IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift),
                IsLeftShift = IsKeyDown(Keys.LeftShift),
                IsRightShift = IsKeyDown(Keys.RightShift),
                IsCapsLock = IsKeyDown(Keys.CapsLock),
                GetClipboardContent = () =>
                {
                    return ClipboardString;
                },
                SetClipboardContent = (o) => {
                    if (o == null)
                    {
                        ClipboardString = "";
                    }
                    else
                    {
                        ClipboardString = o.ToString();
                    }
                }
            };

            UI.OnTextInput(kevent);
        }

        private void InitializeEvents()
        {
            Minimized += MainGLWindow_Minimized;
            TextInput += MainGLWindow_TextInput;
            MouseDown += MainGLWindow_MouseDown;
            MouseMove += MainGLWindow_MouseMove;
            MouseWheel += MainGLWindow_MouseWheel;
            MouseUp += MainGLWindow_MouseUp;
            KeyDown += MainGLWindow_KeyDown;
            KeyUp += MainGLWindow_KeyUp;
            Closing += MainGLWindow_Closing;
            FileDrop += MainGLWindow_FileDrop;
        }

        private void MainGLWindow_FileDrop(OpenTK.Windowing.Common.FileDropEventArgs obj)
        {
            UI.OnFileDrop(obj.FileNames);
        }

        private void MainGLWindow_Minimized(OpenTK.Windowing.Common.MinimizedEventArgs obj)
        {
            isMinimized = true;
        }

        private void UpdateSize()
        {
            graphCanvas?.Resize(Size.X, Size.Y);
            rootCanvas?.Resize(Size.X, Size.Y);
        }
        #endregion

        private void InitializeUI()
        {
            if (IsExiting) return;
            if (UI.Initied) return;
            long ms = Environment.TickCount;
            UI.Init();

            rootArea = new UIObject();
            graphArea = new UIObject();

            graphCanvas = graphArea.AddComponent<UICanvas>();
            rootCanvas = rootArea.AddComponent<UICanvas>();

            activeDocument = new UIGraph();

            activeGraph = new Graph("Untitled", 512, 512)
            {
                DefaultTextureType = GraphPixelType.RGBA //testing
            }; //testing

            graphArea.AddChild(activeDocument);

            preview2D = new UI2DPreview();
            rootArea.AddChild(preview2D);

            preview3D = new UI3DPreview();
            rootArea.AddChild(preview3D);

            shelf = new UIShelf();
            rootArea.AddChild(shelf);

            //GraphTemplate.PBRFull(activeGraph);
            activeDocument.Load(activeGraph.GetJson(), "");
            activeGraph = activeDocument.Current;
            Debug.WriteLine("UI Initialize: " + (Environment.TickCount - ms) + "ms");
        }

        private void InitializePBR()
        {
            if (IsExiting) return;
            if (pbrInitialized) return;
            
            MLog.Log.Info("GL Version: " + OpenTK.Graphics.OpenGL.GL.GetString(OpenTK.Graphics.OpenGL.StringName.Version));

            pbrInitialized = true;
            BRDF.Create();
            InitializeHDR();
        }

        private void InitializeGL()
        {
            if (IsExiting) return;

            GLFW.SwapInterval(0);

            IGL.Primary.PatchParameter((int)PatchParameterInt.PatchVertices, 3);
            IGL.Primary.Enable((int)EnableCap.Blend);
            IGL.Primary.Enable((int)KhrBlendEquationAdvancedCoherent.BlendAdvancedCoherentKhr);
            IGL.Primary.BlendFunc((int)BlendingFactor.One, (int)BlendingFactor.OneMinusSrcAlpha);

            IGL.Primary.Enable((int)EnableCap.PointSprite);
            
            IGL.Primary.Enable((int)EnableCap.DepthTest);
            IGL.Primary.DepthFunc((int)DepthFunction.Lequal);

            IGL.Primary.Enable((int)EnableCap.LineSmooth);
        }

        private void InitializeViewport()
        {
            IGL.Primary.Viewport(0, 0, Size.X, Size.Y);
            IGL.Primary.ClearColor(0, 0, 0, 1);
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit | (int)ClearBufferMask.StencilBufferBit);
        }

        private void Render()
        {
            if (IsExiting || !IsVisible 
                || WindowState == OpenTK.Windowing.Common.WindowState.Minimized)
            {
                return;
            }

            if (isMinimized)
            {
                Restored?.Invoke();
                isMinimized = false;
            }

            MakeCurrent();

            InitializeGL();
            InitializeUI();
            InitializePBR();

            UpdateSize();

            //process hdr load
            ProcessHdr();

            PollGraph();

            //next render 3d preview
            preview3D?.Render();
            
            InitializeViewport();
            //draw UI last
            UI.Draw();
        }

        private void PollGraph()
        {
            if (IsExiting || !IsVisible) return;

            FullScreenQuad.SharedVao?.Bind();
            //poll graph updates if any
            //and let them render first before anything else
            activeGraph?.PollScheduled();

            FullScreenQuad.SharedVao?.Unbind();
        }

        private void ProcessHdr()
        {
            if (hdrToLoad == null) return;

            hdrMap.Dispose();
            
            hdrMap = HdriManager.Process(hdrToLoad);
            hdrToLoad = null;
            
            GlobalEvents.Emit(GlobalEvent.HdriUpdate, hdrMap.irradiance, hdrMap.prefilter);
            GlobalEvents.Emit(GlobalEvent.SkyboxUpdate, this, hdrMap.environment);
        }

        //hdr test load
        private void InitializeHDR()
        {
            string dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hdr");
            IHdrFile f = null;
            Task.Run(() =>
            {
                HdriManager.Scan(dir);
                var available = HdriManager.Available;
                Debug.WriteLine("found hdrs: " + available.Count);
                if (available.Count == 0)
                {
                    return;
                }

                f = HdriManager.Load(available[0]);
            }).ContinueWith(t =>
            {
                if (f == null) return;
                hdrToLoad = f;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public virtual void Process()
        {
            ProcessEvents();
            Invalidate();
        }

        protected virtual void Invalidate()
        {
            if (IsExiting || !IsVisible) return;

            /*if (lastTick == 0)
            {
                lastTick = System.DateTime.Now.Ticks;
                return;
            }

            if (fpsTick == 0)
            {
                fpsTick = 1.0f / TargetFPS;
            }

            float diff = (float)(new TimeSpan(System.DateTime.Now.Ticks - lastTick).TotalMilliseconds);
            deltaTime += diff;

            if (deltaTime >= fpsTick)
            {
                Render();
                SwapBuffers();

                deltaTime %= fpsTick;
            }

            lastTick = DateTime.Now.Ticks;*/

            try
            {
                Render();
                SwapBuffers();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        public virtual void Show()
        {
            IsVisible = true;
            OnResize(new OpenTK.Windowing.Common.ResizeEventArgs(Size));
        }

        public virtual void Hide()
        {
            IsVisible = false;
        }

        public virtual void SwapBuffers()
        {
            if (IsExiting || !IsVisible) return;

            unsafe
            {
                GLFW.SwapBuffers(WindowPtr);
            }
        }

        private void InternalDispose()
        {
            hdrToLoad?.Dispose();
            //testing here only
            hdrMap.Dispose();

            GridGenerator.Dispose();

            UI.Dispose();

            Function.DisposeCache();
            BRDF.Dispose();
            ImageProcessor.DisposeCache();

            FontManager.Dispose();
            GeometryCache.Dispose();
            GLShaderCache.Dispose();
        }
    }
}
