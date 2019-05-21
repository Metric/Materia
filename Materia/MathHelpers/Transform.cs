using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Materia.Nodes.Attributes;

namespace Materia.MathHelpers
{
    public class Transform
    {
        [HideProperty]
        public Transform Parent { get; protected set; }

        private List<Transform> children;
        public List<Transform> Children
        {
            get
            {
                return children.ToList();
            }
        }

        [HideProperty]
        public Quaternion Rotation
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.Rotation * LocalRotation;
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

        [HideProperty]
        public Vector3 LocalEulerAngles
        {
            get
            {
                return localEulerAngles;
            }
            set
            {
                localEulerAngles = value;
                Vector3 angles = value * ((float)Math.PI / 180.0f);
                LocalRotation = Quaternion.FromEulerAngles(angles);
            }
        }

        [HideProperty]
        public Quaternion LocalRotation { get; set; }

        [HideProperty]
        public Vector3 Scale
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.Scale * LocalScale;
                }

                return LocalScale;
            }
        }

        public Vector3 LocalScale { get; set; }

        [HideProperty]
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

        [HideProperty]
        public Vector3 LocalPosition { get; set; }

        public Matrix4 WorldMatrix
        {
            get
            {
                return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
            }
        }

        [HideProperty]
        public Matrix4 LocalMatrix
        {
            get
            {
                return Matrix4.CreateScale(LocalScale) * Matrix4.CreateFromQuaternion(LocalRotation) * Matrix4.CreateTranslation(LocalPosition);
            }
        }

        [HideProperty]
        public Vector3 Forward
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * Vector3.UnitZ;
            }
        }

        [HideProperty]
        public Vector3 Right
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * Vector3.UnitX;
            }
        }

        [HideProperty]
        public Vector3 Up
        {
            get
            {
                Quaternion rot = Rotation;
                rot.Conjugate();
                return rot * Vector3.UnitY;
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
