using System;
using System.Collections.Generic;
using System.Linq;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering
{
    public class Transform
    {
        public Transform Parent { get; protected set; }

        private List<Transform> children;
        public List<Transform> Children
        {
            get
            {
                return children;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                if (Parent != null)
                {
                    return LocalRotation * Parent.Rotation;
                }

                return LocalRotation;
            }
            set
            {
                if (Parent != null)
                {
                    Quaternion pcopy = Parent.Rotation;
                    pcopy.Conjugate();
                    LocalRotation = value * pcopy;
                }
            }
        }

        private Vector3 localEulerAngles;

        public Vector3 LocalEulerAngles
        {
            get
            {
                return localEulerAngles;
            }
            set
            {
                localEulerAngles = value;
                Vector3 angles = value * MathHelper.Deg2Rad;
                LocalRotation = Quaternion.FromEulerAngles(angles);
            }
        }

        public Quaternion LocalRotation { get; set; }

        public Vector3 Scale
        {
            get
            {
                if (Parent != null)
                {
                    return LocalScale * Parent.Scale;
                }

                return LocalScale;
            }
        }

        public Vector3 LocalScale { get; set; }

        public Vector3 Position
        {
            get
            {
                if (Parent != null)
                {
                    return LocalPosition + Parent.Position;
                }

                return LocalPosition;
            }
            set
            {
                if (Parent != null)
                {
                    LocalPosition = value - Parent.Position;
                    return;
                }

                LocalPosition = value;
            }
        }

        public Vector3 LocalPosition { get; set; }

        public Matrix4 WorldMatrix
        {
            get
            {
                return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
            }
        }

        public Matrix4 LocalMatrix
        {
            get
            {
                return Matrix4.CreateScale(LocalScale) * Matrix4.CreateFromQuaternion(LocalRotation) * Matrix4.CreateTranslation(LocalPosition);
            }
        }

        public Vector3 Forward
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * Vector3.UnitZ;
            }
        }

        public Vector3 Back
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * -Vector3.UnitZ;
            }
        }

        public Vector3 Left
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * -Vector3.UnitX;
            }
        }

        public Vector3 Right
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * Vector3.UnitX;
            }
        }

        public Vector3 Up
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * Vector3.UnitY;
            }
        }

        public Vector3 Down
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * -Vector3.UnitY;
            }
        }

        public Transform(Transform parent = null)
        {
            Parent = parent;
            children = new List<Transform>();
            LocalScale = new Vector3(1, 1, 1);
            LocalPosition = Vector3.Zero;
            LocalRotation = Quaternion.Identity;
        }

        public void Add(Transform c)
        {
            if(c.Parent != null)
            {
                c.Parent.Remove(c);
            }
            c.Parent = this;
            children.Add(c);
        }

        public void Remove(Transform c)
        {
            children.Remove(c);
            c.Parent = null;
        }
    }
}
