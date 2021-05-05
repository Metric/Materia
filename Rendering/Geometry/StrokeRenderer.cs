using Materia.Rendering.Buffers;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;
using Materia.Rendering.Textures;
using Materia.Rendering.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Materia.Rendering.Geometry
{
    public class StrokeRenderer : IGeometry, IDisposeShared
    {
        public List<Stroke> Strokes { get; protected set; }

        protected GLArrayBuffer vbo;
        protected int pointCount = 0;

        protected static bool isSharedDisposed = false;
        protected static GLVertexArray sharedVao;

        public static GLVertexArray SharedVao
        {
            get
            {
                if (sharedVao == null && !isSharedDisposed)
                {
                    sharedVao = new GLVertexArray();
                }

                return sharedVao;
            }
        }

        public StrokeRenderer(Stroke s) : this(new List<Stroke>(new Stroke[] { s } ))
        {

        }

        public StrokeRenderer(List<Stroke> strokes)
        {
            GeometryCache.RegisterForDispose(this);

            Strokes = strokes;

            vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);

            if (Strokes != null && Strokes.Count > 0)
            {
                Update();
            }
        }

        public bool AppendStroke(Stroke s)
        {
            if (s.Points.Count + pointCount < int.MaxValue)
            {
                Strokes.Add(s);
                Update();
                return true;
            }

            return false;
        }

        public void Update()
        {
            if (vbo == null)
            {
                return;
            }

            try
            {
                pointCount = 0;

                if (Strokes.Count == 1)
                {
                    float[] sdata = Strokes[0].Compact();
                    if (Strokes[0].SmoothPointCount > 0)
                    {
                        pointCount = Strokes[0].SmoothPointCount;
                    }
                    else
                    {
                        pointCount = Strokes[0].Points.Count;
                    }
                    vbo?.Bind();
                    vbo?.SetData(sdata);
                    vbo?.Unbind();
                }
                else
                {
                    List<float> data = new List<float>();

                    for (int i = 0; i < Strokes.Count; ++i)
                    {
                        float[] sdata = Strokes[i].Compact();
                        if (Strokes[i].SmoothPointCount > 0)
                        {
                            pointCount += Strokes[i].SmoothPointCount;
                        }
                        else
                        {
                            pointCount += Strokes[i].Points.Count;
                        }
                        data.AddRange(sdata);
                    }

                    vbo?.Bind();
                    vbo?.SetData(data.ToArray());
                    vbo?.Unbind();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {
            vbo?.Dispose();
            vbo = null;
        }

        /// <summary>
        /// Disposes the shared resources. Call before exiting the application fully.
        /// To properly release GPU resources for Shared VertexArray
        /// </summary>
        public void DisposeShared()
        {
            isSharedDisposed = true;
            sharedVao?.Dispose();
            sharedVao = null;
        }

        public void Draw()
        {
            if (Strokes == null || Strokes.Count == 0 || vbo == null)
            {
                return;
            }

            if (pointCount > 0)
            {
                vbo?.Bind();
                IGL.Primary.VertexAttribPointer(0, 2, (int)VertexAttribPointerType.Float, false, 10 * sizeof(float), 0);
                IGL.Primary.VertexAttribPointer(1, 2, (int)VertexAttribPointerType.Float, false, 10 * sizeof(float), 2 * 4);
                IGL.Primary.VertexAttribPointer(2, 4, (int)VertexAttribPointerType.Float, false, 10 * sizeof(float), (2 * 4) + (2 * 4));
                IGL.Primary.VertexAttribPointer(3, 1, (int)VertexAttribPointerType.Float, false, 10 * sizeof(float), (2 * 4) + (2 * 4) + (4 * 4));
                IGL.Primary.VertexAttribPointer(4, 1, (int)VertexAttribPointerType.Float, false, 10 * sizeof(float), (2 * 4) + (2 * 4) + (4 * 4) + 4);
                IGL.Primary.EnableVertexAttribArray(0);
                IGL.Primary.EnableVertexAttribArray(1);
                IGL.Primary.EnableVertexAttribArray(2);
                IGL.Primary.EnableVertexAttribArray(3);
                IGL.Primary.EnableVertexAttribArray(4);

                IGL.Primary.DrawArrays((int)PrimitiveType.Points, 0, pointCount);
                vbo?.Unbind();
            }
        }
    }
}
