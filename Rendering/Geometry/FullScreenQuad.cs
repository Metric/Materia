using System;
using Materia.Rendering.Buffers;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Utils;

namespace Materia.Rendering.Geometry
{
    public class FullScreenQuad : IGeometry, IDisposeShared
    {
        static float[] buffer =
        {
            1,1,0,  1,1,
            -1,1,0, 0,1,
            1,-1,0,  1,0,
            -1,-1,0, 0,0
        };

        static readonly Vector4 DEFAULT_UV = new Vector4(0, 0, 1, 1);

        static uint[] indices =
        {
            0, 1, 2, 2, 1, 3
        };

        protected static bool isSharedDisposed = false;
        protected static GLVertexArray sharedVao;
        /// <summary>
        /// Gets the shared vao. Make sure the Stroke is set before calling this.
        /// </summary>
        /// <value>
        /// The shared vao.
        /// </value>
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

        protected GLElementBuffer ebo;
        protected GLArrayBuffer vbo;

        public FullScreenQuad()
        {
            GeometryCache.RegisterForDispose(this);
            ebo = new GLElementBuffer(BufferUsageHint.StaticDraw);
            vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);
            Setup();
        }

        public void DefaultUV()
        {
            Vector4 uv = DEFAULT_UV;
            SetUV(ref uv);
        }

        public void SetUV(ref Vector4 uv)
        {
            buffer[3] = uv.Z;
            buffer[4] = uv.W;

            buffer[8] = uv.X;
            buffer[9] = uv.W;

            buffer[13] = uv.Z;
            buffer[14] = uv.Y;

            buffer[18] = uv.X;
            buffer[19] = uv.Y;

            vbo?.Bind();
            vbo?.SetData(buffer);
            vbo?.Unbind();
        }

        public void Update()
        {
            //do nothing here
        }

        void Setup()
        {
            vbo?.Bind();
            vbo?.SetData(buffer);
            ebo?.Bind();
            ebo?.SetData(indices);
            vbo?.Unbind();
            ebo?.Unbind();
        }

        public void Draw()
        {
            vbo?.Bind();
            ebo?.Bind();

            IGL.Primary.VertexAttribPointer(0, 3, (int)VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            IGL.Primary.VertexAttribPointer(1, 2, (int)VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * 4);
            IGL.Primary.EnableVertexAttribArray(0);
            IGL.Primary.EnableVertexAttribArray(1);

            IGL.Primary.DrawElements((int)BeginMode.Triangles, 6, (int)DrawElementsType.UnsignedInt, 0);

            vbo?.Unbind();
            ebo?.Unbind();
            
        }

        public void DisposeShared()
        {
            isSharedDisposed = true;
            sharedVao?.Dispose();
            sharedVao = null;
        }

        public void Dispose()
        {
            ebo?.Dispose();
            ebo = null;
            vbo?.Dispose();
            vbo = null;
        }
    }
}
