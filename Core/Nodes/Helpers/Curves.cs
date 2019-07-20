using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Materia.Imaging;
using Materia.MathHelpers;

namespace Materia.Nodes.Helpers
{
    public static class Curves
    {
        public static double[] SecondDerivative(params Point[] P)
        {
            int n = P.Length;

            // build the tridiagonal system 
            // (assume 0 boundary conditions: y2[0]=y2[-1]=0) 
            double[,] matrix = new double[n, 3];
            double[] result = new double[n];
            matrix[0, 1] = 1;
            for (int i = 1; i < n - 1; i++)
            {
                matrix[i, 0] = (double)(P[i].X - P[i - 1].X) / 6;
                matrix[i, 1] = (double)(P[i + 1].X - P[i - 1].X) / 3;
                matrix[i, 2] = (double)(P[i + 1].X - P[i].X) / 6;
                result[i] = (double)(P[i + 1].Y - P[i].Y) / (P[i + 1].X - P[i].X) - (double)(P[i].Y - P[i - 1].Y) / (P[i].X - P[i - 1].X);
            }
            matrix[n - 1, 1] = 1;

            // solving pass1 (up->down)
            for (int i = 1; i < n; i++)
            {
                double k = matrix[i, 0] / matrix[i - 1, 1];
                matrix[i, 1] -= k * matrix[i - 1, 2];
                matrix[i, 0] = 0;
                result[i] -= k * result[i - 1];
            }
            // solving pass2 (down->up)
            for (int i = n - 2; i >= 0; i--)
            {
                double k = matrix[i, 2] / matrix[i + 1, 1];
                matrix[i, 1] -= k * matrix[i + 1, 0];
                matrix[i, 2] = 0;
                result[i] -= k * result[i + 1];
            }

            // return second derivative value for each point P
            double[] y2 = new double[n];
            for (int i = 0; i < n; i++) y2[i] = result[i] / matrix[i, 1];
            return y2;
        }
    }

}
