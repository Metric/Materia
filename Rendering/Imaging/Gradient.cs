using Materia.Rendering.Imaging;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using Materia.Rendering.Geometry;
using System.Text;

namespace Materia.Rendering.Imaging
{
    public class Gradient
    {
        public static void Fill(GLBitmap dst, List<FillGradientPosition> positions)
        {
            if (positions.Count == 0) return;
            else if(positions.Count == 1)
            {
                GLPixel px = GLPixel.FromRGBA(positions[0].Color.X, positions[0].Color.Y, positions[0].Color.Z, positions[0].Color.W);
                dst.Clear(px);
                return;
            }

            positions.Sort((a, b) =>
            {
                if (a.Position < b.Position)
                {
                    return -1;
                }
                else if(a.Position > b.Position)
                {
                    return 1;
                }

                return 0;
            });

            List<float> pos = new List<float>();
            List<Vector4> cols = new List<Vector4>();

            for (int i = 0; i < positions.Count; ++i)
            {
                pos.Add(positions[i].Position);
                cols.Add(positions[i].Color);
            }

            if (pos[0] > 0)
            {
                pos.Insert(0, 0);
                Vector4 c = cols[0];
                cols.Insert(0, c);
            }

            if (pos[pos.Count - 1] < 1)
            {
                pos.Add(1);
                Vector4 c = cols[cols.Count - 1];
                cols.Add(c);
            }

            for (int i = 0; i < pos.Count - 1; ++i)
            {
                float p1 = pos[i];
                float p2 = pos[i + 1];

                Vector4 c1 = cols[i];
                Vector4 c2 = cols[i + 1];

                LabColor.ConvertToLAB(ref c1);
                LabColor.ConvertToLAB(ref c2);

                int imin = (int)(p1 * dst.Width);
                int imax = (int)(p2 * dst.Width);

                for (int x = imin; x < imax; ++x)
                {
                    //minus 1 on imax otherwise we won't reach 1 for t
                    float t = (float)(x - imin) / (float)(imax - 1 - imin);

                    Vector4 n = Vector4.Lerp(c1, c2, t);

                    LabColor.ConvertLABToRGB(ref n);

                    GLPixel pix = GLPixel.FromRGBA(n.X, n.Y, n.Z, n.W);

                    for (int y = 0; y < dst.Height; ++y)
                    {
                        dst.SetPixel(x, y, ref pix);
                    }
                }
            }
        }

        public static void Fill(GLBitmap dst, float[] positions, MVector[] colors)
        {
            if (positions.Length != colors.Length)
            {
                return;
            }

            //sort from least to greater
            Array.Sort(positions);

            List<float> pos = new List<float>(positions);
            List<MVector> cols = new List<MVector>(colors);

            if (positions[0] > 0)
            {
                pos.Insert(0, 0);
                MVector c = colors[0];
                cols.Insert(0, c);
            }

            if (positions[positions.Length - 1] < 1)
            {
                pos.Add(1);
                MVector c = colors[colors.Length - 1];
                cols.Add(c);
            }

            for (int i = 0; i < pos.Count - 1; ++i)
            {
                float p1 = pos[i];
                float p2 = pos[i + 1];

                Vector4 c1 = cols[i].ToVector4();
                Vector4 c2 = cols[i + 1].ToVector4();

                LabColor.ConvertToLAB(ref c1);
                LabColor.ConvertToLAB(ref c2);

                int imin = (int)(p1 * dst.Width);
                int imax = (int)(p2 * dst.Width);

                for (int x = imin; x < imax; ++x)
                {
                    //minus 1 on imax otherwise we won't reach 1 for t
                    float t = (float)(x - imin) / (float)(imax - 1 - imin);

                    Vector4 n = Vector4.Lerp(c1, c2, t);

                    LabColor.ConvertLABToRGB(ref n);

                    GLPixel pix = GLPixel.FromRGBA(n.X, n.Y, n.Z, n.W);

                    for (int y = 0; y < dst.Height; ++y)
                    {
                        dst.SetPixel(x, y, ref pix);
                    }
                }
            }
        }

        public static void Fill(GLBitmap dst, float[] positions, Vector4[] colors)
        {
            if (positions.Length != colors.Length)
            {
                return;
            }

            //sort from least to greater
            Array.Sort(positions);

            List<float> pos = new List<float>(positions);
            List<Vector4> cols = new List<Vector4>(colors);

            if (positions[0] > 0)
            {
                pos.Insert(0, 0);
                Vector4 c = colors[0];
                cols.Insert(0, c);
            }

            if (positions[positions.Length - 1] < 1)
            {
                pos.Add(1);
                Vector4 c = colors[colors.Length - 1];
                cols.Add(c);
            }

            for (int i = 0; i < pos.Count - 1; ++i)
            {
                float p1 = pos[i];
                float p2 = pos[i + 1];

                Vector4 c1 = cols[i];
                Vector4 c2 = cols[i + 1];

                LabColor.ConvertToLAB(ref c1);
                LabColor.ConvertToLAB(ref c2);

                int imin = (int)(p1 * dst.Width);
                int imax = (int)(p2 * dst.Width);

                for (int x = imin; x < imax; ++x)
                {
                    //minus 1 on imax otherwise we won't reach 1 for t
                    float t = (float)(x - imin) / (float)(imax - 1 - imin);

                    Vector4 n = Vector4.Lerp(c1, c2, t);

                    LabColor.ConvertLABToRGB(ref n);

                    GLPixel pix = GLPixel.FromRGBA(n.X, n.Y, n.Z, n.W);

                    for (int y = 0; y < dst.Height; ++y)
                    {
                        dst.SetPixel(x, y, ref pix);
                    }
                }
            }
        }
    }
}
