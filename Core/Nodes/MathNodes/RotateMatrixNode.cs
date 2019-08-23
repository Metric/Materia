using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Helpers;
using Materia.Math3D;

namespace Materia.Nodes.MathNodes
{
    public class RotateMatrixNode : MatrixNode
    {
        public RotateMatrixNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w,h,p)
        {
            name = "Rotate Matrix";
            input.Type = NodeType.Float;
            input.Name = "Degrees";
        }

        protected override string GetMatrixPart(string inputId)
        {
            string inputCode = inputId + " * Deg2Rad";
            string matCode = "mat4(cos(" + inputCode + "), -sin(" + inputCode + "), 0, 0,"
                                 + "sin(" + inputCode + "), cos(" + inputCode + "), 0, 0,"
                                 + "0,0,1,0,"
                                 + "0,0,0,1)";
            return matCode;
        }

        protected override void CalculateMatrix(object o)
        {
            if(Utils.IsNumber(o))
            {
                float p = Convert.ToSingle(o);
                matrix = Matrix4.CreateRotationZ(p * (float)(Math.PI / 180.0f)); 
            }
        }
    }
}
