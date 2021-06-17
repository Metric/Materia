using System;
using System.Collections.Generic;
using System.Drawing;
using Materia.Rendering.Mathematics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Materia.Rendering.Imaging;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Materia.Rendering.Textures;

namespace Materia.Rendering.Fonts
{
    //todo: rework this class to generate font atlas
    public class FontManager
    {
        static byte[] UNICODE_BASE = new byte[] 
        {
            0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,
            32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,
            58,59,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,
            85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,
            109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,
            128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,
            149,150,151,152,153,154,155,156,157,158,159,
            160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,176,177,178,179,180,
            181,182,183,184,185,186,187,188,189,190,191,192,193,194,195,196,197,198,199,200,201,
            202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,
            223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,
            244,245,246,247,248,249,250,251,252,253,254,255
        };

        //todo: unicode support for japanese etc.
        //todo: unicode support for cyrillic etc.

        public struct CharMapGroup
        {
            public float fontSize;
            public FontStyle style;
            public Font font;
            public string family;

            public override bool Equals(object obj)
            {
                if (!(obj is CharMapGroup)) return false;
                CharMapGroup g = (CharMapGroup)obj;
                return g.fontSize == fontSize && g.style == style && g.family == family;
            }

            public override int GetHashCode()
            {
                int hash = 7;
                hash = 31 * hash + fontSize.GetHashCode();
                hash = 31 * hash + style.GetHashCode();
                hash = 31 * hash + family.GetHashCode();
                return hash;
            }

            public override string ToString()
            {
                return fontSize + "," + style + "," + family;
            }
        }

        public class CharAtlasData
        {
            public CharData info;
            public Vector4 uv;
        }

        public class CharAtlas
        {
            public GLTexture2D atlas;
            public Dictionary<char, CharAtlasData> info;

            public CharAtlasData Get(char c)
            {
                CharAtlasData d = null;
                info?.TryGetValue(c, out d);
                return d;
            }
        }

        public class CharData
        {
            public char c;
            public SizeF size;
            public float lineHeight;
            public Bitmap render;
        }

        private static Dictionary<string, FontFamily> fonts;
        private static SolidBrush WhiteColor = new SolidBrush(Color.White);
        private static Bitmap fontHelper = new Bitmap(1, 1);
        private static Dictionary<CharMapGroup, CharAtlas> atlasCache = new Dictionary<CharMapGroup, CharAtlas>();

        const int BASE_TEX_WIDTH = 512;
        const int BASE_TEX_HEIGHT = 512;

        public static string[] FamilyNames
        {
            get
            {
                if (fonts == null) return new string[0];
                return fonts.Keys.ToArray();
            }
        }

        public static void GetAvailableFonts()
        {
            fonts = new Dictionary<string, FontFamily>();
            InstalledFontCollection installed = new InstalledFontCollection();
            for (int i = 0; i < installed.Families.Length; ++i)
            {
                var font = installed.Families[i];
                fonts[font.Name] = font;
            }
        }

