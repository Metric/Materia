using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class ShearMatrixNode : MatrixNode
    {
        public ShearMatrixNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            name = "Shear Matrix";
            input.Type = NodeType.Float2;
            input.Name = "Shear";
        }

        protected override string GetMatrixPart(string inputId)
        {
            string matCode = "mat4(1, " + inputId + ".y, 0, 0,"
                                 + inputId + ".x, 1, 0, 0," 
                                 + "0, 0, 1, 0,"
                                 + "0, 0, 0, 1)";
            return matCode;
        }

        protected override void CalculateMatrix(object o)
        {
            if (o is MVector)
            {
                MVector p = (MVector)o;
                matrix = Matrix4.Identity;
                matrix.Row0 = new Vector4(1, p.Y, 0, 0);
                matrix.Row1 = new Vector4(p.X, 1, 0, 0);
            }
        }
    }
}
