using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Geometry;
using Materia.Rendering.Buffers;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class MeshDepthProcessor : ImageProcessor
    {
        public MeshRenderer Mesh { get; set; }

        public MeshDepthProcessor() : base()
        {
        
        }

        public void Process(GLTexture2D output)
        {
            if (Mesh != null)
            {
                PrepareView(colorBuff);

                //enable depth test here again
                IGL.Primary.Enable((int)EnableCap.DepthTest);

                IGL.Primary.Viewport(0, 0, output.Width, output.Height);
                IGL.Primary.ClearColor(0, 0, 0, 0);
                IGL.Primary.Clear((int)ClearBufferMask.DepthBufferBit);
                IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit);

                //draw in depth
                Mesh.DrawAsDepth();

                //restore to default depth info
                IGL.Primary.Disable((int)EnableCap.DepthTest);
            }

            PrepareView(output);

            Identity();
            Bind();
            SetTextures(colorBuff);
            renderQuad?.Draw();
            Unbind();
        }
    }
}
