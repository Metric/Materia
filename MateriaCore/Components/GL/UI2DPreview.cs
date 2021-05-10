﻿using InfinityUI.Components;
using InfinityUI.Controls;
using InfinityUI.Core;
using Materia.Nodes;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UI2DPreview : UIWindow
    {
        public const float DEFAULT_WIDTH_PERCENT = 0.25f;
        public const float DEFAULT_HEIGHT_PERCENT = 0.1f;

        protected Node activeNode;

        #region Components
        protected UIObject internalContainer;
        protected UIImage internalBackground;

        protected MovablePane uvArea;
        protected MovablePane imageArea;

        #endregion

        protected const float ZOOM_SPEED = 1.0f / 10.0f;

        public UI2DPreview() : base(new Vector2(DEFAULT_WIDTH_PERCENT, DEFAULT_HEIGHT_PERCENT), "2D Preview")
        {
            RelativeTo = Anchor.BottomRight;
            InitializeComponents();
            GlobalEvents.On(GlobalEvent.Preview2D, OnPreview);
            GlobalEvents.On(GlobalEvent.Preview2DUV, OnPreviewUV);
        }

        private void OnPreviewUV(object sender, object t)
        {
            GLTexture2D tex = t as GLTexture2D;
            uvArea.Visible = tex != null;
            uvArea.Background.Texture = tex == null ? UI.DefaultWhite : tex;
        }

        private void OnPreview(object sender, object n)
        {
            activeNode = n as Node;
            imageArea.Visible = activeNode != null;
            imageArea.Background.Texture = activeNode == null ? UI.DefaultWhite : activeNode.GetActiveBuffer();
            imageArea.Size = activeNode == null ? new Vector2(512, 512) : new Vector2(activeNode.Width, activeNode.Height);
            uvArea.Size = activeNode == null ? new Vector2(512, 512) : new Vector2(activeNode.Width, activeNode.Height);
            ResetView();
        }

        private void ResetView()
        {
            imageArea.Scale = Vector2.One;
            uvArea.Scale = Vector2.One;
            imageArea?.MoveTo(Vector2.Zero);
            uvArea?.MoveTo(Vector2.Zero);
        }

        private void InitializeComponents()
        {
            internalContainer = new UIObject
            {
                RelativeTo = Anchor.Fill,
            };
            internalBackground = internalContainer.AddComponent<UIImage>();
            internalBackground.Color = new Vector4(0.25f, 0.25f, 0.25f, 1); //todo: use theme class
            internalBackground.Clip = true;

            uvArea = new MovablePane(new Vector2(512, 512))
            {
                RelativeTo = Anchor.Center,
                SnapMode = MovablePaneSnapMode.Grid,
                SnapTolerance = 5,
                Visible = false
            };
            uvArea.Moved += UvArea_Moved;

            imageArea = new MovablePane(new Vector2(512,512))
            {
                RelativeTo = Anchor.Center,
                SnapMode = MovablePaneSnapMode.Grid,
                SnapTolerance = 5,
                Visible = false
            };
            imageArea.Moved += ImageArea_Moved;
            selectable.Wheel += Selectable_Wheel;

            content.AddChild(internalContainer);

            internalContainer.AddChild(imageArea);
            internalContainer.AddChild(uvArea);
        }

        private void ImageArea_Moved(MovablePane arg1, Vector2 delta)
        {
            uvArea?.Move(delta, false);
        }

        private void UvArea_Moved(MovablePane arg1, Vector2 delta)
        {
            imageArea?.Move(delta, false);
        }

        private void Selectable_Wheel(UISelectable arg1, InfinityUI.Interfaces.MouseWheelArgs e)
        {
            imageArea.Scale += new Vector2(e.Delta.Y * ZOOM_SPEED);
            uvArea.Scale += new Vector2(e.Delta.Y * ZOOM_SPEED);
        }

        public override void Dispose(bool disposing = true)
        {
            GlobalEvents.Off(GlobalEvent.Preview2D, OnPreview);
            GlobalEvents.Off(GlobalEvent.Preview2DUV, OnPreviewUV);

            base.Dispose(disposing);
        }

        //todo: add interactive control capabilities for specific nodes / node parameters
    }
}
