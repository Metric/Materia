using System;

namespace Materia.Rendering.Mathematics
{
    public struct PointD : IEquatable<PointD>
    {
        public double x;
        public double y;

        public PointD(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int h1 = x.GetHashCode();
                return ((h1 << 5) + y.GetHashCode()) ^ h1;
            }
        }

        public bool Equals(PointD d)
        {
            return d.x == x && d.y == y;
        }

        public override bool Equals(object obj)
        {
            if(obj is PointD p)
            {
                return p.x == x && p.y == y;
            }

            return false;
        }
    }
}
