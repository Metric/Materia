using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Rendering
{
    public class Camerai : Transformi
    {
        public event Action<Camerai> Update;

        public float Aspect { get; set; }

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

        protected float near;
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
                if (near == 0) near = 0.0001f;
                Update?.Invoke(this);
            }
        }

        protected float far;
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
                if (far == 0) far = 0.0001f;
                Update?.Invoke(this);
            }
        }

        public Matrix4d OrthographicWithSize(float width, float height)
        {
            return Matrix4d.CreateOrthographic(width, height, Near, Far);
        }

        public Matrix4d Orthographic
        {
            get
            {
                return Matrix4d.CreateOrthographic(Math.Max(1, Position.Z), Math.Max(1, Position.Z / Aspect), Near, Far);
            }
        }

        public Matrix4d Perspective
        {
            get
            {
                return Matrix4d.CreatePerspectiveFieldOfView(Fov * ((float)Math.PI / 180.0f), Aspect, Near, Far);
            }
        }

        public Matrix4d View
        {
            get
            {
                return (Matrix4d.CreateFromQuaternion(Rotation) * Matrix4d.CreateTranslation(Position.X, Position.Y, Position.Z)).Inverted();
            }
        }

        public virtual Vector3i EyePosition
        {
            get
            {
                return Position;
            }
        }

        public Camerai(Transformi p = null) : base(p)
        {
            near = 0.03f;
            far = 1000f;
            fov = 40;
            Aspect = 1;
        }
    }
}
