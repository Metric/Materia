using System;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Attributes;

namespace Materia.Rendering
{
    public class Camera : Transform
    {
        public event Action<Camera> Update;

        public float Aspect { get; set; } = 1;

        protected float fov;
        [Editable(ParameterInputType.FloatInput, "Fov")]
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
                Update?.Invoke(this);
            }
        }

        protected float near = 0.03f;
        [Editable(ParameterInputType.FloatInput, "Near")]
        public float Near
        {
            get
            {
                return near;
            }
            set
            {
                near = value;
                if (near <= 0) near = 0.03f;
                Update?.Invoke(this);
            }
        }

        protected float far = 1000f;
        [Editable(ParameterInputType.FloatInput, "Far")]
        public float Far
        {
            get
            {
                return far;
            }
            set
            {
                far = value;
                if (far <= 0) far = 1000f;
                Update?.Invoke(this);
            }
        }

        [Editable(ParameterInputType.Float3Input, "Rotation", min: 0, max: 360)]
        public MVector EulerRotation
        {
            get => new MVector(LocalEulerAngles) % 360f;
            set
            {
                LocalEulerAngles = value.ToVector3();
                Update?.Invoke(this);
            }
        }

        public Matrix4 OrthographicWithSize(float width, float height)
        {
            return Matrix4.CreateOrthographic(width,height,Near,Far);
        }

        public Matrix4 Orthographic
        {
            get
            {
                return Matrix4.CreateOrthographic(Math.Max(1, Position.Z), Math.Max(1, Position.Z / Aspect), Near, Far);
            }
        }

        public Matrix4 Perspective
        {
            get
            {
                return Matrix4.CreatePerspectiveFieldOfView(Fov * MathHelper.Deg2Rad, Aspect, Near, Far);
            }
        }

        public Matrix4 View
        {
            get
            {
                return (Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position)).Inverted();
            }
        }

        public Matrix4 OrbitView
        {
            get
            {
                return (Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(Rotation)).Inverted();
            }
        }

        public Vector3 OrbitEyePosition
        {
            get
            {
                return (OrbitView * new Vector4(Position, 1)).Xyz;
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
