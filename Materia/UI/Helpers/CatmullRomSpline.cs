using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Materia.UI.Helpers
{
    public class CatmullRomSpline
    {
        public static List<Vector2> GetSpline(List<Vector2> points, float totalPoints)
        {
            if(points.Count < 4)
            {
                return points;
            }

            List<Vector2> spline = new List<Vector2>();


            for(int i = 0; i < points.Count - 3; i++)
            {
                Vector2 p0 = points[i];
                Vector2 p1 = points[i + 1];
                Vector2 p2 = points[i + 2];
                Vector2 p3 = points[i + 3];


                float t0 = 0.0f;
                float t1 = GetT(t0, p0, p1);
                float t2 = GetT(t1, p1, p2);
                float t3 = GetT(t2, p2, p3);

                for (float t = t1; t < t2; t += ((t2 - t1) / totalPoints))
                {
                    Vector2 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
                    Vector2 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
                    Vector2 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

                    Vector2 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
                    Vector2 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;

                    Vector2 C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;

                    spline.Add(C);
                }
            }

            return spline;
        }

        static float GetT(float t, Vector2 p0, Vector2 p1)
        {
            float a = (float)Math.Pow((p1.X - p0.X), 2.0f) + (float)Math.Pow((p1.Y - p0.Y), 2.0f);
            float b = (float)Math.Pow(a, 0.5f);
            float c = (float)Math.Pow(b, 0.5f);

            return (c + t);
        }
    }
}
