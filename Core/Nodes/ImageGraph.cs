using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes
{
    public class ImageGraph : Graph
    {
        public ImageGraph(string name, int w = 256, int h = 256) : base (name, w, h)
        {

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
            if (type.Contains("MathNodes") && !type.Contains(System.IO.Path.DirectorySeparatorChar)) return null;

            return base.CreateNode(type);
        }
    }
}
