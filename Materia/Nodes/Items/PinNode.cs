using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Materia.Nodes.Items
{
    public class PinNode : ItemNode
    {
        public PinNode()
        {
            Id = Guid.NewGuid().ToString();
            Outputs = new List<NodeOutput>();
            Inputs = new List<NodeInput>();
            name = "Pin";
            //just storing the color as a content string
            content = "255,255,255,255";
        }

        public void SetSystemColor(System.Drawing.Color c)
        {
            int a = c.A;
            int r = c.R;
            int g = c.G;
            int b = c.B;

            content = string.Format("{0},{1},{2},{3}", a, r, g, b);
        }

        public Color GetColor()
        {
            var split = content.Split(',');
            byte a = byte.Parse(split[0]);
            byte r = byte.Parse(split[1]);
            byte g = byte.Parse(split[2]);
            byte b = byte.Parse(split[3]);

            return Color.FromArgb(a, r, g, b);
        }

        public System.Drawing.Color GetSystemColor()
        {
            var split = content.Split(',');
            int a = int.Parse(split[0]);
            int r = int.Parse(split[1]);
            int g = int.Parse(split[2]);
            int b = int.Parse(split[3]);

            return System.Drawing.Color.FromArgb(a, r, g, b);
        }
    }
}
