using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSMI.Containers
{
    public class Quad
    {
        public int v0;
        public int v1;
        public int v2;
        public int v3;

        public int n0;
        public int n1;
        public int n2;
        public int n3;

        public int u0;
        public int u1;
        public int u2;
        public int u3;

        public Triangle[] ToTriangles()
        {
            Triangle[] tris = new Triangle[2];
            
            Triangle t1 = new Triangle();
            Triangle t2 = new Triangle();

            t1.v0 = v0;
            t1.v1 = v1;
            t1.v2 = v2;

            t1.n0 = n0;
            t1.n1 = n1;
            t1.n2 = n2;

            t1.u0 = u0;
            t1.u1 = u1;
            t1.u2 = u2;

            t2.v0 = v0;
            t2.v1 = v2;
            t2.v2 = v3;

            t2.n0 = n0;
            t2.n1 = n2;
            t2.n2 = n3;

            t2.u0 = u0;
            t2.u1 = u2;
            t2.u2 = u3;

            tris[0] = t1;
            tris[1] = t2;

            return tris;
        }
    }
}
