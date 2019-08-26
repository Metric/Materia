using Materia.Shaders;
using Materia.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;
using Materia.MathHelpers;
using Materia.Nodes.Atomic;
using Materia.Math3D;

namespace Materia.Imaging.GLProcessing
{
    public class FXProcessor : ImageProcessor
    {
        public FXPivot Pivot { get; set; }
        public MVector Translation { get; set; }
        public MVector Scale { get; set; }
        public float Angle { get; set; }
        public FXBlend Blending { get; set; }

        IGLProgram shader;

        public FXProcessor() : base()
        {
            Blending = FXBlend.Blend;
            shader = GetShader("image.glsl", "fx.glsl");
        }

        public void Prepare(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);
        }

        public void Blend(GLTextuer2D piece, GLTextuer2D whole)
        {
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit);

            if (shader != null)
            {
                shader.Use();
                shader.SetUniform("blendMode", (int)Blending);
                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                piece.Bind();
                shader.SetUniform("Background", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                whole.Bind();

                if(renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();

                Blit(whole, whole.Width, whole.Height);
            }
        }

        public void Process(int quadrant, int width, int height, GLTextuer2D tex, GLTextuer2D output, int quads)
        {
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit);

            float mw = width * 0.5f;
            float mh = height * 0.5f;

            if(quads <= 1)
            {
                mw = width;
                mh = height;
            }
            else if(quads == 2)
            {
                mh = height;
            }

            //apply new quadrant iteration

            //calculate new width and height for quad
            float wp = (float)mw / (float)tex.Width;
            float hp = (float)mh / (float)tex.Height;

            float fp = wp < hp ? wp : hp;

            MVector pivotPoint = new MVector();
            MVector quadOffset = new MVector();

            float qx = 0.25f;
            float qy = 0.25f;

            if(quads <= 1)
            {
                qx = 0;
                qy = 0;
            }
            else if(quads <= 2)
            {
                qy = 0;
            }

            if(quadrant == 0)
            {
                quadOffset.X = -qx;
                quadOffset.Y = -qy;
            }
            else if(quadrant == 1)
            {
                quadOffset.X = qx;
                quadOffset.Y = -qy;
            }
            else if(quadrant == 2)
            {
                if(quads <= 2)
                {
                    quadOffset.X = qx;
                }
                else
                {
                    quadOffset.X = -qx;
                }

                quadOffset.Y = qy;
            }
            else
            {
                quadOffset.X = qx;
                quadOffset.Y = qy;
            }

            switch (Pivot)
            {
                case FXPivot.Center:
                    pivotPoint.X = 0f;
                    pivotPoint.Y = 0f;
                    break;
                case FXPivot.Max:
                    pivotPoint.X = 0.25f;
                    pivotPoint.Y = 0.25f;
                    break;
                case FXPivot.Min:
                    pivotPoint.X = -0.25f;
                    pivotPoint.Y = -0.25f;
                    break;
                case FXPivot.MaxX:
                    pivotPoint.X = 0.25f;
                    pivotPoint.Y = 0f;
                    break;
                case FXPivot.MinX:
                    pivotPoint.X = -0.25f;
                    pivotPoint.Y = 0f;
                    break;
                case FXPivot.MaxY:
                    pivotPoint.X = 0f;
                    pivotPoint.Y = 0.25f;
                    break;
                case FXPivot.MinY:
                    pivotPoint.Y = -0.25f;
                    pivotPoint.X = 0f;
                    break;
            }

            ApplyTransform(tex, output, width, height, Translation + quadOffset, Scale * fp, Angle, pivotPoint);
        }

        public override void Release()
        {
            
        }
    }
}
