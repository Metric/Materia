using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class ScaleMatrixNode : MatrixNode
    {
        public ScaleMatrixNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            name = "Scale Matrix";
            input.Type = NodeType.Float2;
            input.Name = "Scale";
        }

        protected override string GetMatrixPart(string inputId)
        {
            string matCode = "mat4(" + inputId + ".x, 0, 0, 0,"
                                 + "0, " + inputId + ".y, 0, 0,"
                                 + "0, 0, 1, 0,"
                                 + "0, 0, 0, 1)";
            return matCode;
        }

        protected override void CalculateMatrix(object o)
        {
            if (o is MVector)
            {
                MVector p = (MVector)o;
                matrix = Matrix4.CreateScale(p.X, p.Y, 1);
            }
        }
    }
}
