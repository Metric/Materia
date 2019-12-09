using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Shaders;
using Materia.Math3D;
using Materia.GLInterfaces;
using Materia.Nodes;
using NLog;

namespace Materia.Imaging.GLProcessing
{
    public class PixelShaderProcessor : ImageProcessor
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();
        public IGLProgram Shader { get; set; }

        public PixelShaderProcessor()
        {
        }

        public void Process(FunctionGraph graph, int width, int height, GLTextuer2D tex, GLTextuer2D tex2, GLTextuer2D tex3, GLTextuer2D tex4, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (Shader != null)
            {
                Shader.Use();
                if (output != null)
                {
                    output.Bind();
                    output.BindAsImage(0, false, true);
                }

                Shader.SetUniform("Input0", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                if (tex != null)
                {
                    tex.Bind();
                }

                Shader.SetUniform("Input1", 2);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture2);
                if(tex2 != null)
                {
                    tex2.Bind();
                }

                Shader.SetUniform("Input2", 3);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture3);
                if(tex3 != null)
                {
                    tex3.Bind();
                }

                Shader.SetUniform("Input3", 4);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture4);
                if(tex4 != null)
                {
                    tex4.Bind();
                }

                graph.AssignUniforms();

                IGL.Primary.DispatchCompute(width / 8, height / 8, 1);

                GLTextuer2D.UnbindAsImage(0);
                GLTextuer2D.Unbind();
            }
        }
    }
}
