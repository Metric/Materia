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
            int width = Width;
            int height = Height;

            if (Shader != null && PreShader != null)
            {
                int sourceMode = source == null ? 2 : SourceOnly ? 1 : 0;

                GLTexture2D inputTemp = new GLTexture2D(PixelInternalFormat.Rgba32f);
                inputTemp.Bind();
                inputTemp.SetData(IntPtr.Zero, PixelFormat.Rgba, Width, Height);
                inputTemp.Nearest();
                inputTemp.ClampToEdge();
                GLTexture2D.Unbind();

                GLTexture2D outputTemp = new GLTexture2D(PixelInternalFormat.Rgba32f);
                outputTemp.Bind();
                outputTemp.SetData(IntPtr.Zero, PixelFormat.Rgba, Width, Height);
                outputTemp.Nearest();
                outputTemp.ClampToEdge();
                GLTexture2D.Unbind();

                GLTexture2D vz = new GLTexture2D(PixelInternalFormat.Rgba32f);
                vz.Bind();
                vz.SetData(IntPtr.Zero, PixelFormat.Rgba, width + 1, height + 1);
                vz.Nearest();
                vz.ClampToEdge();
                GLTexture2D.Unbind();

                PreShader.Use();

                //bind internal 32f output
                inputTemp.Bind();
                inputTemp.BindAsImage(0, true, true);

                //bind incoming input
                input.Bind();
                input.BindAsImage(1, true, true);

                //bind source
                source?.Bind();
                source?.BindAsImage(2, true, true);
                
                //bind internal vs
                vz.Bind();
                vz.BindAsImage(3, true, true);

                PreShader.SetUniform("width", (float)width);
                PreShader.SetUniform("height", (float)height);

                PreShader.SetUniform("sourceOnly", sourceMode);
                PreShader.SetUniform("maxDistance", Distance);

                //stage 1 convert input range 
                //into proper format
                //for dt
                PreShader.SetUniform("stage", (int)0);
                IGL.Primary.DispatchCompute(width / 8, height / 8, 1);
                IGL.Primary.MemoryBarrier((int)MemoryBarrierFlags.AllBarrierBits);

                Shader.Use();

                //bind internal 32f output
                outputTemp.Bind();
                outputTemp.BindAsImage(0, true, true);

                //bind internal 32f input
                inputTemp.Bind();
                inputTemp.BindAsImage(1, true, true);

                //bind source
                source?.Bind();
                source?.BindAsImage(2, true, true);

                //bind vz
                vz.Bind();
                vz.BindAsImage(3, true, true);

                Shader.SetUniform("width", (float)width);
                Shader.SetUniform("height", (float)height);

                Shader.SetUniform("sourceOnly", sourceMode);
                Shader.SetUniform("maxDistance", Distance);

                //stage 2 run column transform
                Shader.SetUniform("stage", (int)0);
                IGL.Primary.DispatchCompute(width / 8, 1, 1);
                IGL.Primary.MemoryBarrier((int)MemoryBarrierFlags.AllBarrierBits);

                //stage 3 run row transform
                Shader.SetUniform("stage", (int)1);
                IGL.Primary.DispatchCompute(height / 8, 1, 1);
                IGL.Primary.MemoryBarrier((int)MemoryBarrierFlags.AllBarrierBits);

                PreShader.Use();

                //bind actual output
                outputBuff.Bind();
                outputBuff.BindAsImage(4, true, true);

                //bind actual input
                input.Bind();
                input.BindAsImage(1, true, true);

                //bind source
                source?.Bind();
                source?.BindAsImage(2, true, true);

                //bind vz
                vz.Bind();
                vz.BindAsImage(3, true, true);

                PreShader.SetUniform("width", (float)width);
                PreShader.SetUniform("height", (float)height);

                PreShader.SetUniform("sourceOnly", sourceMode);
                PreShader.SetUniform("maxDistance", Distance);

                //stage 4 finalize with sqrt() etc
                PreShader.SetUniform("stage", (int)1);
                IGL.Primary.DispatchCompute(width / 8, height / 8, 1);
                IGL.Primary.MemoryBarrier((int)MemoryBarrierFlags.AllBarrierBits);

                GLTexture2D.UnbindAsImage(0);
                GLTexture2D.UnbindAsImage(1);
                GLTexture2D.UnbindAsImage(2);
                GLTexture2D.UnbindAsImage(3);
                GLTexture2D.Unbind();

                inputTemp?.Dispose();
                outputTemp?.Dispose();
                vz?.Dispose();
            }
        }
    }
}
