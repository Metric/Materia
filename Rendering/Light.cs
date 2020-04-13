using Materia.Rendering.Mathematics;

namespace Materia.Rendering
{
    public class Light : Transform
    {
        public Vector3 Color { get; set; }
        public float Power { get; set; }

        public Light()
        {
            Power = 1.0f;
            Color = new Vector3(1.0f);
        }
    }
}
