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

            if (string.IsNullOrEmpty(text))
            {
                totalWidths.Clear();
                return;
            }

            string[] lines = text.Split("\r\n");
            totalWidths.Clear();

            float rheight = 0;
            float maxWidth = 0;

            for (int i = 0; i < lines.Length; ++i)
            {
                float twidth = 0;
                string line = lines[i];

                float maxBearing = 0;

                maxWidth = MathF.Max(maxWidth, twidth);

                for (int k = 0; k < line.Length; ++k)
                {
                    string sb = line.Substring(k, 1);
                    FontManager.CharData data = null;
                    if (map.TryGetValue(sb, out data))
                    {
                        maxBearing = MathF.Max(maxBearing, data.bearing);
                        twidth += data.size.X + spacing;
                        maxWidth = MathF.Max(maxWidth, twidth);
                    }
                }

                rheight += maxBearing;
                totalWidths.Add(twidth);
            }

            //update final render size for info you need it later
            switch (Parent.RelativeTo)
            {
                case Anchor.BottomHorizFill:
                case Anchor.TopHorizFill:
                case Anchor.CenterHorizFill:
                    Parent.Size = new Vector2(Parent.Size.X, rheight);
                    break;
                case Anchor.Fill:
                    break;
                default:
                    Parent.Size = new Vector2(maxWidth, rheight);
                    break;
            }

        }

        public override void Draw(Matrix4 projection)
        {
            if (Parent == null) return;
            if (Shader == null) return;
            if (!Parent.Visible) return;
            if (string.IsNullOrEmpty(text)) return;

            Matrix4 m = Parent.ModelMatrix;
            Vector2 pos = Parent.AnchoredPosition;
            Vector2 size = Parent.AnchoredSize;
            Vector4 color = Color;

            Shader.Use();
            Shader.SetUniformMatrix4("projectionMatrix", ref projection);
            Shader.SetUniformMatrix4("modelMatrix", ref m);
            Shader.SetUniform2("position", ref pos);
            Shader.SetUniform2("size", ref size);
            Shader.SetUniform4("color", ref color);
            Shader.SetUniform("MainTex", 0);
            Shader.SetUniform("flipY", FlipY ? 1 : 0);

            if (character == null)
            {
                character = new GLTexture2D(PixelInternalFormat.Rgba);
                character.Bind();
                character.Linear();
                character.ClampToEdge();
                GLTexture2D.Unbind();
            }

            string[] lines = text.Split("\r\n");

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            character.Bind();

            float bearingSign = 1;
            bool vadjust = false;
            switch (Parent.RelativeTo)
            {
                case Anchor.Bottom:
                case Anchor.BottomLeft:
                case Anchor.BottomRight:
                case Anchor.BottomHorizFill:
                    bearingSign = -1;
                    vadjust = true;
                    break;
            }       

            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                float left = 0;
                float tWidth = totalWidths[i];
                float adjust = 0;

                float diff = MathF.Abs(size.X - tWidth);
                float idiff = vadjust ? (lines.Length - i) : i;

                switch (alignment)
                {
                    case TextAlignment.Center:
                        //why does this work and not simply diff * 0.5f?
                        adjust = MathF.Floor(diff * 0.5f - diff * 0.125f);
                        break;
                    case TextAlignment.Right:
                        //why does this work and not simply diff?
                        adjust = MathF.Floor(diff - diff * 0.25f);
                        break;
                }

                for (int j = 0; j < line.Length; ++j)
                {
                    string ch = line.Substring(j, 1);
                    FontManager.CharData data = null;
                    if (map.TryGetValue(ch, out data))
                    {
                        if (data.texture == null)
                        {
                            continue;
                        }

                        Vector2 csize = data.size;
                        Vector2 finalPos = new Vector2(pos.X + left + adjust, pos.Y - (csize.Y * 0.5f) + idiff * (data.bearing * bearingSign));
                        Box2 charRect = new Box2(finalPos.X, finalPos.Y, finalPos.X + csize.X, finalPos.Y + csize.Y);

                        if (Parent.Parent != null)
                        {
                            var drawable = Parent.Parent.GetComponent<UIDrawable>();

                            if (drawable != null && drawable.Clip)
                            {
                                if (!Parent.Parent.Rect.Intersects(charRect))
                                {
                                    continue;
                                }
                            }
                        }

                        left += (data.size.X + spacing);

                        character.SetData(data.texture.Image, PixelFormat.Bgra, (int)MathF.Ceiling(data.size.X), (int)MathF.Ceiling(data.size.Y));

                        Shader.SetUniform2("position", ref finalPos);
                        Shader.SetUniform2("size", ref csize);

                        UIRenderer.Draw();
                    }
                }
            }

            GLTexture2D.Unbind();
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
