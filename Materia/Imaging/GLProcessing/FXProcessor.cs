using Materia.Shaders;
using Materia.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Materia.MathHelpers;
using Materia.Nodes.Atomic;

namespace Materia.Imaging.GLProcessing
{
    public class FXProcessor : ImageProcessor
    {
        GLShaderProgram shader;

        public FXPivot Pivot { get; set; }
        public MVector Translation { get; set; }
        public MVector Scale { get; set; }
        public float Angle { get; set; }

        public FXProcessor() : base()
        {
            shader = GetShader("image.glsl", "image-basic.glsl");
        }

        public void Prepare(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);
            //renable blending!
            //make sure blending func is
            //srcalpha, oneminussrcalpha
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public void Process(int quadrant, int width, int height, GLTextuer2D tex, GLTextuer2D output, int quads)
        {
            if (shader != null)
            {
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

                float mw2 = mw * 0.5f;
                float mh2 = mh * 0.5f;

                if(quads <= 1)
                {
                    mw2 = 0;
                    mh2 = 0;
                }
                else if(quads == 2)
                {
                    mh2 = 0;
                }

                MVector pivotPoint = new MVector();
                MVector t = new MVector();

                if (quadrant == 0)
                {
                    t = Translation - new MVector(mw2, mh2);
                }
                else if(quadrant == 1)
                {
                    t = Translation + new MVector(mw2, -mh2);
                }
                else if(quadrant == 2)
                {
                    if (quads > 2)
                    {
                        t = Translation + new MVector(-mw2, mh2);
                    }
                    else
                    {
                        t = Translation + new MVector(mw2, -mh2);
                    }
                }
                else
                {
                    t = Translation + new MVector(mw2, mh2);
                }

                switch (Pivot)
                {
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
                        break;
                    case FXPivot.MinX:
                        pivotPoint.X = -0.25f;
                        break;
                    case FXPivot.MaxY:
                        pivotPoint.Y = 0.25f;
                        break;
                    case FXPivot.MinY:
                        pivotPoint.Y = -0.25f;
                        break;
                }

                ApplyTransform(tex, output, width, height, t, Scale * fp, Angle, pivotPoint);
            }
        }

        public override void Release()
        {

        }
    }
}
