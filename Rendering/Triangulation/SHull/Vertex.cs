using System;
using System.Collections.Generic;
using System.Text;

/*
  copyright s-hull.org 2011
  released under the contributors beerware license

  contributors: Phil Atkin, Dr Sinclair.
*/
namespace DelaunayTriangulator
{
    public class Vertex
    {
        public float x, y;

        protected Vertex() { }

        public Vertex(float x, float y) 
        {
            this.x = x; this.y = y;
        }

        public float distance2To(Vertex other)
        {
            float dx = x - other.x;
            float dy = y - other.y;
            return dx * dx + dy * dy;
        }

        public float distanceTo(Vertex other)
        {
            return (float)Math.Sqrt(distance2To(other));
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }
    }

}
