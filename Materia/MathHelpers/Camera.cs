using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Materia.Nodes.Attributes;

namespace Materia.MathHelpers
{
    public class Camera : Transform
    {
        [HideProperty]
        public float Aspect { get; set; }

        public float Fov { get; set; }
        public float Near { get; set; }
        public float Far { get; set; }

        [HideProperty]
        public Matrix4 Orthographic
        {
            get
            {
                return Matrix4.CreateOrthographic(Math.Max(1, Position.Z), Math.Max(1, Position.Z / Aspect), Near, Far);
            }
        }

        [HideProperty]
        public Matrix4 Perspective
        {
            get
            {
                return Matrix4.CreatePerspectiveFieldOfView(Fov * ((float)Math.PI / 180.0f), Aspect, Near, Far);
            }
        }

        [HideProperty]
        public Matrix4 View
        {
            get
            {
                return Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(-Position);
            }
        }

        [HideProperty]
        public Vector3 EyePosition
        {
            get
            {
                return Vector3.Normalize((View * new Vector4(0, 0, 1, 1)).Xyz) * Position.Z;
            }
        }

        public Camera(Transform p = null) : base(p)
        {
            Near = 0.03f;
            Far = 1000f;
            Fov = 40;
            Aspect = 1;
        }
    }
}
