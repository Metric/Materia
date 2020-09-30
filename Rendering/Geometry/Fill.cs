using DelaunayTriangulator;
using Materia.Rendering.Imaging;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Spatial;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Materia.Rendering.Geometry
{
    public enum FillType
    {
        Solid,
        Gradient,
        Pattern
    }

    public enum FillGradientType
    {
        Linear,
        Radial,
        Reflected
    }

    public class FillGradientPosition
    {
        public Vector4 Color { get; set; }
        public float Position { get; set; }

        public FillGradientPosition(Vector4 c, float t)
        {
            Color = c;
            Position = t;
        }

        public FillGradientPosition Clone()
        {
            return new FillGradientPosition(Color, Position);
        }
    }

    public struct FillGradient
    {
        public Vector2 start;
        public Vector2 end;
        public FillGradientType type;

        public FillGradient(Vector2 s, Vector2 e, FillGradientType t)
        {
            start = s;
            end = e;
            type = t;
        }
    }

    public enum FillShape
    {
        Polygon,
        Rectangle,
        Ellipse
    }

    public class Fill : IRenderable, IQuadComparable
    {


        public string ID { get; protected set; } = Guid.NewGuid().ToString();

        public VertexPositionStorageType StorageType { get; protected set; } = VertexPositionStorageType.Large;

        public RenderType RenderType { get; set; } = RenderType.Polygon;

        protected List<Vector2> points;
        public List<Vector2> Points
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

        public List<int> Triangles { get; set; }

        public int Texture { get; set; }

        public List<Vector2> UV { get; protected set; }

        public FillType FillType { get; set; }

        public List<FillGradientPosition> Colors { get; set; }

        public Box2 Rect { get; protected set; }

        public Vector3 Position { get; set; }

        public FillGradient GradientSettings { get; set; }

        public int FillMode
        {
            get
            {
                switch(FillType)
                {
                    default:
                    case FillType.Solid:
                        return 0;
                    case FillType.Gradient:
                        if (GradientSettings.type == FillGradientType.Linear)
                        {
                            return 1;
                        }
                        else if(GradientSettings.type == FillGradientType.Radial)
                        {
                            return 2;
                        }
                        else
                        {
                            return 3;
                        }
                    case FillType.Pattern:
                        return 4;
                }
            }
        }

        public float TextureScale { get; set; } = 1;

        public FloatBitmap GradientMap { get; protected set; }

        public FillShape Shape { get; set; }

        public Fill() : this(new Vector4(0,0,0,1))
        {

        }

        public Fill(Vector4 color)
        {
            Points = new List<Vector2>();
            Triangles = new List<int>();
            Colors = new List<FillGradientPosition>();
            Colors.Add(new FillGradientPosition(color, 0));
            GradientMap = new FloatBitmap(256, 4);
            GradientSettings = new FillGradient(Vector2.Zero, Vector2.One, FillGradientType.Linear);
            CalculateGradient();
        }

        public Fill Clone()
        {
            Fill f = new Fill();
            f.Colors.Clear();
            f.Texture = Texture;
            f.Position = Position;
            f.TextureScale = TextureScale;
            f.FillType = FillType;
            f.Shape = Shape;

            for (int i = 0; i < Colors.Count; ++i)
            {
                f.Colors.Add(Colors[i].Clone());
            }
            f.GradientSettings = GradientSettings;
            f.CalculateGradient();
            return f;
        }

        public void AddPoint(StrokePoint p)
        {
            AddPoint(p.vertex.x, p.vertex.y);
        }

        public void AddPoint(float x, float y)
        {
            if (float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y))
            {
                return;
            }

            //ensure point is rounded to int
            Vector2 v = new Vector2((int)MathF.Round(x), (int)MathF.Round(y));
            points.Add(v);
        }

        public float[] Compact()
        {
            List<float> data = new List<float>();

            if (points == null || UV == null || points.Count != UV.Count || points.Count == 0 || UV.Count == 0)
            {
                return data.ToArray();
            }

            for (int i = 0; i < points.Count; ++i)
            {
                data.Add(points[i].X);
                data.Add(points[i].Y);
                data.Add(UV[i].X);
                data.Add(UV[i].Y);
            }

            return data.ToArray();
        }

        public void CalculateUV()
        {
            try
            {
                UV = Projection.Planar.XyToUv(points, new Vector2(Rect.Left, Rect.Top), new Vector2(Rect.Right, Rect.Bottom));
                if (UV == null)
                {
                    UV = new List<Vector2>();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void CalculateTriangles()
        {
            if (RenderType != RenderType.Polygon) return;

            if (points.Count < 3)
            {
                return;
            }

            Triangles.Clear();

            /*List<Poly2Tri.TriPoint> trps = new List<Poly2Tri.TriPoint>();
            Dictionary<Tuple<int, int>, byte> seen = new Dictionary<Tuple<int, int>, byte>();
            for (int i = 0; i < points.Count; ++i)
            {
                Tuple<int, int> key = new Tuple<int, int>((int)points[i].x, (int)points[i].y);

                //eliminate duplicate points
                //otheriwse Poly2Tri will fail
                if (!seen.ContainsKey(key))
                {
                    seen[key] = 1;
                    trps.Add(new Poly2Tri.TriPoint(points[i].x, points[i].y, i));
                }
            }

            Poly2Tri.Shape shp = new Poly2Tri.Shape(trps);
            List<Poly2Tri.Triangle> tris = new List<Poly2Tri.Triangle>();
            shp.Triangulate(tris);

            for(int i = 0; i < tris.Count; ++i)
            {
                Triangles.Add(tris[i].Points[0].id);
                Triangles.Add(tris[i].Points[1].id);
                Triangles.Add(tris[i].Points[2].id);
            }*/

            try
            {
                Triangulator t = new Triangulator();
                Triangles = t.Triangulate(Points, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            CalculateUV();
        }

        public void CalculateGradient()
        {
            Gradient.Fill(GradientMap, Colors);
        }

        public void UpdateBounds(ref Box2 b)
        {
            Rect = b;
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
                Vector2 point = points[0];

                Position = new Vector3((int)points[0].X, (int)points[0].Y, Position.Z);

                float left = (int)MathF.Round(point.X - 1);
                float right = (int)MathF.Round(point.X + 1);
                float top = (int)MathF.Round(point.Y - 1);
                float bottom = (int)MathF.Round(point.Y + 1);

                Rect = new Box2(left, top, right, bottom);
            }
            else
            {
                float xmin = float.PositiveInfinity;
                float ymin = float.PositiveInfinity;
                float xmax = float.NegativeInfinity;
                float ymax = float.NegativeInfinity;

                for (int i = 0; i < points.Count; ++i)
                {
                    if (points[i].X < xmin)
                    {
                        xmin = points[i].X;
                    }
                    if (points[i].X > xmax)
                    {
                        xmax = points[i].X;
                    }
                    if (points[i].Y < ymin)
                    {
                        ymin = points[i].Y;
                    }
                    if (points[i].Y > ymax)
                    {
                        ymax = points[i].Y;
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
    }
}
