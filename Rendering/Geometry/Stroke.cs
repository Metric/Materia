using Materia.Rendering.Extensions;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Spatial;
using Materia.Rendering.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Materia.Rendering.Geometry
{
    public enum StrokeType
    {
        Precise = 0,
        Smooth = 1
    }

    public enum StrokeState
    {
        None = 0,
        Simplified = 1
    }

    public enum VertexPositionStorageType
    {
        Tiny = 0,
        Small = 1,
        Large = 2
    }

    public class StrokePoint : IEquatable<StrokePoint>
    {
        public Vector2 vertex;
        public Vector2 scale;
        public Vector4 color;

        /// <summary>
        /// The rotation should be in degrees
        /// </summary>
        public float rotation;

        public bool EqualsPosition(StrokePoint p)
        {
            return p.vertex.Equals(vertex);
        }

        public bool Equals(StrokePoint p)
        {
            return p.vertex.Equals(vertex) && p.scale.Equals(p.scale) 
                && p.color.Equals(color) && p.rotation == rotation;
        }

        public static bool operator < (StrokePoint p1, StrokePoint p2)
        {
            Vector2 v1 = p1.vertex;
            Vector2 v2 = p2.vertex;

            if (v1.X == v2.X)
            {
                return v1.Y < v2.Y;
            }

            return v1.X < v2.X;
        }

        public static bool operator > (StrokePoint p1, StrokePoint p2)
        {
            Vector2 v1 = p1.vertex;
            Vector2 v2 = p2.vertex;

            if (v1.X == v2.X)
            {
                return v1.Y > v2.Y;
            }

            return v1.X > v2.X;
        }

        public StrokePoint Clone()
        {
            StrokePoint p = new StrokePoint();
            p.vertex = vertex;
            p.color = color;
            p.scale = scale;
            p.rotation = rotation;
            return p;
        }

        public StrokePoint AddDiff(StrokePoint src, float t)
        {
            StrokePoint p = Clone();

            p.vertex = vertex + (src.vertex - vertex) * t;
            p.color = Vector4.Lerp(color, src.color, t);
            p.scale = Vector2.Lerp(scale, src.scale, t);
            p.rotation = rotation.Lerp(src.rotation, t);

            return p;
        }
    }

    public class Stroke : IEquatable<Stroke>, IQuadComparable, IRenderable
    {
        protected List<StrokePoint> points;
        public List<StrokePoint> Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }

        public VertexPositionStorageType StorageType { get; protected set; } = VertexPositionStorageType.Large;

        public RenderType RenderType { get; protected set; } = RenderType.Stroke;

        public string ID { get; protected set; }

        public int Texture { get; set; }
        public Vector3 Position { get; set; }
        public float Size { get; set; }

        public StrokeType Type { get; set; } = StrokeType.Precise;

        public StrokeState State { get; set; } = StrokeState.None;

        public Box2 Rect { get; set; }

        public float Spacing { get; set; } = 0.25f;

        public byte Hardness { get; set; } = 255;

        public int SmoothPointCount { get; protected set; }

        protected List<StrokePoint> smoothed;

        protected float direction = float.PositiveInfinity;
        public Stroke() : this(0, 0, 255)
        {

        }

        public Stroke(int tex, int size, byte hardness = 255, StrokeType type = StrokeType.Precise)
        {
            ID = Guid.NewGuid().ToString();
            points = new List<StrokePoint>();
            Rect = new Box2(0, 0, 0, 0);
            Position = Vector3.Zero;
            Size = size;
            Type = type;
            Hardness = hardness;
            Texture = tex;
        }

        public Stroke Clone()
        {
            Stroke s = new Stroke(Texture, (int)Size, Hardness, Type);
            s.Position = Position;
            return s;
        }

        public Stroke(List<StrokePoint> pts, int tex, int size, byte hardness = 255, StrokeType type = StrokeType.Precise) : this(tex,size,hardness,type)
        {
            points = pts;
        }

        public void AddPoint(StrokePoint p)
        {
            //ensure no weirdness
            if (float.IsNaN(p.vertex.X) || float.IsNaN(p.vertex.Y) || float.IsInfinity(p.vertex.X) || float.IsInfinity(p.vertex.Y))
            {
                return;
            }

            //ensure point is rounded to int
            p.vertex = new Vector2((int)MathF.Round(p.vertex.X), (int)MathF.Round(p.vertex.Y));
            
            //ensure proper precision range
            p.scale = new Vector2(((byte)(p.scale.X * 255)) * Inverse.Byte, ((byte)(p.scale.Y * 255)) * Inverse.Byte);

            //for now before we support up to 32 bit support on colors
            //we need to ensure the precision range is proper for color as well
            p.color = new Vector4(((byte)(p.color.X * 255)) * Inverse.Byte, ((byte)(p.color.Y * 255)) * Inverse.Byte, ((byte)(p.color.Z * 255)) * Inverse.Byte, ((byte)(p.color.W * 255)) * Inverse.Byte);

            points.Add(p);
        }

        public bool Equals(Stroke s)
        {
            return s.Texture == Texture && s.Size == Size 
                && s.Type == Type && s.Rect.Equals(Rect)
                && s.points == points;
        }

        public void Optimize()
        {
            for (int i = 0; i < points.Count; ++i)
            {
                StrokePoint p = points[i];

                if (p.color.W <= 0.01f || Size * p.scale.LengthFast < 1)
                {
                    points.RemoveAt(i);
                    --i;
                }
            }
        }

        public void ClearSmooth()
        {
            smoothed = null;
        }

        public void Simplify(out List<StrokePoint> pts, float tolerance = 4)
        {
            pts = Catmull.Simplify(points, tolerance);
        }

        public void Simplify()
        {
            points = Catmull.Simplify(points, 1);
        }

        public void Snap(ref Vector2 v)
        {
            List<StrokePoint> segment = ClosestSegment(ref v);

            if (segment == null || segment.Count < 2)
            {
                return;
            }

            Vector2 p1 = segment[0].vertex;
            Vector2 p2 = segment[1].vertex;

            Vector2 heading = (p2 - p1);
            float magnitude = heading.LengthFast;
            heading.Normalize();
            Vector2 lhs = (v - p1);
            float dotP = Vector2.Dot(lhs, heading);
            dotP = MathF.Min(magnitude, MathF.Max(0, dotP));
            v.X = p1.X + heading.X * dotP;
            v.Y = p1.Y + heading.Y * dotP;
        }

        /// <summary>
        /// Snaps to the segment and clamps to it
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="v">The v.</param>
        public static void Snap(ref Vector2 p1, ref Vector2 p2, ref Vector2 v)
        {
            Vector2 heading = (p2 - p1);
            float magnitude = heading.LengthFast;
            heading.Normalize();
            Vector2 lhs = (v - p1);
            float dotP = Vector2.Dot(lhs, heading);
            dotP = MathF.Min(magnitude, MathF.Max(0, dotP));
            v.X = p1.X + heading.X * dotP;
            v.Y = p1.Y + heading.Y * dotP;
        }

        /// <summary>
        /// Snaps to ray. Does not clamp end points to segment
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="v">The v.</param>
        public static void SnapToRay(ref Vector2 p1, ref Vector2 p2, ref Vector2 v)
        {
            Vector2 heading = (p2 - p1);
            float magnitude = heading.LengthFast;
            heading.Normalize();
            Vector2 lhs = (v - p1);
            float dotP = Vector2.Dot(lhs, heading);
            v.X = p1.X + heading.X * dotP;
            v.Y = p1.Y + heading.Y * dotP;
        }

        public List<StrokePoint> ClosestSegment(ref Vector2 p)
        {
            if (points.Count <= 2)
            {
                return points;
            }

            List<StrokePoint> segment = new List<StrokePoint>();

            Vector2 sp = points[0].vertex;
            Vector2 ep = points[points.Count - 1].vertex;
            Vector2 mp = points[(int)Math.Round(points.Count * 0.5f)].vertex;

            float d1 = Vector2.DistanceSquared(ref p, ref sp);
            float d2 = Vector2.DistanceSquared(ref p, ref mp);
            float d3 = Vector2.DistanceSquared(ref p, ref ep);

            int start = 0;
            int end = points.Count - 1;

            int sIdx = 0;
            int mIdx = (int)Math.Round(points.Count * 0.5f);
            int eIdx = points.Count - 1;

            if (d1 < d2 && d1 < d3)
            {
                start = sIdx;
                end = mIdx;
            }
            else if (d2 < d1 && d2 < d3)
            {
                start = mIdx;
                
                if (d1 < d3)
                {
                    end = sIdx;
                }
                else
                {
                    end = eIdx;
                }
            }
            else if(d3 < d1 && d3 < d1)
            {
                start = eIdx;
                end = mIdx;
            }

            float segDist = float.PositiveInfinity;
            StrokePoint p1 = null;
            StrokePoint p2 = null;

            if (start < end)
            {
                for (int i = start; i < end; ++i)
                {
                    StrokePoint t = points[i];
                    StrokePoint t2 = points[i + 1];

                    Vector2 v1 = t.vertex;
                    Vector2 v2 = t2.vertex;

                    float f1 = Vector2.DistanceSquared(ref p, ref v1);
                    float f2 = Vector2.DistanceSquared(ref p, ref v2);

                    if (f1 + f2 < segDist)
                    {
                        segDist = f1 + f2;
                        p1 = t;
                        p2 = t2;
                    }
                    else
                    {
                        //we can break earlier since we know the first known closest
                        break;
                    }
                }
            }
            else
            {
                for (int i = start; i > end; --i)
                {
                    StrokePoint t = points[i];
                    StrokePoint t2 = points[i - 1];

                    Vector2 v1 = t.vertex;
                    Vector2 v2 = t2.vertex;

                    float f1 = Vector2.DistanceSquared(ref p, ref v1);
                    float f2 = Vector2.DistanceSquared(ref p, ref v2);

                    if (f1 + f2 < segDist)
                    {
                        segDist = f1 + f2;
                        p1 = t2;
                        p2 = t;
                    }
                    else
                    {
                        //we can break earlier since we know the first known closest
                        break;
                    }
                }
            }

            if (p1 == null || p2 == null)
            {
                return null;
            }

            segment.Add(p1);
            segment.Add(p2);

            return segment;
        }

        public bool Intersects(Stroke s)
        {
            return Rect.Intersects(s.Rect);
        }

        public bool Intersects(Vector2 v)
        {
            return Rect.Contains(v);
        }

        public bool Intersects(StrokePoint p)
        {
            return Rect.Contains(p.vertex);
        }

        public List<StrokePoint> GetPointsInPolygon(List<StrokePoint> poly)
        {
            //assumes we are already intersecting in this method
            List<StrokePoint> pts = new List<StrokePoint>();

            for (int i = 0; i < points.Count; ++i)
            {
                if (PointInPolygon(poly, points[i]))
                {
                    pts.Add(points[i]);
                }
            }

            return pts;
        }

        public List<StrokePoint> GetPointsInStroke(Stroke s)
        {
            //assumes we are already intersecting in this method

            List<StrokePoint> pts = new List<StrokePoint>();

            for(int i = 0; i < points.Count; ++i)
            {
                if (s.IsInPolygon(points[i]))
                {
                    pts.Add(points[i]);
                }
            }

            return pts;
        }

        /// <summary>
        /// Determines whether [is in polygon] [the specified p].
        /// Assumes this stroke is complete in some fashion and forms a polygon of some sort
        /// with its points
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns>
        ///   <c>true</c> if [is in polygon] [the specified p]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInPolygon(StrokePoint p)
        {
            Vector2 v = p.vertex;
            return IsInPolygon(ref v);
        }

        /// <summary>
        /// Determines whether [is in polygon] [the specified p].
        /// Assumes this stroke is complete in some fashion and forms a polygon of some sort
        /// with its points
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns>
        ///   <c>true</c> if [is in polygon] [the specified p]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInPolygon(ref Vector2 p)
        {
            return PointInPolygon(points, ref p);
        }

        public float Direction
        {
            get
            {
                if (direction == float.PositiveInfinity)
                {
                    if (points.Count < 2) return 0;
                    float area = 0;
                    for (int i = 0, j = 1; i < points.Count - 1; ++i, ++j)
                    {
                        j %= points.Count;
                        Vector2 v1 = points[i].vertex;
                        Vector2 v2 = points[j].vertex;
                        area += (v2.x - v1.x) * (v2.y + v1.y);
                    }
                    direction = MathF.Sign(area * 0.5f);
                }

                return direction;
            }
        }

        public bool Intersection(Stroke o, out Vector2 it)
        {
            //we are ourself thus ignore
            if (o == this)
            {
                it = Vector2.Zero;
                return false;
            }

            for (int i = 0; i < points.Count - 1; ++i)
            {
                StrokePoint p = points[i];
                StrokePoint p2 = points[i + 1];

                Vector2 rp = p.vertex;
                List<StrokePoint> segment = o.ClosestSegment(ref rp);

                if(segment == null || segment.Count < 2)
                {
                    continue;
                }

                Vector2 result;
                if (Intersection(p.vertex, p2.vertex, segment[0].vertex, segment[1].vertex, out result))
                {
                    it = result;
                    return true;
                }
            }

            it = Vector2.Zero;
            return false;
        }

        public static bool Intersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 it)
        {
            float a1 = b.Y - a.Y;
            float b1 = a.X - b.X;
            float c1 = a1 * (a.X) + b1 * (a.Y);

            float a2 = d.Y - c.Y;
            float b2 = c.X - d.X;
            float c2 = a2 * (c.X) + b2 * (c.Y);

            float det = a1 * b2 - a2 * b1;
            if (det == 0)
            {
                it = Vector2.Zero;
                return false;
            }

            float inverseDet = 1.0f / det;
            float x = (b2 * c1 - b1 * c2) * inverseDet;
            float y = (a1 * c2 - a2 * c1) * inverseDet;

            it = new Vector2(x, y);
            return true;
        }

        public static bool PointInPolygon(List<StrokePoint> pts, StrokePoint p)
        {
            Vector2 v = p.vertex;
            return PointInPolygon(pts, ref v);
        }

        public static bool PointInPolygon(List<StrokePoint> pts, ref Vector2 p)
        {
            //return false as we are just a segment
            if (pts.Count <= 2)
            {
                return false;
            }

            float y = p.Y;
            float x = p.X;

            int i = 0, j = pts.Count - 1;
            bool odd = false;

            for (i = 0; i < pts.Count; ++i)
            {
                Vector2 p2 = pts[i].vertex;
                Vector2 p3 = pts[j].vertex;

                if (((p2.Y < y && p3.Y >= y) || (p3.Y < y && p2.Y >= y))
                    && (p2.X <= x || p3.X <= x))
                {
                    odd ^= (p2.X + (y - p2.Y) / (p3.Y - p2.Y) * (p3.X - p2.X) < x);
                }
                j = i;
            }

            return odd;
        }

        public static bool PointInPolygon(List<Vector2> pts, ref Vector2 p)
        {
            if (pts.Count <= 2)
            {
                return false;
            }

            float y = p.Y;
            float x = p.X;

            int i = 0, j = pts.Count - 1;
            bool odd = false;

            for (i = 0; i < pts.Count; ++i)
            {
                Vector2 p2 = pts[i];
                Vector2 p3 = pts[j];

                if (((p2.Y < y && p3.Y >= y) || (p3.Y < y && p2.Y >= y))
                    && (p2.X <= x || p3.X <= x))
                {
                    odd ^= (p2.X + (y - p2.Y) / (p3.Y - p2.Y) * (p3.X - p2.X) < x);
                }
                j = i;
            }

            return odd;
        }

        public static List<StrokePoint> LineTo(StrokePoint p, StrokePoint p2, int stepSize = 1)
        {
            List<StrokePoint> line = new List<StrokePoint>();

            float maxDist = 1.0f / Vector2.Distance(p.vertex, p2.vertex);

            int x0 = (int)p.vertex.X;
            int x1 = (int)p2.vertex.X;
            int y0 = (int)p.vertex.Y;
            int y1 = (int)p2.vertex.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int err = dx - dy;
            int sx = x0 < x1 ? stepSize : -stepSize;
            int sy = y0 < y1 ? stepSize : -stepSize;

            int e2 = err;

            StrokePoint prev = p;

            while (true)
            {
                if ((x0 == x1 && y0 == y1) || (Math.Abs(x1 - x0) <= stepSize && Math.Abs(y1 - y0) <= stepSize))
                {
                    break;
                }

                e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                    StrokePoint next = prev.Clone();
                    Vector2 v = next.vertex;
                    v.X = x0;
                    v.Y = y0;
                    
                    next.vertex = v;

                    float t = Vector2.Distance(v, p2.vertex) * maxDist;
                    next.color = Vector4.Lerp(next.color, p2.color, t);
                    next.scale = Vector2.Lerp(next.scale, p2.scale, t);
                    next.rotation = MathF.Round((1.0f - t) * next.rotation + t * p2.rotation);
                    line.Add(next);
                    prev = next;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;

                    StrokePoint next = prev.Clone();
                    Vector2 v = next.vertex;

                    v.X = x0;
                    v.Y = y0;

                    next.vertex = v;

                    float t = Vector2.Distance(v, p2.vertex) * maxDist;
                    next.color = Vector4.Lerp(next.color, p2.color, t);
                    next.scale = Vector2.Lerp(next.scale, p2.scale, t);
                    next.rotation = MathF.Round((1.0f - t) * next.rotation + t * p2.rotation);
                    line.Add(next);
                    prev = next;
                }
            }

            return line;
        }

        public bool Contains(StrokePoint p)
        {
            return points.Contains(p);
        }

        public void UpdateBounds()
        {
            if (points.Count == 0)
            {
                Position = new Vector3(0, 0, Position.Z);
                Rect = new Box2();
            }
            else if (points.Count == 1)
            {
                Vector2 point = points[0].vertex;

                Position = new Vector3((int)points[0].vertex.X, (int)points[0].vertex.Y, Position.Z);

                float left = (int)MathF.Round(point.X - 1);
                float right = (int)MathF.Round(point.X + 1);
                float top = (int)MathF.Round(point.Y - 1);
                float bottom = (int)MathF.Round(point.Y + 1);

                Rect = new Box2(left, top, right, bottom);
            }
            else {
                float xmin = float.PositiveInfinity;
                float ymin = float.PositiveInfinity;
                float xmax = float.NegativeInfinity;
                float ymax = float.NegativeInfinity;

                for (int i = 0; i < points.Count; ++i)
                {
                    if (points[i].vertex.X < xmin)
                    {
                        xmin = points[i].vertex.X;
                    }
                    if (points[i].vertex.X > xmax)
                    {
                        xmax = points[i].vertex.X;
                    }
                    if (points[i].vertex.Y < ymin)
                    {
                        ymin = points[i].vertex.Y;
                    }
                    if (points[i].vertex.Y > ymax)
                    {
                        ymax = points[i].vertex.Y;
                    }
                }

                float centerX = 0;
                float centerY = 0;

                float left = 0;
                float right = 0;
                float top = 0;
                float bottom = 0;

                if (xmin == xmax)
                {
                    left = (int)MathF.Round(xmin - 1);
                    right = (int)MathF.Round(xmin + 1);
                    centerX = (int)MathF.Round(xmin);
                }
                else
                {
                    left = (int)MathF.Round(xmin);
                    right = (int)MathF.Round(xmax);
                    centerX = (int)MathF.Round((xmin + xmax) * 0.5f);
                }

                if (ymin == ymax)
                {
                    top = (int)MathF.Round(ymin - 1);
                    bottom = (int)MathF.Round(ymin + 1);
                    centerY = (int)MathF.Round(ymin);
                }
                else
                {
                    top = (int)MathF.Round(ymin);
                    bottom = (int)MathF.Round(ymax);
                    centerY = (int)MathF.Round((ymin + ymax) * 0.5f);
                }

                Position = new Vector3(centerX, centerY, Position.Z);
                Rect = new Box2(left, top, right, bottom);
            }

            if (Rect.Left <= sbyte.MaxValue && Rect.Left >= sbyte.MinValue
                && Rect.Top <= sbyte.MaxValue && Rect.Top >= sbyte.MinValue
                && Rect.Right <= sbyte.MaxValue && Rect.Right >= sbyte.MinValue
                && Rect.Bottom <= sbyte.MaxValue && Rect.Bottom >= sbyte.MinValue)
            {
                StorageType = VertexPositionStorageType.Tiny;
            }
            else if (Rect.Left <= short.MaxValue && Rect.Left >= short.MinValue
                && Rect.Top <= short.MaxValue && Rect.Top >= short.MinValue
                && Rect.Right <= short.MaxValue && Rect.Right >= short.MinValue
                && Rect.Bottom <= short.MaxValue && Rect.Bottom >= short.MinValue)
            {
                StorageType = VertexPositionStorageType.Small;
            }
            else
            {
                StorageType = VertexPositionStorageType.Large;
            }
        }

        public float[] Compact()
        {
            List<float> data = new List<float>();

            if (Points == null)
            {
                return data.ToArray();
            }

            if (Type == StrokeType.Smooth && State == StrokeState.Simplified)
            {
                if (smoothed == null)
                {
                    smoothed = Catmull.Spline(Points, Size * Spacing);
                }

                SmoothPointCount = smoothed.Count;

                for (int i = 0; i < smoothed.Count; ++i)
                {
                    StrokePoint p = smoothed[i];
                    data.Add(p.vertex.X);
                    data.Add(p.vertex.Y);
                    data.Add(p.scale.X);
                    data.Add(p.scale.Y);
                    data.Add(p.color.X);
                    data.Add(p.color.Y);
                    data.Add(p.color.Z);
                    data.Add(p.color.W);
                    data.Add(p.rotation * MathHelper.Deg2Rad);
                    data.Add(Size);
                }

                return data.ToArray();
            }
            else
            {
                for (int i = 0; i < Points.Count; ++i)
                {
                    StrokePoint p = Points[i];
                    data.Add(p.vertex.X);
                    data.Add(p.vertex.Y);
                    data.Add(p.scale.X);
                    data.Add(p.scale.Y);
                    data.Add(p.color.X);
                    data.Add(p.color.Y);
                    data.Add(p.color.Z);
                    data.Add(p.color.W);
                    data.Add(p.rotation * MathHelper.Deg2Rad);
                    data.Add(Size);
                }

                return data.ToArray();
            }
        }
    }
}
