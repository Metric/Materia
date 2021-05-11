using InfinityUI.Core;
using Materia.Rendering.Fonts;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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

        protected GLTexture2D character;

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

        protected Dictionary<string, FontManager.CharData> map = new Dictionary<string, FontManager.CharData>();
        protected List<float> totalWidths = new List<float>();

        public override void Awake()
        {
            base.Awake();
            if (Parent == null) return;
            //by default text view should not be raycasted to
            Parent.RaycastTarget = false;
        }

        public Vector2 Measure(string s)
        {
            return FontManager.MeasureString(FontFamily, FontSize, s, Style);
        }

        protected void TryAndGenerateMap()
        {
            if (text == null || text.Length == 0) return;
            map = FontManager.Generate(fontFamily, fontSize, text, style);
        }

        protected void CalculateAlignment()
        {
            if (Parent == null) return;
        }

        //todo: rework this text class
        public override void Draw(Matrix4 projection)
        {
            if (Parent == null) return;
            if (Shader == null) return;
            if (!Parent.Visible) return;
            if (string.IsNullOrEmpty(text)) return;
        }

        public override void Invalidate()
        {
            TryAndGenerateMap();
            CalculateAlignment();
        }

        public override void Dispose()
        {
            character?.Dispose();
            character = null;
        }
    }
}
