﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Materia.Rendering.Mathematics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Materia.Rendering.Imaging;

namespace Materia.Rendering.Fonts
{
    public class FontManager
    {
        public class CharData
        {
            public float fontSize;
            public Vector2 size;
            public float bearing;
            public RawBitmap texture;
            public FontStyle style;

            public CharData(float fSize, Vector2 s, float b, RawBitmap tex, FontStyle styl)
            {
                fontSize = fSize;
                size = s;
                bearing = b;
                texture = tex;
                style = styl;
            }
        }

        private static string[] fontList;
        private static SolidBrush WhiteColor = new SolidBrush(Color.White);
        private static Bitmap fontHelper = new Bitmap(1, 1);
        private static Dictionary<string, Dictionary<string, CharData>> cache = new Dictionary<string, Dictionary<string, CharData>>();

        public static string[] GetAvailableFonts()
        {
            if(fontList != null && fontList.Length > 0)
            {
                return fontList;
            }

            List<string> families = new List<string>();
            InstalledFontCollection installed = new InstalledFontCollection();
            foreach(FontFamily font in installed.Families)
            {
                families.Add(font.Name);
            }

            fontList = families.ToArray();
            return fontList;
        }

        public static Dictionary<string, CharData> Generate(string fontFamily, float fontSize, string text, FontStyle style)
        {
            string[] fonts = GetAvailableFonts();

            if (fonts.Length == 0) return new Dictionary<string, CharData>();
       
            if(Array.IndexOf(fonts, fontFamily) == -1)
            {
                fontFamily = fonts[0];
            }

            Dictionary<string, CharData> map = null;
            cache.TryGetValue(fontFamily, out map);

            if (map == null) map = new Dictionary<string, CharData>();

            if (text.Length == 0) return map;

            string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                using (Font f = new Font(fontFamily, fontSize, style, GraphicsUnit.Pixel))
                {
                    for (int i = 0; i < lines.Length; ++i)
                    {
                        string line = lines[i];
                        for (int j = 0; j < line.Length; ++j)
                        {
                            string ch = line.Substring(j, 1);
                            CharData chData = null;
                            if (map.TryGetValue(ch, out chData))
                            {
                                if (chData.fontSize != fontSize || chData.style != style)
                                {
                                    CreateCharacter(f, chData, fontFamily, fontSize, ch, style);
                                }
                            }
                            else
                            {
                                CharData nData = new CharData(fontSize, Vector2.Zero, 0, null, style);
                                CreateCharacter(f, nData, fontFamily, fontSize, ch, style);
                                map[ch] = nData;
                            }
                        }
                    }
                }

                cache[fontFamily] = map;
            }
            catch { }

            return map;
        }

        protected static void CreateCharacter(Font f, CharData data, string fontFamily, float fontSize, string ch, FontStyle style)
        {
            using (var ghelper = Graphics.FromImage(fontHelper))
            {
                StringFormat format = StringFormat.GenericTypographic;
                
                var size = ghelper.MeasureString(ch, f, 0, format);
                data.size = new Vector2(size.Width, size.Height);
                data.bearing = f.GetHeight();
                data.style = style;
                data.fontSize = fontSize;

                if (Math.Ceiling(size.Width) <= 0 || Math.Ceiling(size.Height) <= 0) return;

                using (Bitmap b = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(b))
                    {
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawString(ch, f, WhiteColor, new PointF(0, 0), format);
                    }

                    RawBitmap bit = RawBitmap.FromBitmap(b);
                    data.texture = bit;
                }
            }
        }

        public static void Release()
        {
            if(fontHelper != null)
            {
                fontHelper.Dispose();
                fontHelper = null;
            }

            if(WhiteColor != null)
            {
                WhiteColor.Dispose();
                WhiteColor = null;
            }

            if (cache != null)
            {
                cache.Clear();
                cache = null;
            }
        }
    }
}
