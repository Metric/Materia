namespace Materia.Rendering.Mathematics
{
    /// <summary>
    /// This is the bezier curve formula that is similar
    /// to photoshop curve editor
    /// </summary>
    public static class Curves
    {
        public static float[] SecondDerivative(params PointF[] P)
        {
            int n = P.Length;

            // build the tridiagonal system 
            // (assume 0 boundary conditions: y2[0]=y2[-1]=0) 
            float[,] matrix = new float[n, 3];
            float[] result = new float[n];
            matrix[0, 1] = 1;
            for (int i = 1; i < n - 1; ++i)
            {
                matrix[i, 0] = (P[i].x - P[i - 1].x) / 6;
                matrix[i, 1] = (P[i + 1].x - P[i - 1].x) / 3;
                matrix[i, 2] = (P[i + 1].x - P[i].x) / 6;
                result[i] = (P[i + 1].y - P[i].y) / (P[i + 1].x - P[i].x) - (P[i].y - P[i - 1].y) / (P[i].x - P[i - 1].x);
            }
            matrix[n - 1, 1] = 1;

            // solving pass1 (up->down)
            for (int i = 1; i < n; ++i)
            {
                float k = matrix[i, 0] / matrix[i - 1, 1];
                matrix[i, 1] -= k * matrix[i - 1, 2];
                matrix[i, 0] = 0;
                result[i] -= k * result[i - 1];
            }
            // solving pass2 (down->up)
            for (int i = n - 2; i >= 0; --i)
            {
                float k = matrix[i, 2] / matrix[i + 1, 1];
                matrix[i, 1] -= k * matrix[i + 1, 0];
                matrix[i, 2] = 0;
                result[i] -= k * result[i + 1];
            }

            // return second derivative value for each point P
            float[] y2 = new float[n];
            for (int i = 0; i < n; ++i) y2[i] = result[i] / matrix[i, 1];
            return y2;
        }

        public static PointF Point(PointF start, PointF control, PointF end, float t)
        {
            float x = (((1 - t) * (1 - t)) * start.x) + (2 * t * (1 - t) * control.x) + ((t * t) * end.x);
            float y = (((1 - t) * (1 - t)) * start.y) + (2 * t * (1 - t) * control.y) + ((t * t) * end.y);
            return new PointF(x, y);
        }

        public static double[] SecondDerivative(params PointD[] P)
        {
            int n = P.Length;

            // build the tridiagonal system 
            // (assume 0 boundary conditions: y2[0]=y2[-1]=0) 
            double[,] matrix = new double[n, 3];
            double[] result = new double[n];
            matrix[0, 1] = 1;
            for (int i = 1; i < n - 1; ++i)
            {
                matrix[i, 0] = (P[i].x - P[i - 1].x) / 6;
                matrix[i, 1] = (P[i + 1].x - P[i - 1].x) / 3;
                matrix[i, 2] = (P[i + 1].x - P[i].x) / 6;
                result[i] = (P[i + 1].y - P[i].y) / (P[i + 1].x - P[i].x) - (P[i].y - P[i - 1].y) / (P[i].x - P[i - 1].x);
            }
            matrix[n - 1, 1] = 1;

            // solving pass1 (up->down)
            for (int i = 1; i < n; ++i)
            {
                double k = matrix[i, 0] / matrix[i - 1, 1];
                matrix[i, 1] -= k * matrix[i - 1, 2];
                matrix[i, 0] = 0;
                result[i] -= k * result[i - 1];
            }
            // solving pass2 (down->up)
            for (int i = n - 2; i >= 0; --i)
            {
                double k = matrix[i, 2] / matrix[i + 1, 1];
                matrix[i, 1] -= k * matrix[i + 1, 0];
                matrix[i, 2] = 0;
                result[i] -= k * result[i + 1];
            }

            // return second derivative value for each point P
            double[] y2 = new double[n];
            for (int i = 0; i < n; ++i) y2[i] = result[i] / matrix[i, 1];
            return y2;
        }

        public static PointD Point(PointD start, PointD control, PointD end, float t)
        {
            double x = (((1 - t) * (1 - t)) * start.x) + (2 * t * (1 - t) * control.x) + ((t * t) * end.x);
            double y = (((1 - t) * (1 - t)) * start.y) + (2 * t * (1 - t) * control.y) + ((t * t) * end.y);
            return new PointD(x, y);
        }
    }
}
