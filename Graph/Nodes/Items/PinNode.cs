
using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Items
{
    public class PinNode : ItemNode
    {
        public PinNode()
        {
            defaultName = name = "Pin";

            //just storing the color as a content string
            content = "255,255,255,255";
        }

        public void SetColor(Vector4 c)
        {
            int a = (int)(c.w * 255) & 255;
            int r = (int)(c.x * 255) & 255;
            int g = (int)(c.y * 255) & 255;
            int b = (int)(c.z * 255) & 255;

            content = string.Format("{0},{1},{2},{3}", a, r, g, b);
        }

        public Vector4 GetColor()
        {
            var split = content.Split(',');
            int a = int.Parse(split[0]);
            int r = int.Parse(split[1]);
            int g = int.Parse(split[2]);
            int b = int.Parse(split[3]);

            return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }
    }
}
