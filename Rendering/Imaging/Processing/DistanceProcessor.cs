using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;

namespace Materia.Rendering.Imaging.Processing
{
    public class DistanceProcessor : ImageProcessor
    {
        public IGLProgram PreShader { get; set; }

        public float Distance { get; set; }
        public bool SourceOnly { get; set; }

        public DistanceProcessor() : base()
        {

        }

        public void Process(GLTexture2D input, GLTexture2D source)
        {
            int width = outputBuff.Width;
            int height = outputBuff.Height;

            if (Shader != null && PreShader != null)
            {
                GLTexture2D sourceTemp = source == null ? outputBuff.Copy() : source;

                GLTexture2D vz = new GLTexture2D(PixelInternalFormat.Rgba32f);
                vz.Bind();
                vz.SetData(IntPtr.Zero, PixelFormat.Rgba, width + 1, height + 1);
                vz.Nearest();
                vz.ClampToEdge();
                GLTexture2D.Unbind();

                PreShader.Use();

                //bind output
                outputBuff.Bind();
                outputBuff.BindAsImage(0, true, true);

                //bind input
                input.Bind();
                input.BindAsImage(1, true, true);

                //bind source
                sourceTemp.Bind();
                sourceTemp.BindAsImage(2, true, true);
                
                //bind vs
                vz.Bind();
                vz.BindAsImage(3, true, true);

                PreShader.SetUniform("width", (float)width);
                PreShader.SetUniform("height", (float)height);

                PreShader.SetUniform("sourceOnly", SourceOnly);
                PreShader.SetUniform("maxDistance", Distance);

                //stage 1 convert input range 
                //into proper format
                //for dt
                PreShader.SetUniform("stage", (int)0);
                IGL.Primary.DispatchCompute(width / 8, height / 8, 1);

                Shader.Use();

                //bind output
                outputBuff.Bind();
                outputBuff.BindAsImage(0, true, true);

                //bind input
                input.Bind();
                input.BindAsImage(1, true, true);

                //bind source
                sourceTemp.Bind();
                sourceTemp.BindAsImage(2, true, true);

                //bind vz
                vz.Bind();
                vz.BindAsImage(3, true, true);

                Shader.SetUniform("width", (float)width);
                Shader.SetUniform("height", (float)height);

                Shader.SetUniform("sourceOnly", SourceOnly);
                Shader.SetUniform("maxDistance", Distance);

                //stage 2 run column transform
                Shader.SetUniform("stage", (int)0);
                IGL.Primary.DispatchCompute(width / 8, 1, 1);
      
                //stage 3 run row transform
                Shader.SetUniform("stage", (int)1);
                IGL.Primary.DispatchCompute(height / 8, 1, 1);

                PreShader.Use();

                //bind output
                outputBuff.Bind();
                outputBuff.BindAsImage(0, true, true);

                //bind input
                input.Bind();
                input.BindAsImage(1, true, true);

                //bind source
                sourceTemp.Bind();
                sourceTemp.BindAsImage(2, true, true);

                //bind vz
                vz.Bind();
                vz.BindAsImage(3, true, true);

                PreShader.SetUniform("width", (float)width);
                PreShader.SetUniform("height", (float)height);

                PreShader.SetUniform("sourceOnly", SourceOnly);
                PreShader.SetUniform("maxDistance", Distance);

                //stage 4 finalize with sqrt() etc
                PreShader.SetUniform("stage", (int)1);
                IGL.Primary.DispatchCompute(width / 8, height / 8, 1);

                GLTexture2D.UnbindAsImage(0);
                GLTexture2D.UnbindAsImage(1);
                GLTexture2D.UnbindAsImage(2);
                GLTexture2D.UnbindAsImage(3);
                GLTexture2D.Unbind();

                temp.Dispose();
                if (sourceTemp != source)
                {
                    sourceTemp.Dispose();
                }
            }
        }
    }
}
