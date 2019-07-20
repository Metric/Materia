using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Materia.Nodes.Items;

namespace Materia.UI.Helpers
{
    public static class NodeExtensions
    {
        public static Color GetColor(this PinNode n)
        {
            System.Drawing.Color c = n.GetSystemColor();
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
