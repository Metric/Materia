using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Materia.MathHelpers
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
