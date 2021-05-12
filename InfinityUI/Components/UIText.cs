﻿using InfinityUI.Core;
using Materia.Rendering.Fonts;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;
using Materia.Rendering.Textures;
using Rendering.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using static Materia.Rendering.Fonts.FontManager;

namespace InfinityUI.Components
{
    public enum TextAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public class UIText : UIDrawable
    {
        public const string DefaultFont = "Segoe UI";

        protected GLTexture2D characters;
        protected TextRenderer renderer;
        protected CharAtlas map;

        protected string text;
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text != value)
                {
                    text = value;
                    Invalidate();
                }
            }
        }

        protected float fontSize = 18;
        public float FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                if (fontSize != value)
                {
                    fontSize = value;
                    Invalidate();
                }
            }
        }

        protected string fontFamily = DefaultFont;
        public string FontFamily
        {
            get
            {
                return fontFamily;
            }
            set
            {
                if (fontFamily != value)
                {
                    fontFamily = value;
                    Invalidate();
                }
            }
        }

        protected float spacing;
        public float Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                if (spacing != value)
                {
                    spacing = value;
                    Invalidate();
                }
            }
        }


        protected FontStyle style = FontStyle.Regular;
        public FontStyle Style
        {
            get
            {
                return style;
            }
            set
            {
                if (style != value)
                {
                    style = value;
                    Invalidate();
                }
            }
        }

        protected TextAlignment alignment = TextAlignment.Left;
        public TextAlignment Alignment
        {
            get
            {
                return alignment;
            }
            set
            {
                if (alignment != value)
                {
                    alignment = value;
                    Invalidate();
                }
            }
        }

        public override void Awake()
        {
            base.Awake();
            if (Parent == null) return;
            //by default text view should not be raycasted to
            Shader = GLShaderCache.GetShader("pointuiuv.glsl", "pointuiuv.glsl", "pointui.glsl");
            Parent.RaycastTarget = false;
        }

        public Vector2 Measure(string s)
        {
            if (map == null) return Vector2.Zero;

            Vector2 area = Vector2.Zero;
            string[] lines = s.Split("\r\n");
            for (int l = 0; l < lines.Length; ++l)
            {
                Vector2 lineSize = MeasureLine(lines[l]);
                area.X = MathF.Max(lineSize.X, area.X);
                area.Y += lineSize.Y;
            }
            return area;
        }

        protected Vector2 MeasureLine(string s)
        {
            if (map == null) return Vector2.Zero;

            Vector2 area = Vector2.Zero;
            for (int i = 0; i < s.Length; ++i)
            {
                var c = s[i];
                var d = map.Get(c);
                if (d == null) continue;
                area.X += d.info.size.Width;
                area.Y = MathF.Max(d.info.lineHeight, area.Y);
            }
            return area;
        }

        protected void TryAndGenerateMap()
        {
            var incomingMap = FontManager.GetAtlas(fontFamily, fontSize, style);

            if (incomingMap != null)
            {
                map = incomingMap;
                characters = map.atlas;
            }

            if (renderer == null)
            {
                renderer = new TextRenderer();
            }
        }

        protected void ArrangeVertices()
        {
            if (map == null || renderer == null) return;
            if (string.IsNullOrEmpty(text)) return;

            List<TextData> tdata = new List<TextData>();
            Vector2 area = Measure(text);

            if (Parent != null)
            {
                Parent.Size = area;
            }

            float y = 0;
            string[] lines = text.Split("\r\n");
            for (int i = 0; i < lines.Length; ++i)
            {
                List<TextData> lineData = new List<TextData>();
                string line = lines[i];

                float x = 0;
                float maxY = 0;
                float xOffset = 0;

                switch (alignment)
                {
                    case TextAlignment.Center:
                        xOffset = area.X * 0.5f;
                        break;
                    case TextAlignment.Right:
                        xOffset = area.X;
                        break;
                    case TextAlignment.Left:
                        xOffset = 0;
                        break;
                }

                for (int k = 0; k < line.Length; ++k)
                {
                    var c = line[k];
                    var d = map.Get(c);
                    if (d == null) continue;
                    Vector2 csize = new Vector2(d.info.size.Width, d.info.size.Height);
                    maxY = MathF.Max(d.info.lineHeight, maxY);
                    Vector2 pos = new Vector2(x + xOffset, y);
                    x += csize.X;
                    TextData t = new TextData
                    {
                        pos = pos,
                        uv = d.uv,
                        size = csize
                    };
                    lineData.Add(t);
                }

                switch (alignment)
                {
                    case TextAlignment.Center:
                        xOffset = x * 0.5f;
                        break;
                    case TextAlignment.Right:
                        xOffset = x;
                        break;
                    case TextAlignment.Left:
                        xOffset = 0;
                        break;
                }

                for (int k = 0; k < lineData.Count; ++k)
                {
                    var d = lineData[k];
                    d.pos -= new Vector2(xOffset, 0);
                }

                tdata.AddRange(lineData);

                y += maxY;
            }

            renderer.Text = tdata;
            renderer.Update();
        }

        public override void Draw(Matrix4 projection)
        {
            if (Parent == null) return;
            if (Shader == null) return;
            if (renderer == null) return;
            if (map == null) return;
            if (characters == null) return;
            if (!Parent.Visible) return;
            if (string.IsNullOrEmpty(text)) return;

            Vector2 size = Parent.AnchoredSize;

            if (size.X <= float.Epsilon || size.Y <= float.Epsilon) return;

            OnBeforeDraw(this);

            Matrix4 m = Parent.ModelMatrix;
            Vector4 color = Color;
            Vector2 tiling = Tiling;
            Vector2 pos = Parent.AnchoredPosition;

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            characters.Bind();

            Shader.Use();
            Shader.SetUniformMatrix4("projectionMatrix", ref projection);
            Shader.SetUniformMatrix4("modelMatrix", ref m);
            Shader.SetUniform2("offset", ref pos);
            Shader.SetUniform4("color", ref color);
            Shader.SetUniform("MainTex", 0);
            Shader.SetUniform("flipY", FlipY ? 1 : 0);
            Shader.SetUniform2("tiling", ref tiling);

            TextRenderer.SharedVao.Bind();

            if (!Clip)
            {
                renderer?.Draw();
                UIRenderer.Bind();
                return;
            }

            UIRenderer.StencilStage++;
            //wrap stencil stage as max is 255 for stencil
            UIRenderer.StencilStage %= 255;

            IGL.Primary.StencilOp((int)StencilOp.Keep, (int)StencilOp.Keep, (int)StencilOp.Replace);

            IGL.Primary.StencilFunc((int)StencilFunction.Always, UIRenderer.StencilStage, UIRenderer.StencilStage);
            IGL.Primary.StencilMask(0xFF);

            renderer?.Draw();

            IGL.Primary.StencilFunc((int)StencilFunction.Equal, UIRenderer.StencilStage, UIRenderer.StencilStage);
            IGL.Primary.StencilMask(0x00);

            UIRenderer.Bind();
        }

        public override void Invalidate()
        {
            TryAndGenerateMap();
            ArrangeVertices();
        }

        public override void Dispose()
        {
            renderer?.Dispose();
            renderer = null;
        }
    }
}
