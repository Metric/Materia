using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class TextProcessor : ImageProcessor
    {
        public MVector Translation { get; set; }
        public MVector Scale { get; set; }
        public float Angle { get; set; }
        public MVector Pivot { get; set; }

        public void Prepare(int width, int height, GLTexture2D tex, GLTexture2D output)
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

        public void ProcessCharacter(int width, int height, GLTexture2D tex, GLTexture2D output)
        {
            IGL.Primary.BlendEquationSeparate((int)BlendEquationMode.FuncAdd, (int)BlendEquationMode.FuncAdd);
            IGL.Primary.BlendFunc((int)BlendingFactor.SrcAlpha, (int)BlendingFactor.OneMinusSrcAlpha);

            ApplyTransformNoAuto(tex, output, width, height, Translation, Scale, Angle, Pivot);
        }
    }
}
