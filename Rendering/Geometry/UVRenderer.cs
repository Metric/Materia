using Materia.Rendering.Mathematics;
using Materia.Rendering.Buffers;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Shaders;

namespace Materia.Rendering.Geometry
{
    public class UVRenderer : IGeometry
    {
        public Matrix4 Model { get; set; }
        public Matrix4 Projection { get; set; }
        public Matrix4 View { get; set; }

        protected int indicesCount;
        protected MeshRenderer renderer;

        protected IGLProgram shader;

        public UVRenderer(MeshRenderer mesh)
        {
            if (mesh != null)
            {
                renderer = mesh;
                indicesCount = mesh.IndicesCount;
                shader = GLShaderCache.GetShader("uv.glsl", "uv.glsl");
            }
        }

        public void Update()
        {
            //do nothing
        }

        public void Draw()
        {
            if(shader != null && renderer != null)
            {
                shader.Use();
                Matrix4 view = View;
                Matrix4 proj = Projection;
                Matrix4 model = Model;
                shader.SetUniformMatrix4("modelMatrix", ref model);
                shader.SetUniformMatrix4("viewMatrix", ref view);
                shader.SetUniformMatrix4("projectionMatrix", ref proj);

                IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Line);
                renderer.Bind();
                IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);
                GLVertexArray.Unbind();

                IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);
            }
        }

        public void Dispose()
        {
            renderer = null;
        }
    }
}