        /// <summary>
        /// Gets the specified font altas if possible.
        /// Results in an atlas that is 512x512 px
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="style">The style.</param>
        /// <returns></returns>
        public static CharAtlas GetAtlas(string fontFamily, float fontSize, FontStyle style)
        {
            FontFamily family;
            if (!fonts.TryGetValue(fontFamily, out family))
            {
                if (fonts.Count > 0)
                {
                    family = fonts.Values.FirstOrDefault();
                }
            }
            if (family == null) return null;
            CharMapGroup group = new CharMapGroup
            {
                fontSize = fontSize,
                style = style,
                family = fontFamily
            };
            if (atlasCache.TryGetValue(group, out CharAtlas fatlas))
            {
                return fatlas;
            }

            Font f = new Font(family, fontSize, style, GraphicsUnit.Pixel);
           
            group.font = f;
            var characters = GetCharacters(ref group);

            int w = BASE_TEX_WIDTH;
            int h = BASE_TEX_HEIGHT;
            float quadSize = UNICODE_BASE.Length;

            //determine max texture size we need based on font size
            //previous formula was incorrect
            //this formula is correct and works as expected
            while (fontSize * fontSize * quadSize >= w * h)
            {
                w *= 2;
                h *= 2;

                if (w >= 8192) break;
            }

            Debug.WriteLine($"required font sheet size: {w},{h}");

            //TODO: separate out into multiple sheets if needed

            //clamp to 8K for now
            w = Math.Min(w, 8192);
            h = Math.Min(h, 8192);

            using (Bitmap atlas = new Bitmap(w,h,System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(atlas))
            {
                g.Clear(Color.Transparent);

                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float x = 0;
                float y = 0;
                float yMax = 0;

                Dictionary<char, CharAtlasData> atlasData = new Dictionary<char, CharAtlasData>();

                for (int i = 0; i < characters.Count; ++i)
                {
                    var c = characters[i];
                    if (x + c.size.Width >= w)
                    {
                        x = 0;
                        y += yMax;
                        yMax = 0;
                    }

                    if (y >= h)
                    {
                        break;
                    }

                    yMax = MathF.Max(c.size.Height, yMax);

                    Vector4 uv = new Vector4(
                            x / w, y / h,
                            (x + c.size.Width) / w, (y + c.size.Height) / h
                        );

                    CharAtlasData cdata = new CharAtlasData
                    {
                        info = c,
                        uv = uv
                    };

                    atlasData[c.c] = cdata;

                    if (c.render != null)
                    {
                        g.DrawImage(c.render, new System.Drawing.PointF(x, y));
                        c.render.Dispose();
                        c.render = null;
                    }

                    x += c.size.Width;
                }

                var rawMap = RawBitmap.FromBitmap(atlas);

                GLTexture2D atlasTexture = new GLTexture2D(Interfaces.PixelInternalFormat.Rgba8);
                atlasTexture.Bind();
                atlasTexture.SetData(rawMap.Image, Interfaces.PixelFormat.Bgra, rawMap.Width, rawMap.Height);
                atlasTexture.Linear();
                atlasTexture.ClampToEdge();
                atlasTexture.GenerateMipMaps();
                GLTexture2D.Unbind();

                CharAtlas catlas = new CharAtlas
                {
                    info = atlasData,
                    atlas = atlasTexture
                };

                atlasCache[group] = catlas;

                group.font = null;
                f.Dispose();

                return catlas;
            }
        }

        protected static List<CharData> GetCharacters(ref CharMapGroup group)
        {
            List<CharData> characters = new List<CharData>();
            Debug.WriteLine("character group info: " + group.ToString());
            long ms = Environment.TickCount;
            using (Graphics g = Graphics.FromImage(fontHelper))
            {
                StringFormat format = StringFormat.GenericTypographic;
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                format.Trimming = StringTrimming.None;
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Near;

                for (int i = 0; i < UNICODE_BASE.Length; ++i)
                {
                    byte b = UNICODE_BASE[i];
                    char c = (char)b;
                    CharData data = new CharData
                    {
                        c = c
                    };
                    RenderCharacter(g, format, ref group, data);
                    characters.Add(data);
                }
            }
            Debug.WriteLine("ascii render set: " + (Environment.TickCount - ms));
            return characters;
        }

        protected static void RenderCharacter(Graphics ghelper, StringFormat format, ref CharMapGroup group, CharData data)
        {
            var size = ghelper.MeasureString(data.c + "", group.font, 0, format);
            data.size = size;
            data.lineHeight = group.font.Height;

            if (Math.Ceiling(size.Width) <= 0 || Math.Ceiling(size.Height) <= 0) return;

            Bitmap b = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(b))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawString(data.c + "", group.font, WhiteColor, new System.Drawing.PointF(0, 0), format);
            }
            data.render = b;
        }

        public static void Dispose()
        {
            var items = atlasCache.Values.ToList();
            for (int i = 0; i < items.Count; ++i)
            {
                items[i]?.atlas?.Dispose();
            }
            atlasCache.Clear();

            fontHelper?.Dispose();
            fontHelper = null;

            WhiteColor?.Dispose();
            WhiteColor = null;
        }
    }
}
