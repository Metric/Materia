using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class TranslateMatrixNode : MatrixNode
    {
        public TranslateMatrixNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            name = "Translate Matrix";
            input.Type = NodeType.Float2;
            input.Name = "Position";
        }

        protected override string GetMatrixPart(string inputId)
        {
            string matCode = "mat4(1, 0, 0, 0,"
                                 + "0, 1, 0, 0,"
                                 + "0, 0, 1, 0,"
                                 + inputId + ".x, " + inputId + ".y, 0, 1)";
            return matCode;
        }

        protected override void CalculateMatrix(object o)
        {
            if (o is MVector)
            {
                MVector p = (MVector)o;
                matrix = Matrix4.CreateTranslation(p.X, p.Y, 0);
            }
        }
    }
}
