using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;

namespace Materia.Rendering.Imaging.Processing
{
    public class DistanceProcessor : ImageProcessor
    {
        public IGLProgram Shader { get; set; }
        public IGLProgram PreShader { get; set; }

        public float Distance { get; set; }
        public bool SourceOnly { get; set; }

        public DistanceProcessor() : base()
        {

        }

        public void Process(int width, int height, GLTexture2D tex, GLTexture2D source, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (Shader != null && PreShader != null)
            {
                GLTexture2D temp = new GLTexture2D(output.InternalFormat);
                temp.Bind();
                temp.SetData(IntPtr.Zero, PixelFormat.Rgba, width, height);
                temp.Linear();
                temp.Repeat();
                GLTexture2D.Unbind();
                GLTexture2D temp2 = new GLTexture2D(output.InternalFormat);
                temp2.Bind();
                temp2.SetData(IntPtr.Zero, PixelFormat.Rgba, width, height);
                temp2.Linear();
                temp2.Repeat();
                GLTexture2D.Unbind();
                GLTexture2D temp3 = new GLTexture2D(PixelInternalFormat.Rgba32f);
                temp3.Bind();
                temp3.SetData(IntPtr.Zero, PixelFormat.Rgba, width + 1, height + 1);
                temp3.Nearest();
                temp3.ClampToEdge();
                GLTexture2D.Unbind();

                ResizeViewTo(tex, temp, tex.Width, tex.Height, width, height);

                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                if (source != null)
                {
                    ResizeViewTo(source, temp2, source.Width, source.Height, width, height);
                }

                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));


                tex = temp;
                PreShader.Use();
                output.Bind();
                output.BindAsImage(0, true, true);
                tex.Bind();
                tex.BindAsImage(1, true, true);
                temp2.Bind();
                temp2.BindAsImage(2, true, true);
                temp3.Bind();
                temp3.BindAsImage(3, true, true);

                PreShader.SetUniform("width", (float)width);
                PreShader.SetUniform("height", (float)height);

                PreShader.SetUniform("sourceOnly", SourceOnly);
                PreShader.SetUniform("maxDistance", Distance);

                //stage 1 convert temp input range 
                //into proper format
                //for dt
                PreShader.SetUniform("stage", (int)0);
                IGL.Primary.DispatchCompute(width / 8, height / 8, 1);

                Shader.Use();
                output.Bind();
                output.BindAsImage(0, true, true);
                tex.Bind();
                tex.BindAsImage(1, true, true);
                temp2.Bind();
                temp2.BindAsImage(2, true, true);
                temp3.Bind();
                temp3.BindAsImage(3, true, true);

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
                output.Bind();
                output.BindAsImage(0, true, true);
                tex.Bind();
                tex.BindAsImage(1, true, true);
                temp2.Bind();
                temp2.BindAsImage(2, true, true);
                temp3.Bind();
                temp3.BindAsImage(3, true, true);

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
                temp2.Dispose();
                temp3.Dispose();
            }
        }
    }
}
