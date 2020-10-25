using Materia.Rendering.Extensions;

namespace Materia.Rendering.Imaging
{
    public struct GLPixel
    {
        public float fr;
        public float fg;
        public float fb;
        public float fa;

        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public static void Premult(ref GLPixel p)
        {
            float a = p.fa;

            p.r = (byte)(p.r * a);
            p.g = (byte)(p.g * a);
            p.b = (byte)(p.b * a);
            p.fr *= a;
            p.fg *= a;
            p.fb *= a;
        }

        public static GLPixel Lerp(ref GLPixel from, ref GLPixel to, float t)
        {
            float fr = from.fr.Lerp(to.fr, t);
            float fg = from.fg.Lerp(to.fg, t);
            float fb = from.fb.Lerp(to.fb, t);
            float fa = from.fa.Lerp(to.fa, t);

            byte r = from.r.Lerp(to.r, t);
            byte g = from.g.Lerp(to.g, t);
            byte b = from.b.Lerp(to.b, t);
            byte a = from.a.Lerp(to.a, t);

            return new GLPixel() { r = r, g = g, b = b, a = a, fr = fr, fg = fg, fb = fb, fa = fa };
        }

        public static GLPixel FromRGBA(byte r, byte g, byte b, byte a)
        {
            float fr = r / 255.0f;
            float fg = g / 255.0f;
            float fb = b / 255.0f;
            float fa = a / 255.0f;

            return new GLPixel() { r = r, g = g, b = b, a = a, fr = fr, fg = fg, fb = fb, fa = fa };
        }

        public static GLPixel FromRGBA(float fr, float fg, float fb, float fa)
        {
            byte r = (byte)(fr * 255);
            byte g = (byte)(fg * 255);
            byte b = (byte)(fb * 255);
            byte a = (byte)(fa * 255);

            return new GLPixel() { r = r, g = g, b = b, a = a, fr = fr, fg = fg, fb = fb, fa = fa };
        }
    }
}
