using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes;

namespace Materia.Graph
{
    public class Image : Graph
    {
        public Image(string name, ushort w = 256, ushort h = 256) : base (name, w, h)
        {

        }

        public Image(string name, ushort w = 256, ushort h = 256, Graph parent = null) : base(name, w, h)
        {
            parentGraph = parent;
        }

        public override bool Add(Node n)
        {
            if(n is ImageNode)
            {
                return base.Add(n);
            }
            else if(n is ItemNode)
            {
                return base.Add(n);
            }

            return false;
        }

        public override Node CreateNode(string type)
        {
            //math based nodes are not allowed on image graphs
            if (type.Contains("MathNodes") && !type.Contains(System.IO.Path.DirectorySeparatorChar) && !type.Contains("Materia::Layer::")) return null;

            return base.CreateNode(type);
        }
    }
}
