using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Rendering.Mathematics
{
    public struct PointF : IEquatable<PointF>
    {
        public float x;
        public float y;

        public PointF(float x, float y)
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

        public bool Equals(PointF d)
        {
            return d.x == x && d.y == y;
        }

        public override bool Equals(object obj)
        {
            if (obj is PointF p)
            {
                return p.x == x && p.y == y;
            }

            return false;
        }
    }
}
