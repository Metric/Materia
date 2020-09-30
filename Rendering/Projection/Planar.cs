using System;
using System.Collections.Generic;
using Materia.Rendering.Mathematics;
using System.Text;

namespace Materia.Rendering.Projection
{
    public class Planar
    {
        public static List<Vector2> XyToUv(List<Vector2> pts)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            for (int i = 0; i < pts.Count; ++i)
            {
                if (pts[i].X < minX)
                {
                    minX = pts[i].X;
                }
                if (pts[i].X > maxX)
                {
                    maxX = pts[i].X;
                }
                if (pts[i].Y < minY)
                {
                    minY = pts[i].Y;
                }
                if (pts[i].Y > maxY)
                {
                    maxY = pts[i].Y;
                }
            }

            return XyToUv(pts, new Vector2(minX, minY), new Vector2(maxY, maxY));
        }

        public static List<Vector2> XyToUv(List<Vector2> pts, Vector2 min, Vector2 max)
        {
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < pts.Count; ++i)
            {
                Vector2 p = pts[i];

                float u = (p.X - min.X) / (max.X - min.X);
                float v = (p.Y - min.Y) / (max.Y - min.Y);

                uvs.Add(new Vector2(u, v));
            }

            return uvs;
        }

        public static List<Vector2> XyToUv(List<Vector3> pts)
        {
            //calculate min max
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            for(int i = 0; i < pts.Count; ++i)
            {
                if (pts[i].X < minX)
                {
                    minX = pts[i].X;
                }
                if (pts[i].X > maxX)
                {
                    maxX = pts[i].X;
                }
                if (pts[i].Y < minY)
                {
                    minY = pts[i].Y;
                }
                if (pts[i].Y > maxY)
                {
                    maxY = pts[i].Y;
                }
            }

            return XyToUv(pts, new Vector2(minX, minY), new Vector2(maxY, maxY));
        }

        public static List<Vector2> XzToUv(List<Vector3> pts)
        {
            //calculate min max
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            for (int i = 0; i < pts.Count; ++i)
            {
                if (pts[i].X < minX)
                {
                    minX = pts[i].X;
                }
                if (pts[i].X > maxX)
                {
                    maxX = pts[i].X;
                }
                if (pts[i].Z < minY)
                {
                    minY = pts[i].Z;
                }
                if (pts[i].Z > maxY)
                {
                    maxY = pts[i].Z;
                }
            }

            return XzToUv(pts, new Vector2(minX, minY), new Vector2(maxY, maxY));
        }

        public static List<Vector2> ZyToUv(List<Vector3> pts)
        {
            //calculate min max
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            for (int i = 0; i < pts.Count; ++i)
            {
                if (pts[i].Z < minX)
                {
                    minX = pts[i].Z;
                }
                if (pts[i].Z > maxX)
                {
                    maxX = pts[i].Z;
                }
                if (pts[i].Y < minY)
                {
                    minY = pts[i].Y;
                }
                if (pts[i].Y > maxY)
                {
                    maxY = pts[i].Y;
                }
            }

            return ZyToUv(pts, new Vector2(minX, minY), new Vector2(maxY, maxY));
        }

        public static List<Vector2> ZyToUv(List<Vector3> pts, Vector2 min, Vector2 max)
        {
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < pts.Count; ++i)
            {
                Vector3 p = pts[i];

                float u = (p.Z - min.X) / (max.X - min.X);
                float v = (p.Y - min.Y) / (max.Y - min.Y);

                uvs.Add(new Vector2(u, v));
            }

            return uvs;
        }

        public static List<Vector2> XzToUv(List<Vector3> pts, Vector2 min, Vector2 max)
        {
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < pts.Count; ++i)
            {
                Vector3 p = pts[i];

                float u = (p.X - min.X) / (max.X - min.X);
                float v = (p.Z - min.Y) / (max.Y - min.Y);

                uvs.Add(new Vector2(u, v));
            }

            return uvs;
        }

        public static List<Vector2> XyToUv(List<Vector3> pts, Vector2 min, Vector2 max)
        {
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < pts.Count; ++i)
            {
                Vector3 p = pts[i];

                float u = (p.X - min.X) / (max.X - min.X);
                float v = (p.Y - min.Y) / (max.Y - min.Y);

                uvs.Add(new Vector2(u, v));
            }

            return uvs;
        }
    }
}
