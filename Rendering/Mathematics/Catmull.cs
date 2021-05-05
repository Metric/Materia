using Assimp.Configs;
using Materia.Rendering.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Materia.Rendering.Mathematics
{
    public static class Catmull
    {

        /// <summary>
        /// Smoothes the specified point based on the previous points
        /// This is a differiental method as it looks
        /// only at the current incoming + the prev num points
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="next">The next.</param>
        /// <param name="pointsToAverage">The points to average.</param>
        /// <param name="round">if set to <c>true</c> [round].</param>
        public static void Smooth(List<StrokePoint> points, StrokePoint next, int iterations = 2, bool round = true)
        {
            if (points.Count < 2)
            {
                return;
            }

            float xTotal = 0;
            float yTotal = 0;
            int total = 0;

            for (int j = 0; j < iterations; ++j)
            {
                xTotal += next.vertex.X;
                yTotal += next.vertex.Y;
                ++total;

                for (int i = points.Count - 2; i < points.Count; ++i)
                {
                    xTotal += points[i].vertex.X;
                    yTotal += points[i].vertex.Y;

                    ++total;
                }
            }

            if (!round)
            {
                next.vertex = new Vector2(xTotal / total, yTotal / total);
            }
            else
            {
                next.vertex = new Vector2(MathF.Round(xTotal / total), MathF.Round(yTotal / total));
            }
        }

        /// <summary>
        /// Determines whether the point should be added to the list
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="next">The next.</param>
        /// <param name="radiusSquared">The radius squared.</param>
        /// <returns>
        ///   <c>true</c> if [is radial length] [the specified points]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRadialLength(StrokePoint prev, StrokePoint next, float radiusSquared) 
        {
            Vector2 cpv = next.vertex;
            Vector2 pv = prev.vertex;
            return Vector2.DistanceSquared(ref cpv, ref pv) >= radiusSquared;
        }

        /// <summary>
        /// Simplifies the point line based on a radial distance
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        static List<StrokePoint> SimplifyRadialDist(List<StrokePoint> points, float radiusSquared)
        {
            List<StrokePoint> newPoints = new List<StrokePoint>();
            StrokePoint prev = points[0];

            if (points.Count < 4)
            {
                return points;
            }

            newPoints.Add(prev);
            StrokePoint cpoint = null;

            for (int i = 1, len = points.Count; i < len; ++i)
            {
                cpoint = points[i];

                Vector2 cpv = cpoint.vertex;
                Vector2 pv = prev.vertex;
                if (Vector2.DistanceSquared(ref cpv, ref pv) > radiusSquared)
                {
                    newPoints.Add(cpoint);
                    prev = cpoint;
                }
            }

            if (prev != cpoint && cpoint != null)
            {
                newPoints.Add(cpoint);
            }

            return newPoints;
        }

        /// <summary>
        /// Smoothes the specified points based on averaging
        /// original point count is maintained
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns></returns>
        public static void Smooth(List<StrokePoint> points, int pointsToAverage = 2, bool round = true)
        {
            if (points.Count <= 2)
            {
                return;
            }

            int halfPoints = (int)MathF.Max(1, (pointsToAverage * 0.5f));

            for(int i = 1; i < points.Count - 1; ++i)
            {
                float xTotal = 0;
                float yTotal = 0;
                
                int total = 0;

                for (int k = -halfPoints; k <= halfPoints; ++k)
                {
                    if (k + i < 0 || k + i >= points.Count)
                    {
                        continue;
                    }

                    xTotal += points[i + k].vertex.X;
                    yTotal += points[i + k].vertex.Y;

                    ++total;
                }

                if (total <= 0)
                {
                    continue;
                }

                if (!round)
                {
                    points[i].vertex = new Vector2(xTotal / total, yTotal / total);
                }
                else
                {
                    points[i].vertex = new Vector2(MathF.Round(xTotal / total), MathF.Round(yTotal / total));
                }
            }
        }

        public static List<StrokePoint> Simplify(List<StrokePoint> points, float radius)
        {
            if (points.Count < 4)
            {
                return points;
            }

            float sqTolerance = radius * radius;
            return SimplifyRadialDist(points, sqTolerance);
        }

        public static List<StrokePoint> Spline(List<StrokePoint> points, float size = 2)
        {
            if (points.Count < 4)
            {
                return points;
            }

            List<StrokePoint> spline = new List<StrokePoint>();

            for (int i = 1; i < points.Count - 2; ++i)
            {
                StrokePoint point1 = points[i - 1]; //control point
                StrokePoint point2 = points[i];
                StrokePoint point3 = points[i + 1];
                StrokePoint point4 = points[i + 2]; //control point

                Vector2 p0 = points[i - 1].vertex;
                Vector2 p1 = points[i].vertex;
                Vector2 p2 = points[i + 1].vertex;

                float t0 = 0.0f;
                float t1 = GetT(t0, ref p0, ref p1);
                float t2 = GetT(t1, ref p1, ref p2);

                float inverseStep = 1.0f / (t2 - t1);
                float step = inverseStep * size;

                //ensure minimum of 0.0001f step size
                step = MathF.Max(step, 0.0001f);

                for (float t = t1; t < t2; t += step)
                {
                    float rt = (t - t1) * inverseStep;
                    spline.Add(PointOnCurve(point1, point2, point3, point4, t, rt));
                }
            }

            return spline;
        }

        static StrokePoint PointOnCurve(StrokePoint point1, StrokePoint point2, StrokePoint point3, StrokePoint point4, float t, float rt)
        {
            StrokePoint npoint = new StrokePoint();

            Vector2 p0 = point1.vertex;
            Vector2 p1 = point2.vertex;
            Vector2 p2 = point3.vertex;
            Vector2 p3 = point4.vertex;

            float t0 = 0.0f;
            float t1 = GetT(t0, ref p0, ref p1);
            float t2 = GetT(t1, ref p1, ref p2);
            float t3 = GetT(t2, ref p2, ref p3);

            npoint.color = Vector4.Lerp(point2.color, point3.color, rt);
            npoint.scale = Vector2.Lerp(point2.scale, point3.scale, rt);
            npoint.rotation = MathF.Round((1.0f - rt) * point2.rotation + rt * point3.rotation);

            Vector2 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
            Vector2 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
            Vector2 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

            Vector2 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
            Vector2 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;

            Vector2 C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;

            npoint.vertex = C;

            return npoint;
        }

        
        public static List<Vector2> Spline(List<Vector2> points, float size)
        {
            if (points.Count < 4)
            {
                return points;
            }

            List<Vector2> spline = new List<Vector2>();

            for (int i = 1; i < points.Count - 2; ++i)
            {
                Vector2 p0 = points[i - 1]; //control point
                Vector2 p1 = points[i];
                Vector2 p2 = points[i + 1];
                Vector2 p3 = points[i + 2]; //control point


                float t0 = 0.0f;
                float t1 = GetT(t0, ref p0, ref p1);
                float t2 = GetT(t1, ref p1, ref p2);
                float t3 = GetT(t2, ref p2, ref p3);

                float step = 1.0f / (t2 - t1) * size;

                //ensure minimum of 0.0001f step size
                step = MathF.Max(step, 0.0001f);

                for (float t = t1; t < t2; t += step)
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

        static float GetT(float t, ref Vector2 p0, ref Vector2 p1)
        {
            float a = (float)Math.Pow((p1.X - p0.X), 2.0f) + (float)Math.Pow((p1.Y - p0.Y), 2.0f);
            float b = (float)Math.Pow(a, 0.5f);
            float c = (float)Math.Pow(b, 0.5f);

            return (c + t);
        }
    }
}
