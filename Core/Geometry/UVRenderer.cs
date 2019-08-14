using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;
using Materia.Buffers;
using Materia.GLInterfaces;
using RSMI.Containers;
using Materia.Shaders;

namespace Materia.Geometry
{
    public class UVRenderer : Geometry
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
                shader = Material.Material.GetShader("uv.glsl", "uv.glsl");
            }
        }

        public override void Draw()
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

        public override void Release()
        {
            renderer = null;
        }
    }
}
