using System;
using System.Collections.Generic;

namespace Materia.Nodes.Items
{
    public class PinNode : ItemNode
    {
        public PinNode()
        {
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
