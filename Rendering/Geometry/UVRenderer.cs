using Materia.Rendering.Mathematics;
using Materia.Rendering.Buffers;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Shaders;

namespace Materia.Rendering.Geometry
{
    public class UVRenderer : IGeometry
    { 
        protected MeshRenderer renderer;

        protected IGLProgram shader;

        public UVRenderer()
        {
            shader = GLShaderCache.GetShader("uv.glsl", "uv.glsl");
        }

        public void Set(MeshRenderer mesh)
        {
            renderer = mesh;
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

                IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Line);
                renderer?.DrawBasic();
                IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);
            }
        }

        public void Dispose()
        {
            renderer = null;
        }
    }
}
