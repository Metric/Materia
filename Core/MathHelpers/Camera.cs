using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;
using Materia.Nodes.Attributes;

namespace Materia.MathHelpers
{
    public class Camera : Transform
    {
        public delegate void CameraChange(Camera sender);
        public event CameraChange OnCameraChanged;

        [HideProperty]
        public float Aspect { get; set; }

        protected float fov;
        public float Fov
        {
            get
            {
                return fov;
            }
            set
            {
                fov = value;
                if (fov < 1) fov = 1;
                if (OnCameraChanged != null)
                {
                    OnCameraChanged.Invoke(this);
                }
            }
        }

        protected float near;
        public float Near
        {
            get
            {
                return near;
            }
            set
            {
                near = value;
                if (near == 0) near = 0.0001f;
                if (OnCameraChanged != null)
                {
                    OnCameraChanged.Invoke(this);
                }
            }
        }

        protected float far;
        public float Far
        {
            get
            {
                return far;
            }
            set
            {
                far = value;
                if (far == 0) far = 0.0001f;
                if(OnCameraChanged != null)
                {
                    OnCameraChanged.Invoke(this);
                }
            }
        }

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
        public virtual Vector3 EyePosition
        {
            get
            {
                return Vector3.Normalize((View * new Vector4(0, 0, 1, 1)).Xyz) * Position.Z;
            }
        }

        public Camera(Transform p = null) : base(p)
        {
            near = 0.03f;
            far = 1000f;
            fov = 40;
            Aspect = 1;
        }
    }
}
