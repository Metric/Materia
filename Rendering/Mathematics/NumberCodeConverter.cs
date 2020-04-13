using System.Threading;

namespace Materia.Rendering.Mathematics
{
    public static class NumberCodeConverter
    {
        public static string ToCodeString(this decimal d)
        {
            return d.ToString().Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator, "").Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
        }

        public static string ToCodeString(this double d)
        {
            return d.ToString().Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator, "").Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
        }

        public static string ToCodeString(this float d)
        {
            return d.ToString().Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator, "").Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
        }

        public static string ToCodeString(this int d)
        {
            return d.ToString().Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator, "");
        }

        public static string ToCodeString(this long d)
        {
            return d.ToString().Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator, "");
        }

        public static string ToCodeString(this short d)
        {
            return d.ToString().Replace(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator, "");
        }
    }
}
