using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.GLInterfaces;
using Materia.MathHelpers;

namespace Materia.Imaging.GLProcessing
{
    public class TextProcessor : ImageProcessor
    {
        public MVector Translation { get; set; }
        public MVector Scale { get; set; }
        public float Angle { get; set; }
        public MVector Pivot { get; set; }

        public void Prepare(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            //renable blending!
            IGL.Primary.Enable((int)EnableCap.Blend);
        }

        public override void Complete()
        {
            base.Complete();

            IGL.Primary.BlendEquationSeparate((int)BlendEquationMode.FuncAdd, (int)BlendEquationMode.FuncAdd);
            IGL.Primary.BlendFunc((int)BlendingFactor.SrcAlpha, (int)BlendingFactor.OneMinusSrcAlpha);
        }

        public void ProcessCharacter(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            IGL.Primary.BlendEquationSeparate((int)BlendEquationMode.FuncAdd, (int)BlendEquationMode.FuncAdd);
            IGL.Primary.BlendFunc((int)BlendingFactor.SrcAlpha, (int)BlendingFactor.OneMinusSrcAlpha);

            ApplyTransformNoAuto(tex, output, width, height, Translation, Scale, Angle, Pivot);
        }
    }
}
