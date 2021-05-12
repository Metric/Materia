using Materia.Rendering.Buffers;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Materia.Rendering.Geometry
{
    public class LineRenderer : IGeometry, IDisposeShared
    {
        public List<Line> Lines { get; protected set; } = new List<Line>();

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

        public LineRenderer() : this(new List<Line>())
        {
         
        }

        public LineRenderer(Line s) : this(new List<Line>(new Line[] { s }))
        {

        }

        public LineRenderer(List<Line> lines)
        {
            GeometryCache.RegisterForDispose(this);

            Lines = lines;

            vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);

            if (Lines != null && Lines.Count > 0)
            {
                Update();
            }
        }

        public bool AppendLine(Line s)
        {
            if (pointCount + 2 < int.MaxValue)
            {
                Lines.Add(s);
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
                pointCount = Lines.Count * 2;

                if (Lines.Count == 1)
                {
                    float[] sdata = Lines[0].Compact();

                    vbo?.Bind();
                    vbo?.SetData(sdata);
                    vbo?.Unbind();
                }
                else
                {
                    List<float> data = new List<float>();

                    for (int i = 0; i < Lines.Count; ++i)
                    {
                        float[] sdata = Lines[i].Compact();
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
            if (Lines == null || Lines.Count == 0 || vbo == null)
            {
                return;
            }

            vbo?.Bind();
            IGL.Primary.VertexAttribPointer(0, 3, (int)VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            IGL.Primary.VertexAttribPointer(1, 4, (int)VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * 4);
            IGL.Primary.EnableVertexAttribArray(0);
            IGL.Primary.EnableVertexAttribArray(1);
            IGL.Primary.DrawArrays((int)PrimitiveType.Lines, 0, Lines.Count);
            vbo?.Unbind();
        }
    }
}
