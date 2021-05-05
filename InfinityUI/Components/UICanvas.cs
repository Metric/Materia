﻿using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityUI.Components
{
    public class UICanvas : IComponent
    {
        public UIObject Parent { get; set; }

        public float Width { get; protected set; }
        public float Height { get; protected set; }

        public Vector2 Size 
        { 
            get { return new Vector2(Width, Height); } 
        }

        public Vector2 PixelSize
        {
            get { return new Vector2(Width * scale, Height * scale); }
        }

        public Matrix4 Projection { get; protected set; }
        public Camera Cam { get; protected set; }

        protected float scaleWidthBase = 3896;
        public float ScaleWidthBase
        {
            get
            {
                return scaleWidthBase;
            }
            set
            {
                scaleWidthBase = value;
                if (AutoScale) Resize(Width, Height);
            }
        }

        public bool AutoScale { get; set; } = false;

        protected float scale = 1f;
        public float Scale
        {
            get { return scale; }
            set 
            {
                if (!AutoScale)
                {
                    scale = value;
                    Resize(Width, Height);
                }
            }
        }

        public bool Visible
        {
            get
            {
                return Parent != null ? Parent.Visible : false;
            }
            set
            {
                if (Parent != null)
                {
                    Parent.Visible = value;
                }
            }
        }

        public void Resize(float width, float height)
        {
            Width = width;
            Height = height;

            if (Parent != null)
            {
                Parent.Size = new Vector2(width, height);
            }

            CalculateScale();

            Projection = Matrix4.CreateScale(1, -1, 1) * Matrix4.CreateTranslation(-width * scale * 0.5f, height * scale * 0.5f, 0) * Matrix4.CreateOrthographic(width * scale, height * scale, 0.0f, 1000f);
        }

        public void Render()
        {
            if (Parent == null) return;

            var projection = Projection;

            for (int i = 0; i < Parent.Children.Count; ++i)
            {

                //Reset stencil per primary child of canvas
                UIRenderer.StencilStage = 0;
                IGL.Primary.StencilFunc((int)StencilFunction.Always, 1, 0xFF);
                IGL.Primary.Clear((int)ClearBufferMask.StencilBufferBit);
                IGL.Primary.StencilMask(0xFF);

                var child = Parent.Children[i];
                child.SendMessage("Draw", true, projection);
            }

            //Reset Stencil
            UIRenderer.StencilStage = 0;
            IGL.Primary.StencilFunc((int)StencilFunction.Always, 1, 0xFF);
            IGL.Primary.Clear((int)ClearBufferMask.StencilBufferBit);
            IGL.Primary.StencilMask(0xFF);
        }

        public virtual void Awake()
        {
            //register canvas for picking
            UI.RegisterCanvas(this);
        }

        private void CalculateScale()
        {
            if (AutoScale)
            {
                scale = ScaleWidthBase / Width;
            }
        }

        public virtual void Dispose()
        {
            //unregister canvas for picking
            UI.UnregisterCanvas(this);
        }
    }
}