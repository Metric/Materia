using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.MathHelpers
{
    public struct Point
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int h1 = X.GetHashCode();
                return ((h1 << 5) + Y.GetHashCode()) ^ h1;
            }
        }

        public override bool Equals(object obj)
        {
            if(obj is Point p)
            {
                return p.X == X && p.Y == Y;
            }

            return false;
        }
    }
}
