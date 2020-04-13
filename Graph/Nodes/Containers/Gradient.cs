using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Containers
{
    public class Gradient
    {
        public float[] positions;
        public MVector[] colors;

        public Gradient()
        {
            positions = new float[] { 0, 1 };
            colors = new MVector[] { new MVector(0, 0, 0, 1), new MVector(1, 1, 1, 1) };
        }
    }
}
