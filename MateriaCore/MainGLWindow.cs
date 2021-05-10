using InfinityUI.Components;
using InfinityUI.Core;
using Materia.Graph;
using Materia.Rendering.Fonts;
using Materia.Rendering.Geometry;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Material;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;
using MateriaCore.Components.GL;
using MateriaCore.Components.GL.Renderer;
using MateriaCore.Utils;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore
{
    public class MainGLWindow : NativeWindow
    {
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

        #region 3D Stuff
        protected UIWindow scenePreview;
        protected ISceneRenderer sceneRenderer;
        protected Scene scene;
        #endregion

        protected Graph activeGraph;

        private Keys currentKey;
        private string currentTextInput;

        public MainGLWindow(NativeWindowSettings settings) : base(settings)
        {
            InitializeEvents();
        }

        #region Window Events
        private void MainGLWindow_Closing(System.ComponentModel.CancelEventArgs obj)
        {
            InternalDispose();
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
            TextInput += MainGLWindow_TextInput;
            MouseDown += MainGLWindow_MouseDown;
            MouseMove += MainGLWindow_MouseMove;
            MouseWheel += MainGLWindow_MouseWheel;
            MouseUp += MainGLWindow_MouseUp;
            KeyDown += MainGLWindow_KeyDown;
            KeyUp += MainGLWindow_KeyUp;
            Closing += MainGLWindow_Closing;
        }
        #endregion

        private void InitializeUI()
        {
            if (IsExiting) return;
            if (UI.Initied) return;
            UI.Init();

            rootArea = new UIObject();
            graphArea = new UIObject();

            graphCanvas = graphArea.AddComponent<UICanvas>();
            rootCanvas = rootArea.AddComponent<UICanvas>();

            MLog.Log.Info("GL Version: " + OpenTK.Graphics.OpenGL.GL.GetString(OpenTK.Graphics.OpenGL.StringName.Version));
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

            IGL.Primary.Enable((int)EnableCap.CullFace);
            IGL.Primary.CullFace((int)CullFaceMode.Back);
        }

        private void InitializeViewport()
        {
            IGL.Primary.Viewport(0, 0, Size.X, Size.Y);
            IGL.Primary.ClearColor(1, 1, 1, 1);
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit | (int)ClearBufferMask.StencilBufferBit);
        }

        private void Render()
        {
            if (IsExiting || !IsVisible 
                || WindowState == OpenTK.Windowing.Common.WindowState.Minimized)
            {
                return;
            }

            MakeCurrent();

            InitializeGL();
            InitializeViewport();
            InitializeUI();

            //poll graph updates if any
            //and let them render first before anything else
            activeGraph?.PollScheduled();

            //update scene view area
            if (scenePreview != null) 
            {
                scene?.UpdateViewSize(scenePreview.AnchoredSize);
            }

            //render 3d scene to buffer
            sceneRenderer?.Render(scene);
            //send out event update of rendered image
            if (sceneRenderer != null)
            {
                GlobalEvents.Emit(GlobalEvent.Preview3D, this, sceneRenderer.Image);
            }

            //ensure ui canvases are resized appropriately
            graphCanvas?.Resize(Size.X, Size.Y);
            rootCanvas?.Resize(Size.X, Size.Y);

            //draw UI last
            UI.Draw();
        }

        public virtual void Invalidate()
        {
            if (lastTick == 0)
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

            lastTick = DateTime.Now.Ticks;
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
            unsafe
            {
                GLFW.SwapBuffers(WindowPtr);
            }
        }

        private void InternalDispose()
        {
            sceneRenderer?.Dispose();
            scene?.Dispose();
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
