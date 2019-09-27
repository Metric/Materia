using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using Materia.Geometry;
using Materia.GLInterfaces;
using Materia.Math3D;

namespace Materia.Rendering
{
    public class BloomPass : RenderStackItem
    {
        BlurProcessor blur;
        FullScreenQuad quad;
        IGLProgram shader;

        int width;
        int height;

        public float Intensity
        {
            get; set;
        }

        public BloomPass(int w, int h)
        {
            Intensity = 8;
            width = w;
            height = h;
            quad = new FullScreenQuad();
            blur = new BlurProcessor();
            shader = Material.Material.GetShader("image.glsl", "bloom.glsl");
        }

        public void Update(int w, int h)
        {
            width = w;
            height = h;
        }

        public override void Release()
        {
            if (blur != null)
            {
                blur.Release();
                blur = null;
            }

            if(quad != null)
            {
                quad.Release();
                quad = null;
            }
        }

        public override void Render(GLTextuer2D[] inputs, out GLTextuer2D[] outputs)
        {
            outputs = null;

            if (shader == null) return;

            if (inputs == null) return;

            if (inputs.Length < 2) return;

            blur.Intensity = Intensity;
            blur.Process(width, height, inputs[1], inputs[1]);
            blur.Complete();

            Vector2 tiling = new Vector2(1);

            shader.Use();
            shader.SetUniform("MainTex", 0);
            shader.SetUniform("Bloom", 1);
            shader.SetUniform2("tiling", ref tiling);

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            inputs[0].Bind();

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
            inputs[1].Bind();

            //ensure polygon is actually rendered instead of wireframe during this step
            IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);

            IGL.Primary.Disable((int)EnableCap.CullFace);

            quad.Draw();

            IGL.Primary.Enable((int)EnableCap.CullFace);

            GLTextuer2D.Unbind();
        }
    }
}
