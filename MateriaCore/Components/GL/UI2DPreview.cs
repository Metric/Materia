using InfinityUI.Components;
using InfinityUI.Components.Layout;
using InfinityUI.Controls;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Nodes;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UI2DPreview : UIWindow
    {
        protected Node activeNode;

        #region Components
        protected UIObject internalContainer;
        protected UIImage internalBackground;

        protected MovablePane uvArea;
        protected MovablePane imageArea;

        protected UIObject leftStack;
        protected UIObject rightStack;
        protected UIObject bottomBar;

        protected UIObject zoomLevelArea;
        protected UIText zoomLevel;

        protected ToggleButton toggleUV;
        protected Button zoomIn;
        protected Button zoomOut;
        protected Button ratio1x1;
        protected Button fitIntoView;
        #endregion

        protected static Vector2 ZOOM_INCREMENT = new Vector2(0.02f, 0.02f);
        protected const float ZOOM_SPEED = 1.0f / 60.0f;

        public UI2DPreview() : base(new Vector2(956f, 512f), "2D Preview")
        {
            RelativeTo = Anchor.BottomRight;
            InitializeComponents();
            GlobalEvents.On(GlobalEvent.Preview2D, OnPreview);
            GlobalEvents.On(GlobalEvent.Preview2DUV, OnPreviewUV);
        }

        private void OnPreviewUV(object sender, object t)
        {
            GLTexture2D tex = t as GLTexture2D;
            uvArea.Background.Texture = tex ?? UI.DefaultWhite;    
        }

        private void OnPreview(object sender, object n)
        {
            activeNode = n as Node;
            var buffer = activeNode?.GetActiveBuffer();
            imageArea.Visible = activeNode != null;
            imageArea.Background.Texture = buffer ?? UI.DefaultWhite;
            imageArea.Size = activeNode == null ? new Vector2(512, 512) : new Vector2(activeNode.Width, activeNode.Height);
            uvArea.Size = activeNode == null ? new Vector2(512, 512) : new Vector2(activeNode.Width, activeNode.Height);
            FitIntoView_Submit(null);
        }

        private void ResetView()
        {
            imageArea.Scale = Vector2.One;
            uvArea.Scale = Vector2.One;
            imageArea.MoveTo(Vector2.Zero);
            uvArea.MoveTo(Vector2.Zero);
            UpdateZoomText();
        }

        private void InitializeComponents()
        { 
            internalContainer = new UIObject
            {
                RaycastTarget = true,
                RelativeTo = Anchor.Fill,
            };
            internalBackground = internalContainer.AddComponent<UIImage>();
            internalBackground.Color = new Vector4(0.05f, 0.05f, 0.05f, 1); //todo: use theme class
            internalBackground.Clip = true;

            uvArea = new MovablePane(new Vector2(512, 512))
            {
                RelativeTo = Anchor.Center,
                SnapMode = MovablePaneSnapMode.None,
                Visible = false,
                Origin = new Vector2(0.5f, 0.5f),
                RaycastTarget = true
            };
            uvArea.Moved += UvArea_Moved;
            uvArea.Background.Color = new Vector4(1, 1, 1, 1);

            imageArea = new MovablePane(new Vector2(512, 512))
            {
                SnapMode = MovablePaneSnapMode.None,
                Origin = new Vector2(0.5f, 0.5f),
                RelativeTo = Anchor.Center,
                RaycastTarget = true
            };

            imageArea.Background.Color = new Vector4(1, 1, 1, 1);
            imageArea.Moved += ImageArea_Moved;

            bottomBar = new UIObject
            {
                Size = new Vector2(1, 32),
                RelativeTo = Anchor.BottomHorizFill,
                RaycastTarget = true
            };
            var bottomBarBg = bottomBar.AddComponent<UIImage>();
            bottomBarBg.Color = new Vector4(0.1f, 0.1f, 0.1f, 1);

            #region left button stack
            leftStack = new UIObject
            {
                RelativeTo = Anchor.Left,
                RaycastTarget = true
            };
            var leftStackPanel = leftStack.AddComponent<UIStackPanel>();

            toggleUV = new ToggleButton("UV", new Vector2(32, 32));
            toggleUV.Submit += ToggleUV_Submit;

            leftStack.AddChild(toggleUV);
            leftStackPanel.ChildAlignment = Anchor.Left;
            #endregion


            #region right button stack
            rightStack = new UIObject
            {
                RelativeTo = Anchor.Right,
                RaycastTarget = true
            };
            var rightStackPanel = rightStack.AddComponent<UIStackPanel>();

            zoomIn = new Button("", new Vector2(32, 32));
            zoomIn.Background.Texture = UI.GetEmbeddedImage(Icons.ADD, typeof(UI2DPreview));
            zoomIn.Submit += ZoomIn_Submit;

            rightStack.AddChild(zoomIn);

            zoomLevelArea = new UIObject
            {
                RaycastTarget = false,
            };
            zoomLevel = zoomLevelArea.AddComponent<UIText>();
            zoomLevel.Text = "100%";
            zoomLevel.Alignment = TextAlignment.Center;

            rightStackPanel.ChildAlignment = Anchor.Left;
            rightStack.AddChild(zoomLevelArea);

            zoomOut = new Button("", new Vector2(32, 32));
            zoomOut.Background.Texture = UI.GetEmbeddedImage(Icons.MINUS, typeof(UI2DPreview));
            zoomOut.Submit += ZoomOut_Submit;
            rightStack.AddChild(zoomOut);

            ratio1x1 = new Button("", new Vector2(32, 32));
            ratio1x1.Background.Texture = UI.GetEmbeddedImage(Icons.ONE_X_ONE, typeof(UI2DPreview));
            ratio1x1.Submit += Ratio1x1_Submit;
            rightStack.AddChild(ratio1x1);

            fitIntoView = new Button("", new Vector2(32, 32));
            fitIntoView.Background.Texture = UI.GetEmbeddedImage(Icons.ASPECT, typeof(UI2DPreview));
            fitIntoView.Submit += FitIntoView_Submit;

            rightStack.AddChild(fitIntoView);
            #endregion

            bottomBar.AddChild(leftStack);
            bottomBar.AddChild(rightStack);

            selectable.BubbleEvents = false;
            selectable.Wheel += Selectable_Wheel;

            internalContainer.AddChild(imageArea);
            internalContainer.AddChild(uvArea);
            content.AddChild(internalContainer);
            content.AddChild(bottomBar);
        }

        private void FitIntoView_Submit(Button obj)
        {
            ResetView();
            Vector2 size = internalContainer.AnchorSize;
            Vector2 imageSize = imageArea.Size;
            float minViewArea = MathF.Min(size.X, size.Y);
            float maxSize = MathF.Max(imageSize.X, imageSize.Y);
            float scale = minViewArea / maxSize;
            imageArea.Scale = new Vector2(scale, scale);
            uvArea.Scale = new Vector2(scale, scale);
            UpdateZoomText();
        }

        private void Ratio1x1_Submit(Button obj)
        {
            ResetView();
            UpdateZoomText();
        }

        private void ZoomOut_Submit(Button obj)
        {
            imageArea.Scale -= ZOOM_INCREMENT;
            uvArea.Scale -= ZOOM_INCREMENT;
            UpdateZoomText();
        }

        private void ZoomIn_Submit(Button obj)
        {
            imageArea.Scale += ZOOM_INCREMENT;
            uvArea.Scale += ZOOM_INCREMENT;
            UpdateZoomText();
        }

        private void ToggleUV_Submit(Button obj)
        {
            uvArea.Visible = !uvArea.Visible;
        }

        private void ImageArea_Moved(MovablePane arg1, Vector2 delta, MouseEventArgs e)
        {
            Debug.WriteLine("preview 2d image area moved: " + delta.ToString());
            uvArea?.Move(delta, false, e);
        }

        private void UvArea_Moved(MovablePane arg1, Vector2 delta, MouseEventArgs e)
        {
            Debug.WriteLine("preview 2d image uv area moved: " + delta.ToString());
            imageArea?.Move(delta, false, e);
        }

        private void Selectable_Wheel(UISelectable arg1, MouseWheelArgs e)
        {
            Vector2 delta = new Vector2(-e.Delta.Y * ZOOM_SPEED);
            imageArea.Scale += delta;
            uvArea.Scale += delta;
            UpdateZoomText();
        }

        private void UpdateZoomText()
        {
            float z = imageArea.Scale.X;
            
            //clamp it and reassign
            z = z.Clamp(0.02f, 3f);
            imageArea.Scale = new Vector2(z, z);
            uvArea.Scale = new Vector2(z, z);

            z = MathF.Round(z * 100);
            zoomLevel.Text = $"{z}%";
        }

        public override void Dispose(bool disposing = true)
        {
            GlobalEvents.Off(GlobalEvent.Preview2D, OnPreview);
            GlobalEvents.Off(GlobalEvent.Preview2DUV, OnPreviewUV);

            base.Dispose(disposing);
        }
    }
}
